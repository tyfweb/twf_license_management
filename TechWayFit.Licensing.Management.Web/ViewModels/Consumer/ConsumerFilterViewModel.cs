namespace TechWayFit.Licensing.WebUI.ViewModels.Consumer
{
    /// <summary>
    /// Consumer filter view model
    /// </summary>
    public class ConsumerFilterViewModel
    {
        public string SearchTerm { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
    }
}
