namespace TechWayFit.Licensing.WebUI.ViewModels.Consumer
{
    /// <summary>
    /// Pagination view model
    /// </summary>
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 10;
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
