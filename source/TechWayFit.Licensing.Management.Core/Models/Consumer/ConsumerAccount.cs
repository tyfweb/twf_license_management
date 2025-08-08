using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Core.Models.Consumer;

public class ConsumerAccount : IWorkflowCapable
{
    public ConsumerAccount()
    {
        PrimaryContact = new ContactPerson();
        SecondaryContact = new ContactPerson();
        Address = new Address();
        Audit.IsActive = true;
    }

    /// <summary>
    /// Unique identifier for the consumer
    /// </summary>
    public Guid ConsumerId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant identifier for multi-tenant isolation
    /// </summary>
    public Guid TenantId { get; set; } = Guid.Empty;

    /// <summary>
    /// Implementation of IWorkflowCapable.Id - maps to ConsumerId
    /// </summary>
    public Guid Id
    {
        get => ConsumerId;
        set => ConsumerId = value;
    }

    /// <summary>
    /// Audit information for the consumer account
    /// </summary>
    public AuditInfo Audit { get; set; } = new();

    /// <summary>
    /// Workflow information for the consumer account
    /// </summary>
    public WorkflowInfo Workflow { get; set; } = new();

    /// <summary>
    ///  Company or organization name
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Account code for the consumer
    /// </summary>
    public string? AccountCode { get; set; }

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
    public DateTime CreatedAt
    {
        get => Audit.CreatedOn;
        set => Audit.CreatedOn = value;
    }

    /// <summary>
    /// Date when the consumer was activated
    /// </summary>
    public DateTime ActivatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Subscription end date
    /// </summary>
    public DateTime? SubscriptionEnd { get; set; }

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
