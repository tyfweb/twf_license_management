namespace TechWayFit.Licensing.Management.Core.Contracts;

/// <summary>
/// Interface for accessing current user context information
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Gets the current user's unique identifier
    /// </summary>
    string? UserId { get; }

    string? TenantId { get; }

    /// <summary>
    /// Gets the current user's email
    /// </summary>
    string? UserEmail { get; }

    /// <summary>
    /// Gets the current user's display name
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Gets the current user's roles
    /// </summary>
    IEnumerable<string> UserRoles { get; }

    /// <summary>
    /// Checks if the current user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Checks if the current user has a specific role
    /// </summary>
    /// <param name="role">The role to check</param>
    /// <returns>True if user has the role, false otherwise</returns>
    bool HasRole(string role);

    /// <summary>
    /// Gets a claim value for the current user
    /// </summary>
    /// <param name="claimType">The claim type to retrieve</param>
    /// <returns>The claim value or null if not found</returns>
    string? GetClaimValue(string claimType);
}
