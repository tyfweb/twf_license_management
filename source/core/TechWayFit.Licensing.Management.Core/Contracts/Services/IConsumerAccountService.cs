using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Consumer;

namespace TechWayFit.Licensing.Management.Core.Contracts.Services;

/// <summary>
/// Service interface for managing consumer accounts
/// </summary>
public interface IConsumerAccountService
{
    /// <summary>
    /// Creates a new consumer account
    /// </summary>
    /// <param name="consumerAccount">Consumer account to create</param>
    /// <param name="createdBy">User creating the account</param>
    /// <returns>Created consumer account</returns>
    Task<ConsumerAccount> CreateConsumerAccountAsync(ConsumerAccount consumerAccount, string createdBy);

    /// <summary>
    /// Updates an existing consumer account
    /// </summary>
    /// <param name="consumerAccount">Consumer account to update</param>
    /// <param name="updatedBy">User updating the account</param>
    /// <returns>Updated consumer account</returns>
    Task<ConsumerAccount> UpdateConsumerAccountAsync(ConsumerAccount consumerAccount, string updatedBy);

    /// <summary>
    /// Gets a consumer account by ID
    /// </summary>
    /// <param name="consumerId">Consumer ID</param>
    /// <returns>Consumer account or null if not found</returns>
    Task<ConsumerAccount?> GetConsumerAccountByIdAsync(Guid consumerId);

    /// <summary>
    /// Gets a consumer account by email address
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>Consumer account or null if not found</returns>
    Task<ConsumerAccount?> GetConsumerAccountByEmailAsync(string email);

    /// <summary>
    /// Gets all consumer accounts with optional filtering
    /// </summary>
    /// <param name="status">Filter by consumer status</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="searchTerm">Search term for name or email</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of consumer accounts</returns>
    Task<IEnumerable<ConsumerAccount>> GetConsumerAccountsAsync(
        ConsumerStatus? status = null,
        bool? isActive = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 50);

    /// <summary>
    /// Gets the total count of consumer accounts with optional filtering
    /// </summary>
    /// <param name="status">Filter by consumer status</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="searchTerm">Search term for name or email</param>
    /// <returns>Total count</returns>
    Task<int> GetConsumerAccountCountAsync(
        ConsumerStatus? status = null,
        bool? isActive = null,
        string? searchTerm = null);

    /// <summary>
    /// Activates a consumer account
    /// </summary>
    /// <param name="consumerId">Consumer ID</param>
    /// <param name="activatedBy">User activating the account</param>
    /// <returns>True if activated successfully</returns>
    Task<bool> ActivateConsumerAccountAsync(Guid consumerId, string activatedBy);

    /// <summary>
    /// Deactivates a consumer account
    /// </summary>
    /// <param name="consumerId">Consumer ID</param>
    /// <param name="deactivatedBy">User deactivating the account</param>
    /// <param name="reason">Reason for deactivation</param>
    /// <returns>True if deactivated successfully</returns>
    Task<bool> DeactivateConsumerAccountAsync(Guid consumerId, string deactivatedBy, string reason);

    /// <summary>
    /// Updates consumer status
    /// </summary>
    /// <param name="consumerId">Consumer ID</param>
    /// <param name="status">New status</param>
    /// <param name="updatedBy">User updating the status</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> UpdateConsumerStatusAsync(Guid consumerId, ConsumerStatus status, string updatedBy);

    /// <summary>
    /// Deletes a consumer account
    /// </summary>
    /// <param name="consumerId">Consumer ID</param>
    /// <param name="deletedBy">User deleting the account</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteConsumerAccountAsync(Guid consumerId, string deletedBy);

    /// <summary>
    /// Checks if a consumer account exists
    /// </summary>
    /// <param name="consumerId">Consumer ID</param>
    /// <returns>True if exists</returns>
    Task<bool> ConsumerAccountExistsAsync(Guid consumerId);

    /// <summary>
    /// Validates consumer account data
    /// </summary>
    /// <param name="consumerAccount">Consumer account to validate</param>
    /// <returns>Validation result with any errors</returns>
    Task<ValidationResult> ValidateConsumerAccountAsync(ConsumerAccount consumerAccount);

    /// <summary>
    /// Gets consumer accounts managed by a specific account manager
    /// </summary>
    /// <param name="accountManagerId">Account manager ID</param>
    /// <param name="status">Filter by consumer status</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of consumer accounts managed by the account manager</returns>
    Task<IEnumerable<ConsumerAccount>> GetConsumersByAccountManagerAsync(
        Guid accountManagerId,
        ConsumerStatus? status = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 50);

    /// <summary>
    /// Gets the count of consumer accounts managed by a specific account manager
    /// </summary>
    /// <param name="accountManagerId">Account manager ID</param>
    /// <param name="status">Filter by consumer status</param>
    /// <param name="isActive">Filter by active status</param>
    /// <returns>Total count of consumers managed by the account manager</returns>
    Task<int> GetConsumerCountByAccountManagerAsync(
        Guid accountManagerId,
        ConsumerStatus? status = null,
        bool? isActive = null);

    /// <summary>
    /// Gets consumer accounts that have licenses for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="status">Filter by consumer status</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="licenseStatus">Filter by license status</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of consumer accounts with licenses for the product</returns>
    Task<IEnumerable<ConsumerAccount>> GetConsumersByProductAsync(
        Guid productId,
        ConsumerStatus? status = null,
        bool? isActive = null,
        string? licenseStatus = null,
        int pageNumber = 1,
        int pageSize = 50);

    /// <summary>
    /// Gets the count of consumer accounts that have licenses for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="status">Filter by consumer status</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="licenseStatus">Filter by license status</param>
    /// <returns>Total count of consumers with licenses for the product</returns>
    Task<int> GetConsumerCountByProductAsync(
        Guid productId,
        ConsumerStatus? status = null,
        bool? isActive = null,
        string? licenseStatus = null);

    /// <summary>
    /// Gets consumer accounts linked to both a specific account manager and product
    /// </summary>
    /// <param name="accountManagerId">Account manager ID</param>
    /// <param name="productId">Product ID</param>
    /// <param name="status">Filter by consumer status</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="licenseStatus">Filter by license status</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of consumer accounts managed by the account manager with licenses for the product</returns>
    Task<IEnumerable<ConsumerAccount>> GetConsumersByAccountManagerAndProductAsync(
        Guid accountManagerId,
        Guid productId,
        ConsumerStatus? status = null,
        bool? isActive = null,
        string? licenseStatus = null,
        int pageNumber = 1,
        int pageSize = 50);

    /// <summary>
    /// Gets the count of consumer accounts linked to both a specific account manager and product
    /// </summary>
    /// <param name="accountManagerId">Account manager ID</param>
    /// <param name="productId">Product ID</param>
    /// <param name="status">Filter by consumer status</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="licenseStatus">Filter by license status</param>
    /// <returns>Total count of consumers managed by the account manager with licenses for the product</returns>
    Task<int> GetConsumerCountByAccountManagerAndProductAsync(
        Guid accountManagerId,
        Guid productId,
        ConsumerStatus? status = null,
        bool? isActive = null,
        string? licenseStatus = null);

    /// <summary>
    /// Gets consumer accounts that don't have any licenses for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="status">Filter by consumer status</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of consumer accounts without licenses for the product</returns>
    Task<IEnumerable<ConsumerAccount>> GetConsumersWithoutProductAsync(
        Guid productId,
        ConsumerStatus? status = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 50);

    /// <summary>
    /// Gets consumer accounts managed by an account manager that don't have licenses for a specific product
    /// </summary>
    /// <param name="accountManagerId">Account manager ID</param>
    /// <param name="productId">Product ID</param>
    /// <param name="status">Filter by consumer status</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of consumer accounts managed by the account manager without licenses for the product</returns>
    Task<IEnumerable<ConsumerAccount>> GetConsumersByAccountManagerWithoutProductAsync(
        Guid accountManagerId,
        Guid productId,
        ConsumerStatus? status = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 50);
}
