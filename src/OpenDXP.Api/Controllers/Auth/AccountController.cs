using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OpenDXP.Application.Common.Auditing;
using OpenDXP.Infrastructure.Identity;

namespace OpenDXP.Api.Controllers.Auth;

/// <summary>
/// Minimal server-rendered login page. The OpenIddict authorization endpoint redirects here
/// when the browser has no active session yet; Angular never talks to this controller directly.
/// </summary>
[ApiController]
[Route("account")]
public class AccountController(SignInManager<ApplicationUser> signInManager, IAuditLogService auditLog) : ControllerBase
{
    [HttpGet("login")]
    public IActionResult Login([FromQuery] string? returnUrl)
    {
        return Content(BuildLoginPage(returnUrl, error: null), "text/html");
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> LoginPost(
        [FromForm] string email, [FromForm] string password, [FromForm] string? returnUrl)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await signInManager.PasswordSignInAsync(email, password, isPersistent: false, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            var eventType = result.IsLockedOut ? "AccountLocked" : "LoginFailed";
            await auditLog.LogAsync(eventType, email, $"Login attempt for '{email}' failed.", ipAddress);

            var error = result.IsLockedOut ? "Account locked. Try again later." : "Invalid email or password.";
            return Content(BuildLoginPage(returnUrl, error), "text/html");
        }

        await auditLog.LogAsync("LoginSucceeded", email, $"'{email}' signed in.", ipAddress);
        return Redirect(returnUrl ?? "/");
    }

    private static string BuildLoginPage(string? returnUrl, string? error)
    {
        var encodedReturnUrl = HtmlEncoder.Default.Encode(returnUrl ?? "/");
        var errorHtml = error is null
            ? string.Empty
            : $"""<p class="error">{HtmlEncoder.Default.Encode(error)}</p>""";

        return $$"""
                <!doctype html>
                <html lang="en">
                <head>
                  <meta charset="utf-8" />
                  <meta name="viewport" content="width=device-width, initial-scale=1" />
                  <title>Sign in - OpenDXP</title>
                  <style>
                    :root { color-scheme: light; }
                    * { box-sizing: border-box; }
                    body {
                      margin: 0;
                      min-height: 100vh;
                      display: flex;
                      align-items: center;
                      justify-content: center;
                      background: #f8fafc;
                      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
                      color: #0f172a;
                    }
                    .card {
                      width: 100%;
                      max-width: 360px;
                      margin: 24px;
                      padding: 32px;
                      background: #fff;
                      border: 1px solid #e2e8f0;
                      border-radius: 12px;
                      box-shadow: 0 1px 2px rgba(15, 23, 42, 0.04), 0 8px 24px rgba(15, 23, 42, 0.06);
                    }
                    .brand {
                      display: flex;
                      align-items: center;
                      gap: 10px;
                      margin-bottom: 24px;
                    }
                    .brand-mark {
                      display: flex;
                      align-items: center;
                      justify-content: center;
                      width: 32px;
                      height: 32px;
                      border-radius: 8px;
                      background: #4f46e5;
                      color: #fff;
                      font-size: 13px;
                      font-weight: 700;
                    }
                    .brand-name {
                      font-size: 16px;
                      font-weight: 600;
                    }
                    h1 {
                      margin: 0 0 20px;
                      font-size: 20px;
                      font-weight: 600;
                    }
                    .error {
                      margin: 0 0 16px;
                      padding: 10px 12px;
                      border: 1px solid #fecaca;
                      background: #fef2f2;
                      color: #b91c1c;
                      border-radius: 8px;
                      font-size: 13px;
                    }
                    label {
                      display: block;
                      margin-bottom: 14px;
                      font-size: 13px;
                      font-weight: 500;
                      color: #334155;
                    }
                    input {
                      display: block;
                      width: 100%;
                      margin-top: 6px;
                      padding: 9px 12px;
                      font-size: 14px;
                      border: 1px solid #cbd5e1;
                      border-radius: 8px;
                      background: #fff;
                      color: #0f172a;
                    }
                    input:focus {
                      outline: none;
                      border-color: #6366f1;
                      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.15);
                    }
                    button {
                      width: 100%;
                      margin-top: 6px;
                      padding: 10px 12px;
                      font-size: 14px;
                      font-weight: 600;
                      color: #fff;
                      background: #4f46e5;
                      border: none;
                      border-radius: 8px;
                      cursor: pointer;
                    }
                    button:hover {
                      background: #4338ca;
                    }
                  </style>
                </head>
                <body>
                  <div class="card">
                    <div class="brand">
                      <span class="brand-mark">DX</span>
                      <span class="brand-name">OpenDXP</span>
                    </div>
                    <h1>Sign in</h1>
                    {{errorHtml}}
                    <form method="post">
                      <input type="hidden" name="returnUrl" value="{{encodedReturnUrl}}" />
                      <label>
                        Email
                        <input name="email" type="email" autocomplete="email" required autofocus />
                      </label>
                      <label>
                        Password
                        <input name="password" type="password" autocomplete="current-password" required />
                      </label>
                      <button type="submit">Log in</button>
                    </form>
                  </div>
                </body>
                </html>
                """;
    }
}
