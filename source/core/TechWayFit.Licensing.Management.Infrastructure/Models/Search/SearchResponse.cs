namespace TechWayFit.Licensing.Management.Infrastructure.Models.Search;

public class SearchResponse<T>
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; } 
    public IEnumerable<T> Results { get; set; } = new List<T>();

    public SearchResponse(int totalCount, int page, int pageSize,IEnumerable<T> results)
    {
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
        Results = results;
    }
}
