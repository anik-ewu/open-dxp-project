using Microsoft.AspNetCore.Authorization;
using OpenDXP.Application.Common.Security;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OpenDXP.Api.Authorization;

public class PageOwnershipAuthorizationHandler : AuthorizationHandler<PageOwnershipRequirement, IOwnedResource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PageOwnershipRequirement requirement, IOwnedResource resource)
    {
        if (context.User.IsInRole(Roles.Admin))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var subject = context.User.FindFirst(Claims.Subject)?.Value;
        if (Guid.TryParse(subject, out var userId) && userId == resource.OwnerId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
