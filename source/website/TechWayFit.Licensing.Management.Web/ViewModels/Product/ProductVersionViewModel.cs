using System;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Product;

public class ProductVersionViewModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public required string Version { get; set; }
    public required string VersionName { get; set; }
    public DateTime ReleaseDate { get; set; }
    public DateTime? EndOfLifeDate { get; set; }
    public DateTime SupportEndDate { get; set; } 
    public required string ReleaseNotes { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsCurrent { get; set; }
    public bool CanDelete { get; set; }
}
