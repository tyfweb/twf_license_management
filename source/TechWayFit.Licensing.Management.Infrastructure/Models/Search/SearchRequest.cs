using System.Linq.Expressions;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Search;

public class SearchRequest<TEntity> where TEntity : class
{
    public string? Query { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public IList<Expression<Func<TEntity, bool>>> Filters { get; set; } = new List<Expression<Func<TEntity, bool>>>();
}
