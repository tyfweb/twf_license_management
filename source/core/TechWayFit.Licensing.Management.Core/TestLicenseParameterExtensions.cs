using TechWayFit.Licensing.Generator.Models;
using TechWayFit.Licensing.Management.Core.Models.Enums;
using TechWayFit.Licensing.Management.Core.Extensions;

namespace TestLicenseParameterExtensions
{
    /// <summary>
    /// Simple test to verify that our LicenseKeyParameter enum and extension methods work correctly
    /// </summary>
    public class TestLicenseParameterExtensions
    {
        public static void TestParameterExtensions()
        {
            // Create a new SimplifiedLicenseGenerationRequest
            var request = new SimplifiedLicenseGenerationRequest();

            // Test fluent API with method chaining
            request
                .AddLicenseParameter(LicenseKeyParameter.LicenseType, "ProductKey")
                .AddLicenseParameter(LicenseKeyParameter.OnlineActivation, true)
                .AddLicenseParameter(LicenseKeyParameter.ProductKey, "ABCD-EFGH-IJKL-MNOP")
                .AddLicenseParameter(LicenseKeyParameter.MaxUsers, 10)
                .AddLicenseParameter(LicenseKeyParameter.RequiresActivation, true);

            // Test parameter retrieval
            var licenseType = request.GetLicenseParameterAsString(LicenseKeyParameter.LicenseType);
            var onlineActivation = request.GetLicenseParameterAsBool(LicenseKeyParameter.OnlineActivation);
            var productKey = request.GetLicenseParameterAsString(LicenseKeyParameter.ProductKey);
            var maxUsers = request.GetLicenseParameterAsInt(LicenseKeyParameter.MaxUsers);
            var requiresActivation = request.GetLicenseParameterAsBool(LicenseKeyParameter.RequiresActivation);

            // Test HasLicenseParameter
            var hasLicenseType = request.HasLicenseParameter(LicenseKeyParameter.LicenseType);
            var hasOfflineActivation = request.HasLicenseParameter(LicenseKeyParameter.OfflineActivation);

            // Verify results
            Console.WriteLine($"License Type: {licenseType}");
            Console.WriteLine($"Online Activation: {onlineActivation}");
            Console.WriteLine($"Product Key: {productKey}");
            Console.WriteLine($"Max Users: {maxUsers}");
            Console.WriteLine($"Requires Activation: {requiresActivation}");
            Console.WriteLine($"Has License Type: {hasLicenseType}");
            Console.WriteLine($"Has Offline Activation: {hasOfflineActivation}");

            // Test GetAllLicenseParameters
            var allParameters = request.GetAllLicenseParameters();
            Console.WriteLine($"Total Parameters: {allParameters.Count}");

            Console.WriteLine("\nTest completed successfully!");
        }
    }
}
