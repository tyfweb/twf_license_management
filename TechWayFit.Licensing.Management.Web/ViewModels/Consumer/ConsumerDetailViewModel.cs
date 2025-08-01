using TechWayFit.Licensing.Management.Core.Models.Consumer;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Consumer
{
    /// <summary>
    /// Consumer detail view model
    /// </summary>
    public class ConsumerDetailViewModel
    {
        public ConsumerAccount Consumer { get; set; } = new();
        public List<LicenseSummaryViewModel> Licenses { get; set; } = new();
        public ConsumerStatisticsViewModel Statistics { get; set; } = new();
    }
}
