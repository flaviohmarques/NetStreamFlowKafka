using System.Collections.Concurrent;
using NetStreamFlowKafka.Models;

namespace NetStreamFlowKafka.Services;

public interface IEventStore
{
    ValueTask AddEventAsync(UserActivityEvent userEvent);
    ValueTask<IEnumerable<UserActivityEvent>> GetAllEventsAsync();
    ValueTask<IEnumerable<UserActivityEvent>> GetEventsByUserIdAsync(string userId);
    ValueTask<IEnumerable<UserActivityEvent>> GetEventsByTypeAsync(string eventType);
}

public class InMemoryEventStore(ILogger<InMemoryEventStore> logger) : IEventStore
{
    private readonly ConcurrentBag<UserActivityEvent> _events = [];
    private readonly ILogger<InMemoryEventStore> _logger = logger;

    public ValueTask AddEventAsync(UserActivityEvent userEvent)
    {
        _events.Add(userEvent);
        _logger.LogInformation("Event stored: {EventId} - {EventType} for User: {UserId}",
            userEvent.EventId, userEvent.EventType, userEvent.UserId);
        return ValueTask.CompletedTask;
    }

    public ValueTask<IEnumerable<UserActivityEvent>> GetAllEventsAsync()
    {
        return ValueTask.FromResult(_events.AsEnumerable());
    }

    public ValueTask<IEnumerable<UserActivityEvent>> GetEventsByUserIdAsync(string userId)
    {
        var filtered = _events.Where(e => e.UserId == userId);
        return ValueTask.FromResult(filtered);
    }

    public ValueTask<IEnumerable<UserActivityEvent>> GetEventsByTypeAsync(string eventType)
    {
        var filtered = _events.Where(e => e.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase));
        return ValueTask.FromResult(filtered);
    }
}