namespace TechWayFit.Licensing.Management.Core.Models.Consumer;

public class Address
{
    /// <summary>
    /// Street address of the consumer
    /// </summary>
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// City of the consumer
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// State or region of the consumer
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Postal code of the consumer
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Country of the consumer
    /// </summary>
    public string Country { get; set; } = string.Empty;
}
