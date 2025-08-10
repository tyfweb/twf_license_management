using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Core.Models.Workflow;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Workflow;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Workflow;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.Workflow;

/// <summary>
/// Entity Framework implementation of workflow history repository
/// </summary>
public class EfCoreWorkflowHistoryRepository : BaseRepository<WorkflowHistoryEntry, WorkflowHistoryEntity>, IWorkflowHistoryRepository
{
    public EfCoreWorkflowHistoryRepository(EfCoreLicensingDbContext context, IUserContext userContext) 
        : base(context, userContext)
    {
    }

    public async Task<IEnumerable<WorkflowHistoryEntry>> GetByEntityIdAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .Where(h => h.EntityId == entityId && h.IsActive && !h.IsDeleted)
            .OrderByDescending(h => h.ActionDate)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.Map());
    }

    public async Task<IEnumerable<WorkflowHistoryEntry>> GetByEntityTypeAsync(string entityType, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .Where(h => h.EntityType == entityType && h.IsActive && !h.IsDeleted)
            .OrderByDescending(h => h.ActionDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.Map());
    }

    public async Task<IEnumerable<WorkflowHistoryEntry>> GetByActionByAsync(string actionBy, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .Where(h => h.ActionBy == actionBy && h.IsActive && !h.IsDeleted)
            .OrderByDescending(h => h.ActionDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.Map());
    }

    public Task<WorkflowHistoryEntry> RecordActionAsync(WorkflowHistoryEntry historyEntry, CancellationToken cancellationToken = default)
    {
        var entity = new WorkflowHistoryEntity();
        entity.Map(historyEntry);
        entity.Id = Guid.NewGuid();
        entity.IsActive = true;
        entity.IsDeleted = false;
        entity.CreatedBy = _userContext.UserName ?? "Anonymous";
        entity.CreatedOn = DateTime.UtcNow;
        entity.UpdatedBy = _userContext.UserName ?? "Anonymous";
        entity.UpdatedOn = DateTime.UtcNow;
        
        _dbSet.Add(entity);
        return Task.FromResult(entity.Map());
    }

    // Implement missing IBaseRepository methods for workflow history specific behavior
    public async Task<WorkflowHistoryEntry> UpdateAsync(WorkflowHistoryEntry model, CancellationToken cancellationToken = default)
    {
        // Workflow history entries are typically immutable after creation
        // Only allow updates to comments and metadata
        var entity = await _dbSet.FindAsync(new object[] { model.Id }, cancellationToken);
        if (entity == null)
        {
            throw new ArgumentException($"WorkflowHistoryEntry with ID {model.Id} not found", nameof(model));
        }

        entity.Comments = model.Comments;
        entity.Metadata = model.Metadata;
        entity.UpdatedBy = _userContext.UserName ?? "Anonymous";
        entity.UpdatedOn = DateTime.UtcNow;

        _dbSet.Update(entity);
        return entity.Map();
    }

    public async Task<bool> SoftDeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        if (entity == null)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.DeletedBy = deletedBy;
        entity.DeletedOn = DateTime.UtcNow;
        entity.UpdatedBy = _userContext.UserName ?? deletedBy;
        entity.UpdatedOn = DateTime.UtcNow;

        _dbSet.Update(entity);
        return true;
    }

    public async Task<IEnumerable<WorkflowHistoryEntry>> GetAllWithIncludesAsync(params System.Linq.Expressions.Expression<Func<WorkflowHistoryEntry, object>>[] includeProperties)
    {
        // Workflow history entries don't typically have navigation properties to include
        return await GetAllAsync(cancellationToken: CancellationToken.None);
    }

    protected override IQueryable<WorkflowHistoryEntity> SearchQuery(IQueryable<WorkflowHistoryEntity> query, string searchQuery)
    {
        return query.Where(h =>
            h.EntityType.Contains(searchQuery) ||
            h.ActionBy.Contains(searchQuery) ||
            (h.Comments != null && h.Comments.Contains(searchQuery)));
    }
}
