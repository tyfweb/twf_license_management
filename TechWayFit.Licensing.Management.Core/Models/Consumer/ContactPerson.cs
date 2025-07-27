namespace TechWayFit.Licensing.Management.Core.Models.Consumer;

public class ContactPerson
{
    /// <summary>
    /// Name of the contact person
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the contact person
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Phone number of the contact person
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Position or title of the contact person
    /// </summary>
    public string Position { get; set; } = string.Empty;
}
