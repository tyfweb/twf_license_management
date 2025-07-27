namespace TechWayFit.Licensing.WebUI.ViewModels.Consumer
{
    /// <summary>
    /// Consumer listing view model
    /// </summary>
    public class ConsumerListViewModel
    {
        public string SearchTerm { get; set; } = string.Empty;
        public List<ConsumerViewModel> Consumers { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();
        public ConsumerFilterViewModel Filter { get; set; } = new();
    }
}
