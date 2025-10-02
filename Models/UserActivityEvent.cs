namespace NetStreamFlowKafka.Models;

public class UserActivityEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string>? Metadata { get; set; }
}

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string TopicName { get; set; } = "user-activity-events";
    public string GroupId { get; set; } = "event-processor-group";
}