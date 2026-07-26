using Microsoft.AspNetCore.Identity;

namespace OpenDXP.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
}
