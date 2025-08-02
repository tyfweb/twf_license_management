namespace TechWayFit.Licensing.Management.Core.Models.Consumer;

public class ConsumerAccount
{
    public ConsumerAccount()
    {
        PrimaryContact = new ContactPerson();
        SecondaryContact = new ContactPerson();
        Address = new Address();
    }
    /// <summary>
    /// Unique identifier for the consumer
    /// </summary>
    public string ConsumerId { get; set; } = string.Empty;
    /// <summary>
    ///  Company or organization name
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;
    /// <summary>
    /// Primary contact information for the consumer
    /// </summary>
    public ContactPerson PrimaryContact { get; set; } = new();
    /// <summary>
    /// Secondary contact information for the consumer (optional)
    /// </summary>
    public ContactPerson? SecondaryContact { get; set; }

    /// <summary>
    /// Date when the consumer was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Date when the consumer was activated
    /// </summary>
    public DateTime ActivatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Indicates if the consumer is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    /// <summary>
    /// Address of the consumer
    /// </summary>
    public Address Address { get; set; } = new();
    /// <summary>
    /// Additional notes or comments about the consumer
    /// </summary>
    public string Notes { get; set; } = string.Empty;
    /// <summary>
    /// Status of the consumer account
    /// </summary>
    public ConsumerStatus Status { get; set; } = ConsumerStatus.Prospect;
}
