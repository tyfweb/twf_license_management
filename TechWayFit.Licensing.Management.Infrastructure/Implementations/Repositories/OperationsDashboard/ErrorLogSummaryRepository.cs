using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.OperationsDashboard;
using TechWayFit.Licensing.Management.Infrastructure.Data;
using TechWayFit.Licensing.Management.Infrastructure.Data.Context;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;

namespace TechWayFit.Licensing.Management.Infrastructure.Implementations.Repositories.OperationsDashboard;

/// <summary>
/// Repository implementation for error log summary operations
/// </summary>
public class ErrorLogSummaryRepository : OperationsBaseRepository<ErrorLogSummaryEntity>,
        IErrorLogSummaryRepository
{
    
    public ErrorLogSummaryRepository(LicensingDbContext context)
        : base(context)
    { 
    }

    protected override string GetIdPropertyName()
    {
        return nameof(ErrorLogSummaryEntity.ErrorSummaryId);
    }

    public async Task<ErrorLogSummaryEntity?> GetByMessageHashAsync(string messageHash)
    {
        if (string.IsNullOrWhiteSpace(messageHash))
            throw new ArgumentException("Message hash cannot be null or empty", nameof(messageHash));

        return await _dbSet
            .Where(e => e.IsActive)
            .FirstOrDefaultAsync(e => e.ErrorMessageHash == messageHash);
    }

    public async Task<IEnumerable<ErrorLogSummaryEntity>> GetByErrorTypeAsync(string errorType, DateTime startTime, DateTime endTime)
    {
        if (string.IsNullOrWhiteSpace(errorType))
            throw new ArgumentException("Error type cannot be null or empty", nameof(errorType));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(e => e.ErrorType == errorType)
            .Where(e => e.TimestampHour >= startTime && e.TimestampHour <= endTime)
            .OrderByDescending(e => e.LastOccurrence)
            .ToListAsync();
    }

    public async Task<IEnumerable<ErrorLogSummaryEntity>> GetBySourceAsync(string source, DateTime startTime, DateTime endTime)
    {
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Source cannot be null or empty", nameof(source));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(e => e.ErrorSource == source)
            .Where(e => e.TimestampHour >= startTime && e.TimestampHour <= endTime)
            .OrderByDescending(e => e.LastOccurrence)
            .ToListAsync();
    }

    public async Task<IEnumerable<ErrorLogSummaryEntity>> GetBySeverityAsync(string severity, DateTime startTime, DateTime endTime)
    {
        if (string.IsNullOrWhiteSpace(severity))
            throw new ArgumentException("Severity cannot be null or empty", nameof(severity));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(e => e.SeverityLevel == severity)
            .Where(e => e.TimestampHour >= startTime && e.TimestampHour <= endTime)
            .OrderByDescending(e => e.LastOccurrence)
            .ToListAsync();
    }

    public async Task<IEnumerable<ErrorLogSummaryEntity>> GetTopByOccurrenceCountAsync(int topCount, DateTime startTime, DateTime endTime)
    {
        if (topCount <= 0)
            throw new ArgumentException("Top count must be greater than zero", nameof(topCount));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(e => e.TimestampHour >= startTime && e.TimestampHour <= endTime)
            .OrderByDescending(e => e.OccurrenceCount)
            .Take(topCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<ErrorLogSummaryEntity>> GetTopByAffectedUsersAsync(int topCount, DateTime startTime, DateTime endTime)
    {
        if (topCount <= 0)
            throw new ArgumentException("Top count must be greater than zero", nameof(topCount));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(e => e.TimestampHour >= startTime && e.TimestampHour <= endTime)
            .OrderByDescending(e => e.AffectedUsers)
            .Take(topCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<ErrorLogSummaryEntity>> GetUnresolvedErrorsAsync()
    {
        return await _dbSet
            .Where(e => e.IsActive)
            .Where(e => !e.IsResolved)
            .OrderByDescending(e => e.LastOccurrence)
            .ToListAsync();
    }

    public async Task<IEnumerable<ErrorLogSummaryEntity>> GetRecentErrorsAsync(int hours)
    {
        if (hours <= 0)
            throw new ArgumentException("Hours must be greater than zero", nameof(hours));

        var cutoffTime = DateTime.UtcNow.AddHours(-hours);
        
        return await _dbSet
            .Where(e => e.IsActive)
            .Where(e => e.LastOccurrence >= cutoffTime)
            .OrderByDescending(e => e.LastOccurrence)
            .ToListAsync();
    }

    public async Task<int> GetTotalOccurrenceCountAsync(DateTime startTime, DateTime endTime)
    {
        return await _dbSet
            .Where(e => e.IsActive)
            .Where(e => e.TimestampHour >= startTime && e.TimestampHour <= endTime)
            .SumAsync(e => e.OccurrenceCount);
    }

    public async Task<int> GetUniqueErrorCountAsync(DateTime startTime, DateTime endTime)
    {
        return await _dbSet
            .Where(e => e.IsActive)
            .Where(e => e.TimestampHour >= startTime && e.TimestampHour <= endTime)
            .CountAsync();
    }

    public async Task<int> GetUnresolvedCountAsync(DateTime startTime, DateTime endTime)
    {
        return await _dbSet
            .Where(e => e.IsActive)
            .Where(e => !e.IsResolved)
            .Where(e => e.TimestampHour >= startTime && e.TimestampHour <= endTime)
            .CountAsync();
    }

    public async Task<int> GetTotalAffectedUsersAsync(DateTime startTime, DateTime endTime)
    {
        return await _dbSet
            .Where(e => e.IsActive)
            .Where(e => e.TimestampHour >= startTime && e.TimestampHour <= endTime)
            .SumAsync(e => e.AffectedUsers);
    }

    public async Task<ErrorLogSummaryEntity> IncrementOccurrenceAsync(string messageHash)
    {
        if (string.IsNullOrWhiteSpace(messageHash))
            throw new ArgumentException("Message hash cannot be null or empty", nameof(messageHash));

        var existingError = await GetByMessageHashAsync(messageHash);
        
        if (existingError == null)
        {
            throw new InvalidOperationException($"Error with message hash '{messageHash}' not found");
        }

        existingError.OccurrenceCount++;
        existingError.LastOccurrence = DateTime.UtcNow;
        existingError.UpdatedOn = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingError;
    }

    public async Task<ErrorLogSummaryEntity> MarkAsResolvedAsync(string messageHash, string resolvedBy)
    {
        if (string.IsNullOrWhiteSpace(messageHash))
            throw new ArgumentException("Message hash cannot be null or empty", nameof(messageHash));
        
        if (string.IsNullOrWhiteSpace(resolvedBy))
            throw new ArgumentException("Resolved by cannot be null or empty", nameof(resolvedBy));

        var existingError = await GetByMessageHashAsync(messageHash);
        
        if (existingError == null)
        {
            throw new InvalidOperationException($"Error with message hash '{messageHash}' not found");
        }

        existingError.IsResolved = true;
        existingError.ResolvedBy = resolvedBy;
        existingError.ResolvedOn = DateTime.UtcNow;
        existingError.UpdatedBy = resolvedBy;
        existingError.UpdatedOn = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingError;
    }
}
