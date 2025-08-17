using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common;
using TechWayFit.Licensing.Management.Core.Models.License;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.License;

/// <summary>
/// Repository interface for ProductActivation entities
/// </summary>
public interface IProductActivationRepository : IDataRepository<ProductActivation>
{
    /// <summary>
    /// Gets an activation by its signature
    /// </summary>
    /// <param name="signature">Activation signature</param>
    /// <returns>ProductActivation entity if found</returns>
    Task<ProductActivation?> GetBySignatureAsync(string signature);

    /// <summary>
    /// Gets an activation by product key and machine ID
    /// </summary>
    /// <param name="productKey">Product key</param>
    /// <param name="machineId">Machine ID</param>
    /// <returns>ProductActivation entity if found</returns>
    Task<ProductActivation?> GetByProductKeyAndMachineAsync(string productKey, string machineId);

    /// <summary>
    /// Gets all activations for a product key
    /// </summary>
    /// <param name="productKey">Product key</param>
    /// <returns>List of activations</returns>
    Task<IEnumerable<ProductActivation>> GetByProductKeyAsync(string productKey);

    /// <summary>
    /// Gets activation configuration by license ID
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <returns>ProductActivation entity with configuration</returns>
    Task<ProductActivation?> GetConfigurationByLicenseIdAsync(Guid licenseId);

    /// <summary>
    /// Counts active activations for a product key
    /// </summary>
    /// <param name="productKey">Product key</param>
    /// <returns>Number of active activations</returns>
    Task<int> CountActiveActivationsByProductKeyAsync(string productKey);

    /// <summary>
    /// Gets activations by license ID
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <returns>List of activations</returns>
    Task<IEnumerable<ProductActivation>> GetByLicenseIdAsync(Guid licenseId);

    /// <summary>
    /// Gets activations that haven't sent heartbeat within specified time
    /// </summary>
    /// <param name="timeSpan">Time span for heartbeat check</param>
    /// <returns>List of stale activations</returns>
    Task<IEnumerable<ProductActivation>> GetStaleActivationsAsync(TimeSpan timeSpan);

    /// <summary>
    /// Updates heartbeat for an activation
    /// </summary>
    /// <param name="activationId">Activation ID</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> UpdateHeartbeatAsync(Guid activationId);

    /// <summary>
    /// Gets activations by status
    /// </summary>
    /// <param name="status">Activation status</param>
    /// <returns>List of activations with specified status</returns>
    Task<IEnumerable<ProductActivation>> GetByStatusAsync(Core.Models.Enums.ProductActivationStatus status);
}
