using Microsoft.AspNetCore.Authorization;

namespace OpenDXP.Api.Authorization;

/// <summary>Satisfied if the current user owns the resource, or is an Admin.</summary>
public class PageOwnershipRequirement : IAuthorizationRequirement
{
}
