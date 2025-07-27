namespace TechWayFit.Licensing.WebUI.ViewModels.Consumer
{
    /// <summary>
    /// Consumer summary view model
    /// </summary>
    public class ConsumerSummaryViewModel
    {
        public string ConsumerId { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int TotalLicenses { get; set; }
        public int ActiveLicenses { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
