using TechWayFit.Licensing.Core.Contracts;
using TechWayFit.Licensing.Core.Models;
using Microsoft.Extensions.Logging;

namespace TechWayFit.Licensing.Generator.Services
{
    /// <summary>
    /// Service for managing consumers
    /// </summary>
    public class ConsumerService : IConsumerService
    {
        private readonly IConsumerRepository _consumerRepository;
        private readonly ILogger<ConsumerService> _logger;

        public ConsumerService(IConsumerRepository consumerRepository, ILogger<ConsumerService> logger)
        {
            _consumerRepository = consumerRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Consumer>> GetConsumersByProductAsync(string productId)
        {
            return await _consumerRepository.GetConsumersByProductAsync(productId);
        }

        public async Task<Consumer?> GetConsumerAsync(string consumerId)
        {
            return await _consumerRepository.GetConsumerByIdAsync(consumerId);
        }

        public async Task<Consumer> CreateConsumerAsync(Consumer consumer)
        {
            if (string.IsNullOrEmpty(consumer.ConsumerId))
                consumer.ConsumerId = Guid.NewGuid().ToString();

            return await _consumerRepository.CreateConsumerAsync(consumer);
        }

        public async Task<Consumer> UpdateConsumerAsync(Consumer consumer)
        {
            return await _consumerRepository.UpdateConsumerAsync(consumer);
        }

        public async Task<IEnumerable<ConsumerLicenseInfo>> GetConsumerLicensesAsync(string consumerId)
        {
            var consumer = await _consumerRepository.GetConsumerByIdAsync(consumerId);
            return consumer?.Licenses ?? new List<ConsumerLicenseInfo>();
        }

        public async Task<IEnumerable<Consumer>> SearchConsumersAsync(string searchTerm)
        {
            return await _consumerRepository.SearchConsumersAsync(searchTerm);
        }
    }
}
