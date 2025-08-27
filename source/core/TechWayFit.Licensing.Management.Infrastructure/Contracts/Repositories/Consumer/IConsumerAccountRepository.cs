using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common; 

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Consumer;

/// <summary>
/// Repository interface for ConsumerAccount entities
/// </summary>
public interface IConsumerAccountRepository : IDataRepository<ConsumerAccount>
{
    /// <summary>
    /// Gets a consumer account by email address (searches primary and secondary contacts)
    /// </summary>
    /// <param name="email">Email address to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Consumer account if found, null otherwise</returns>
    Task<ConsumerAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
