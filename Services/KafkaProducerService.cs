using Confluent.Kafka;
using NetStreamFlowKafka.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace NetStreamFlowKafka.Services;

public interface IKafkaProducerService
{
    Task<bool> PublishEventAsync(UserActivityEvent userEvent);
}

public class KafkaProducerService : IKafkaProducerService, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaSettings _settings;
    private readonly ILogger<KafkaProducerService> _logger;

    public KafkaProducerService(
        IOptions<KafkaSettings> settings,
        ILogger<KafkaProducerService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            Acks = Acks.Leader,
            EnableIdempotence = true,
            MaxInFlight = 5,
            MessageTimeoutMs = 10000
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler((_, error) =>
            {
                _logger.LogError("Kafka producer error: {Reason}", error.Reason);
            })
            .Build();
    }

    public async Task<bool> PublishEventAsync(UserActivityEvent userEvent)
    {
        try
        {
            var message = new Message<string, string>
            {
                Key = userEvent.UserId,
                Value = JsonSerializer.Serialize(userEvent)
            };

            var result = await _producer.ProduceAsync(_settings.TopicName, message);

            _logger.LogInformation(
                "Event published to Kafka: {EventId} - Partition: {Partition}, Offset: {Offset}",
                userEvent.EventId, result.Partition.Value, result.Offset.Value);

            return true;
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventId}: {Error}",
                userEvent.EventId, ex.Error.Reason);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error publishing event {EventId}", userEvent.EventId);
            return false;
        }
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}