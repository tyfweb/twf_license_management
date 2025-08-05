using System;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Web.Helpers;

namespace TechWayFit.Licensing.Management.Web.Extensions;

public static class ModelConverterExtensions
{
    public static Consumer ConvertToConsumerModel(this ConsumerAccount consumer)
    {
        if (consumer == null) throw new ArgumentNullException(nameof(consumer));

        return new Consumer
        {
            ConsumerId = consumer.ConsumerId.ConvertToString(),
            OrganizationName = consumer.CompanyName,
            ContactPerson = consumer.PrimaryContact?.Name ?? string.Empty,
            ContactEmail = consumer.PrimaryContact?.Email ?? string.Empty,
            SecondaryContactPerson = consumer.SecondaryContact?.Name,
            SecondaryContactEmail = consumer.SecondaryContact?.Email,
            Address = consumer.Address.ToString(),
            IsActive = consumer.IsActive,
            CreatedAt = consumer.CreatedAt
        };
    }
    public static ProductConfiguration ConvertToProductConfiguration(this EnterpriseProduct product)
    {
        if (product == null) throw new ArgumentNullException(nameof(product));

        return new ProductConfiguration
        {
            ProductId = product.ProductId.ConvertToString(),
            ProductName = product.Name,
            ProductType = ProductType.EnterpriseMonitoring,
            Version = product.Version,
            IsActive = product.Status == ProductStatus.Active,
            CreatedAt = product.ReleaseDate,
            UpdatedAt = product.ReleaseDate
        };
    }
     public static License ConvertToLicenseModel(this ProductLicense license)
    {
        return new License
        {
            LicenseId = license.LicenseId.ToString(),
            ProductId = license.LicenseConsumer.Product.ProductId.ToString(),
            // Update this line to use the correct property from LicenseConsumer, e.g. LicenseConsumer.Consumer.ConsumerId if Consumer is a property of ProductConsumer
            ConsumerId = license.LicenseConsumer.Consumer.ConsumerId.ToString(),
            ValidFrom = license.ValidFrom,
            ValidTo = license.ValidTo,
            CreatedAt = license.CreatedAt,
            Status = license.Status,
            RevokedAt = license.RevokedAt,
            RevocationReason = license.RevocationReason,
            Metadata = license.Metadata,
            FeaturesIncluded = license.LicenseConsumer.Features.Select(f => new LicenseFeature
            {
                Id = f.FeatureId.ToString(),
                Name = f.Name,
                Description = f.Description,
                IsCurrentlyValid = f.IsEnabled
            }).ToList(),
            LicensedTo = license.LicenseConsumer.Consumer.CompanyName,
            ContactEmail = license.LicenseConsumer.Consumer.PrimaryContact.Email,
            ContactPerson = license.LicenseConsumer.Consumer.PrimaryContact.Name,
            CreatedBy = license.IssuedBy,
            IssuedAt = license.KeyGeneratedAt,
            Issuer = license.IssuedBy,
            SecondaryContactEmail = license.LicenseConsumer.Consumer.SecondaryContact?.Email,
            SecondaryContactPerson = license.LicenseConsumer.Consumer.SecondaryContact?.Name,
            Version = license.LicenseConsumer.Product.Version,
            ProductVersion = SemanticVersion.Parse(license.LicenseConsumer.Product.Version).ToVersion(),
            //MaxSupportedVersion = SemanticVersion.Parse(LicenseConsumer.Product.MaxSupportedVersion).ToVersion(),
           //Tier = LicenseConsumer.Product.Tier,
        };
    }
}
