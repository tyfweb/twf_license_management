using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Workflow;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Products;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.Workflow;

/// <summary>
/// EntityFramework implementation of EnterpriseProduct approval repository
/// </summary>
public class EfCoreEnterpriseProductApprovalRepository : BaseRepository<EnterpriseProduct, ProductEntity>, IApprovalRepository<EnterpriseProduct>
{
    public EfCoreEnterpriseProductApprovalRepository(EfCoreLicensingDbContext context, IUserContext userContext) 
        : base(context, userContext)
    {
    }

    public async Task<IEnumerable<EnterpriseProduct>> GetByStatusAsync(EntityStatus status, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .Where(e => e.EntityStatus == (int)status && !e.IsDeleted)
            .OrderByDescending(e => e.UpdatedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.Map());
    }

    public async Task<IEnumerable<EnterpriseProduct>> GetPendingApprovalAsync(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        return await GetByStatusAsync(EntityStatus.PendingApproval, pageNumber, pageSize, cancellationToken);
    }

    public async Task<IEnumerable<EnterpriseProduct>> GetUserEntitiesAsync(string userId, EntityStatus? status = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
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

    public async Task<IEnumerable<EnterpriseProduct>> GetEntitiesRequiringApprovalAsync(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .Where(e => e.EntityStatus == (int)EntityStatus.PendingApproval && !e.IsDeleted)
            .OrderBy(e => e.SubmittedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.Map());
    }

    public async Task<IEnumerable<EnterpriseProduct>> GetApprovedEntitiesAsync(DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(e => e.EntityStatus == (int)EntityStatus.Approved && !e.IsDeleted);

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.ReviewedOn >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.ReviewedOn <= toDate.Value);
        }

        var entities = await query
            .OrderByDescending(e => e.ReviewedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.Map());
    }

    public async Task<IEnumerable<EnterpriseProduct>> GetRejectedEntitiesAsync(DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(e => e.EntityStatus == (int)EntityStatus.Rejected && !e.IsDeleted);

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.ReviewedOn >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.ReviewedOn <= toDate.Value);
        }

        var entities = await query
            .OrderByDescending(e => e.ReviewedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.Map());
    }

    public async Task<int> GetCountByStatusAsync(EntityStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(e => e.EntityStatus == (int)status && !e.IsDeleted, cancellationToken);
    }

    public async Task<Dictionary<EntityStatus, int>> GetStatusCountsAsync(CancellationToken cancellationToken = default)
    {
        var counts = await _dbSet
            .Where(e => !e.IsDeleted)
            .GroupBy(e => e.EntityStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(x => (EntityStatus)x.Status, x => x.Count);
    }

    public async Task<EnterpriseProduct> UpdateStatusAsync(Guid entityId, EntityStatus newStatus, string? comments = null, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(x => x.Id == entityId, cancellationToken);
        
        if (entity == null)
        {
            throw new InvalidOperationException($"EnterpriseProduct with ID {entityId} not found.");
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

    public async Task<int> GetPendingApprovalCountAsync(CancellationToken cancellationToken = default)
    {
        return await GetCountByStatusAsync(EntityStatus.PendingApproval, cancellationToken);
    }

    public async Task<IEnumerable<EnterpriseProduct>> GetUserPendingSubmissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .Where(e => e.SubmittedBy == userId && 
                       e.EntityStatus == (int)EntityStatus.PendingApproval && 
                       !e.IsDeleted)
            .OrderByDescending(e => e.SubmittedOn)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.Map());
    }

    public async Task<EnterpriseProduct> UpdateAsync(EnterpriseProduct entity, CancellationToken cancellationToken = default)
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
