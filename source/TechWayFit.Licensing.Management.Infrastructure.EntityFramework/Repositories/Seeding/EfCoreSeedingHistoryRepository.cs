using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Core.Models.Seeding;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Seeding;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Seeding;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.Seeding;

/// <summary>
/// EntityFramework implementation of seeding history repository
/// </summary>
public class EfCoreSeedingHistoryRepository : BaseRepository<SeedingHistory, SeedingHistoryEntity>, ISeedingHistoryRepository
{
    public EfCoreSeedingHistoryRepository(EfCoreLicensingDbContext context, IUserContext userContext)
        : base(context, userContext)
    {
    }

    public async Task<bool> IsSeederExecutedAsync(string seederName, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(s => s.SeederName == seederName && s.IsSuccessful, cancellationToken);
    }

    public async Task<SeedingHistory?> GetLatestExecutionAsync(string seederName, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet
            .Where(s => s.SeederName == seederName)
            .OrderByDescending(s => s.ExecutedOn)
            .FirstOrDefaultAsync(cancellationToken);

        return entity?.Map();
    }

    public async Task<IEnumerable<SeedingHistory>> GetExecutionHistoryAsync(string seederName, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .Where(s => s.SeederName == seederName)
            .OrderByDescending(s => s.ExecutedOn)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.Map());
    }

    public async Task<SeedingHistory> RecordExecutionAsync(
        string seederName,
        bool isSuccessful,
        int recordsCreated = 0,
        long durationMs = 0,
        string? errorMessage = null,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var seedingHistory = new SeedingHistory
        {
            SeederName = seederName,
            IsSuccessful = isSuccessful,
            RecordsCreated = recordsCreated,
            DurationMs = durationMs,
            ErrorMessage = errorMessage,
            Metadata = metadata ?? new Dictionary<string, object>(),
            ExecutedOn = DateTime.UtcNow,
            ExecutedBy = _userContext.UserName ?? "System"
        };

        return await AddAsync(seedingHistory, cancellationToken);
    }

    public async Task ClearHistoryAsync(string? seederName = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrEmpty(seederName))
        {
            query = query.Where(s => s.SeederName == seederName);
        }

        var entities = await query.ToListAsync(cancellationToken);
        _dbSet.RemoveRange(entities);
    }
}
