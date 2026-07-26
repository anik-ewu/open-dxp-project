namespace OpenDXP.Application.Common.Security;

/// <summary>
/// Implemented by anything that resource-based authorization policies can check ownership of.
/// </summary>
public interface IOwnedResource
{
    Guid OwnerId { get; }
}
