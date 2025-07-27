namespace TechWayFit.Licensing.WebUI.ViewModels.Consumer
{
    /// <summary>
    /// Consumer detail view model
    /// </summary>
    public class ConsumerDetailViewModel
    {
        public Core.Models.Consumer Consumer { get; set; } = new();
        public List<LicenseSummaryViewModel> Licenses { get; set; } = new();
        public ConsumerStatisticsViewModel Statistics { get; set; } = new();
    }
}
