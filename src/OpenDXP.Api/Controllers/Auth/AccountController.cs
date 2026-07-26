using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenDXP.Infrastructure.Identity;

namespace OpenDXP.Api.Controllers.Auth;

/// <summary>
/// Minimal server-rendered login page. The OpenIddict authorization endpoint redirects here
/// when the browser has no active session yet; Angular never talks to this controller directly.
/// </summary>
[ApiController]
[Route("account")]
public class AccountController(SignInManager<ApplicationUser> signInManager) : ControllerBase
{
    [HttpGet("login")]
    public IActionResult Login([FromQuery] string? returnUrl)
    {
        return Content(BuildLoginPage(returnUrl, error: null), "text/html");
    }

    [HttpPost("login")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> LoginPost(
        [FromForm] string email, [FromForm] string password, [FromForm] string? returnUrl)
    {
        var result = await signInManager.PasswordSignInAsync(email, password, isPersistent: false, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            var error = result.IsLockedOut ? "Account locked. Try again later." : "Invalid email or password.";
            return Content(BuildLoginPage(returnUrl, error), "text/html");
        }

        return Redirect(returnUrl ?? "/");
    }

    private static string BuildLoginPage(string? returnUrl, string? error)
    {
        var encodedReturnUrl = HtmlEncoder.Default.Encode(returnUrl ?? "/");
        var errorHtml = error is null ? string.Empty : $"<p style=\"color:red\">{HtmlEncoder.Default.Encode(error)}</p>";

        return $"""
                <html>
                <body>
                  <h1>OpenDXP sign in</h1>
                  {errorHtml}
                  <form method="post">
                    <input type="hidden" name="returnUrl" value="{encodedReturnUrl}" />
                    <label>Email <input name="email" type="email" required /></label><br/>
                    <label>Password <input name="password" type="password" required /></label><br/>
                    <button type="submit">Log in</button>
                  </form>
                </body>
                </html>
                """;
    }
}
