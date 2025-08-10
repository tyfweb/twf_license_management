using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Workflow;
using TechWayFit.Licensing.Management.Infrastructure.Data.Entities.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.Workflow;

/// <summary>
/// EntityFramework implementation of ConsumerAccount approval repository
/// </summary>
public class EfCoreConsumerAccountApprovalRepository : BaseRepository<ConsumerAccount, ConsumerAccountEntity>, IApprovalRepository<ConsumerAccount>
{
    public EfCoreConsumerAccountApprovalRepository(EfCoreLicensingDbContext context, IUserContext userContext) 
        : base(context, userContext)
    {
    }

    public async Task<IEnumerable<ConsumerAccount>> GetByStatusAsync(EntityStatus status, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .Where(e => e.EntityStatus == (int)status && !e.IsDeleted)
            .OrderByDescending(e => e.UpdatedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.Map());
    }

    public async Task<IEnumerable<ConsumerAccount>> GetPendingApprovalAsync(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        return await GetByStatusAsync(EntityStatus.PendingApproval, pageNumber, pageSize, cancellationToken);
    }

    public async Task<IEnumerable<ConsumerAccount>> GetUserEntitiesAsync(string userId, EntityStatus? status = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(e => e.CreatedBy == userId && !e.IsDeleted);

        if (status.HasValue)
        {
            query = query.Where(e => e.EntityStatus == (int)status.Value);
        }

        var entities = await query
            .OrderByDescending(e => e.UpdatedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.Map());
    }

    public async Task<ConsumerAccount> UpdateStatusAsync(Guid entityId, EntityStatus newStatus, string? comments = null, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(x => x.Id == entityId, cancellationToken);
        
        if (entity == null)
        {
            throw new InvalidOperationException($"ConsumerAccount with ID {entityId} not found.");
        }

        // Update status and add comments
        entity.EntityStatus = (int)newStatus;
        if (!string.IsNullOrEmpty(comments))
        {
            entity.ReviewComments = comments;
        }

        // Update workflow tracking fields based on status
        switch (newStatus)
        {
            case EntityStatus.Approved:
                entity.ReviewedBy = _userContext.UserName;
                entity.ReviewedOn = DateTime.UtcNow;
                break;
            case EntityStatus.Rejected:
                entity.ReviewedBy = _userContext.UserName;
                entity.ReviewedOn = DateTime.UtcNow;
                break;
            case EntityStatus.PendingApproval:
                entity.SubmittedBy = _userContext.UserName;
                entity.SubmittedOn = DateTime.UtcNow;
                break;
        }

        // DbContext will automatically handle UpdatedBy and UpdatedOn during SaveChanges
        return entity.Map();
    }

    public async Task<int> GetCountByStatusAsync(EntityStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.EntityStatus == (int)status && !e.IsDeleted)
            .CountAsync(cancellationToken);
    }

    public async Task<int> GetPendingApprovalCountAsync(CancellationToken cancellationToken = default)
    {
        return await GetCountByStatusAsync(EntityStatus.PendingApproval, cancellationToken);
    }

    public async Task<IEnumerable<ConsumerAccount>> GetUserPendingSubmissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .Where(e => e.SubmittedBy == userId && 
                       e.EntityStatus == (int)EntityStatus.PendingApproval && 
                       !e.IsDeleted)
            .OrderByDescending(e => e.SubmittedOn)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.Map());
    }

    public async Task<ConsumerAccount> UpdateAsync(ConsumerAccount entity, CancellationToken cancellationToken = default)
    {
        await UpdateAsync(entity.Id, entity, cancellationToken);
        return entity;
    }

    public async Task<bool> SoftDeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
    {
        await DeleteAsync(id, cancellationToken);
        return true;
    }
 
}
