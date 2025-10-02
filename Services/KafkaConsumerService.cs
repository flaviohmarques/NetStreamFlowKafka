using Confluent.Kafka;
using NetStreamFlowKafka.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace NetStreamFlowKafka.Services;

public class KafkaConsumerService : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly KafkaSettings _settings;
    private readonly IEventStore _eventStore;
    private readonly ILogger<KafkaConsumerService> _logger;

    public KafkaConsumerService(
        IOptions<KafkaSettings> settings,
        IEventStore eventStore,
        ILogger<KafkaConsumerService> logger)
    {
        _settings = settings.Value;
        _eventStore = eventStore;
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false
        };

        _consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, error) =>
            {
                _logger.LogError("Kafka consumer error: {Reason}", error.Reason);
            })
            .Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_settings.TopicName);
        _logger.LogInformation("Kafka consumer started, listening to topic: {Topic}", _settings.TopicName);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(1));

                    if (consumeResult != null)
                    {
                        await ProcessMessageAsync(consumeResult, stoppingToken);

                        // Commit offset after successful processing
                        _consumer.StoreOffset(consumeResult);
                        _consumer.Commit(consumeResult);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message: {Error}", ex.Error.Reason);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in consumer loop");
                }

                // Small delay to prevent tight loop
                await Task.Delay(100, stoppingToken);
            }
        }
        finally
        {
            _consumer.Close();
            _consumer.Dispose();
            _logger.LogInformation("Kafka consumer stopped");
        }
    }

    private async Task ProcessMessageAsync(ConsumeResult<string, string> consumeResult, CancellationToken cancellationToken)
    {
        try
        {
            var userEvent = JsonSerializer.Deserialize<UserActivityEvent>(consumeResult.Message.Value);

            if (userEvent != null)
            {
                await _eventStore.AddEventAsync(userEvent);

                _logger.LogInformation(
                    "Processed event: {EventId} - {EventType} for User: {UserId} from Partition: {Partition}, Offset: {Offset}",
                    userEvent.EventId, userEvent.EventType, userEvent.UserId,
                    consumeResult.Partition.Value, consumeResult.Offset.Value);
            }
            else
            {
                _logger.LogWarning("Failed to deserialize event from Partition: {Partition}, Offset: {Offset}",
                    consumeResult.Partition.Value, consumeResult.Offset.Value);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in message at Partition: {Partition}, Offset: {Offset}",
                consumeResult.Partition.Value, consumeResult.Offset.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message at Partition: {Partition}, Offset: {Offset}",
                consumeResult.Partition.Value, consumeResult.Offset.Value);
        }
    }
}