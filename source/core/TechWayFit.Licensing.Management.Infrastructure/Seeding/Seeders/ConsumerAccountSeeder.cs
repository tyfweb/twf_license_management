using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.Seeding;

public class ConsumerAccountSeeder : BaseDataSeeder
{
    public ConsumerAccountSeeder(IUnitOfWork unitOfWork, ILogger<BaseDataSeeder> logger) : base(unitOfWork, logger)
    {
    }

    public override string SeederName => "ConsumerAccount";

    public override int Order => 10; // Run after basic data seeders

    protected async override Task<int> ExecuteSeedingAsync(CancellationToken cancellationToken = default)
    {
        // Create a list to store all consumer accounts
        var consumerAccounts = new List<ConsumerAccount>();

        // Add the original accounts
        var account1 = CreateConsumerAccount("TechWayFit", "contact@techwayfit.com", "123-456-7890");
        var account2 = CreateConsumerAccount("FitTech Solutions", "info@fittechsolutions.com", "098-765-4321");
        var account3 = CreateConsumerAccount("HealthSync", "support@healthsync.com", "555-123-4567");
        var account4 = CreateConsumerAccount("WellnessPro", "contact@wellnesspro.com", "444-987-6543");

        // Add original accounts to the list
        consumerAccounts.Add(account1);
        consumerAccounts.Add(account2);
        consumerAccounts.Add(account3);
        consumerAccounts.Add(account4);

        // Create 20 additional random consumer accounts
        string[] companyNames = {
            "FitnessFirst", "WellbeTech", "HealthMatic", "VitalityPlus", "CoreFitness",
            "OptiHealth", "PeakPerformance", "BalancedLife", "WellnessHub", "FitForward",
            "HealthStream", "ActiveSync", "VitalFit", "PrimeWellness", "FitFusion",
            "TechHealth", "WellnessWave", "LifeBalance", "OptiFit", "VitalTech"
        };

        for (int i = 0; i < 20; i++)
        {
            string companyName = companyNames[i % companyNames.Length];
            string email = $"contact@{companyName.ToLower()}.com";
            string phone = $"{new Random().Next(100, 999)}-{new Random().Next(100, 999)}-{new Random().Next(1000, 9999)}";
            
            var randomAccount = CreateConsumerAccount(companyName, email, phone);
            consumerAccounts.Add(randomAccount);
        }

        // Save all accounts to the database
        await _unitOfWork.Consumers.AddRangeAsync(consumerAccounts, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return consumerAccounts.Count;
    }

    public ConsumerAccount CreateConsumerAccount(string companyName,string contactEmail,string contactPhone)
    {
        var consumerAccount = new ConsumerAccount
        {
            ConsumerId = Guid.NewGuid(),
            CompanyName = companyName,
            PrimaryContact = new ContactPerson
            {
                Name = "Default Contact",
                Email = contactEmail,
                Phone = contactPhone
            },
            Address = new Address
            {
                Street = "123 Main St",
                City = "Anytown",
                State = "CA",
                PostalCode = "12345"
            }
        };
        return consumerAccount;
    }
 
}