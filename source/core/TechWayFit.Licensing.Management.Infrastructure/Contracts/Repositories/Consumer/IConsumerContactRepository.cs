using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common; 

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Consumer;

/// <summary>
/// Repository interface for ConsumerContact entities
/// </summary>
public interface IConsumerContactRepository : IDataRepository<ConsumerContact>
{
    /// <summary>
    /// Gets all contacts for a specific consumer
    /// </summary>
    /// <param name="consumerId">The consumer ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of consumer contacts</returns>
    Task<IEnumerable<ConsumerContact>> GetByConsumerIdAsync(Guid consumerId, CancellationToken cancellationToken = default);
}
