using System.Threading.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using OpenDXP.Api.Authorization;
using OpenDXP.Api.Middleware;
using OpenDXP.Application.Common.Security;
using OpenDXP.Application.Content.Commands;
using OpenDXP.Infrastructure;
using OpenDXP.Infrastructure.Identity;
using OpenDXP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

const string AuthRateLimiterPolicy = "auth";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreatePageCommand>());
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<OpenDxpDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});

builder.Services.AddOpenIddict()
    .AddServer(options =>
    {
        options.SetAuthorizationEndpointUris("connect/authorize")
               .SetTokenEndpointUris("connect/token")
               .SetUserInfoEndpointUris("connect/userinfo");

        options.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();
        options.AllowRefreshTokenFlow();

        options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles, Scopes.OfflineAccess, "opendxp-api");

        options.SetAccessTokenLifetime(TimeSpan.FromMinutes(15));
        options.SetRefreshTokenLifetime(TimeSpan.FromDays(14));

        // Development-only certificates. Production needs real signing/encryption keys (Phase 6/7 concern).
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        // UserInfo has no passthrough: OpenIddict answers it directly from the claims embedded
        // in the access token (see AuthorizationController's SetDestinations), no controller needed.
        var aspNetCoreServerBuilder = options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .EnableTokenEndpointPassthrough()
               .EnableStatusCodePagesIntegration();

        // Local HTTP-only dev containers have no TLS in front of them; never disable this in production.
        if (builder.Environment.IsDevelopment())
        {
            aspNetCoreServerBuilder.DisableTransportSecurityRequirement();
        }
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services.AddAuthorization(options =>
    options.AddPolicy(Policies.MustOwnResource, policy => policy.Requirements.Add(new PageOwnershipRequirement())));
builder.Services.AddSingleton<IAuthorizationHandler, PageOwnershipAuthorizationHandler>();

// Defense in depth alongside Identity's account lockout: caps login/token attempts per client IP,
// independent of which account is being targeted (lockout is per-account).
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(AuthRateLimiterPolicy, limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
    options.OnRejected = (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        return new ValueTask();
    };
});

const string AdminUiCorsPolicy = "AdminUi";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                      ?? ["http://localhost:4200"];
builder.Services.AddCors(options =>
    options.AddPolicy(AdminUiCorsPolicy, policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OpenDxpDbContext>();
    dbContext.Database.Migrate();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    foreach (var roleName in Roles.All)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new ApplicationRole(roleName));
        }
    }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    async Task SeedDemoUserAsync(string email, string displayName, string role)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var demoUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(demoUser, "ChangeMe123!");
        await userManager.AddToRoleAsync(demoUser, role);
    }

    await SeedDemoUserAsync("admin@opendxp.local", "Demo Admin", Roles.Admin);
    await SeedDemoUserAsync("editor@opendxp.local", "Demo Editor", Roles.Editor);
    await SeedDemoUserAsync("editor2@opendxp.local", "Demo Editor Two", Roles.Editor);
    await SeedDemoUserAsync("viewer@opendxp.local", "Demo Viewer", Roles.Viewer);

    var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
    if (await scopeManager.FindByNameAsync("opendxp-api") is null)
    {
        await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
        {
            Name = "opendxp-api",
            Resources = { "opendxp-api" }
        });
    }

    var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
    var angularAdminDescriptor = new OpenIddictApplicationDescriptor
    {
        ClientId = "angular-admin",
        ClientType = ClientTypes.Public,
        RedirectUris = { new Uri("http://localhost:4200/auth-callback") },
        PostLogoutRedirectUris = { new Uri("http://localhost:4200") },
        Permissions =
        {
            Permissions.Endpoints.Authorization,
            Permissions.Endpoints.Token,
            Permissions.GrantTypes.AuthorizationCode,
            Permissions.GrantTypes.RefreshToken,
            Permissions.ResponseTypes.Code,
            Permissions.Scopes.Email,
            Permissions.Scopes.Profile,
            Permissions.Scopes.Roles,
            Permissions.Prefixes.Scope + Scopes.OfflineAccess,
            Permissions.Prefixes.Scope + "opendxp-api"
        },
        Requirements = { Requirements.Features.ProofKeyForCodeExchange }
    };

    var existingApplication = await applicationManager.FindByClientIdAsync("angular-admin");
    if (existingApplication is null)
    {
        await applicationManager.CreateAsync(angularAdminDescriptor);
    }
    else
    {
        await applicationManager.UpdateAsync(existingApplication, angularAdminDescriptor);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors(AdminUiCorsPolicy);

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
