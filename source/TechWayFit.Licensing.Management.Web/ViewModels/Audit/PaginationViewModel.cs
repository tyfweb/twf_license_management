namespace TechWayFit.Licensing.Management.Web.ViewModels.Audit;

/// <summary>
/// Pagination helper
/// </summary>
public class PaginationViewModel
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
    public int StartIndex => Math.Max(1, CurrentPage - 2);
    public int EndIndex => Math.Min(TotalPages, CurrentPage + 2);
    public IEnumerable<int> PageNumbers => Enumerable.Range(StartIndex, EndIndex - StartIndex + 1);
}
