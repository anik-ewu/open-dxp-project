namespace OpenDXP.Application.Common.Security;

public static class Policies
{
    /// <summary>Admins may act on any resource; everyone else must own the resource.</summary>
    public const string MustOwnResource = "MustOwnResource";
}
