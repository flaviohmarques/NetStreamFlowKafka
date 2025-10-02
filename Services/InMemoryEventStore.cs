using System.Collections.Concurrent;
using NetStreamFlowKafka.Models;

namespace NetStreamFlowKafka.Services;

public interface IEventStore
{
    Task AddEventAsync(UserActivityEvent userEvent);
    Task<IEnumerable<UserActivityEvent>> GetAllEventsAsync();
    Task<IEnumerable<UserActivityEvent>> GetEventsByUserIdAsync(string userId);
    Task<IEnumerable<UserActivityEvent>> GetEventsByTypeAsync(string eventType);
}

public class InMemoryEventStore : IEventStore
{
    private readonly ConcurrentBag<UserActivityEvent> _events = new();
    private readonly ILogger<InMemoryEventStore> _logger;

    public InMemoryEventStore(ILogger<InMemoryEventStore> logger)
    {
        _logger = logger;
    }

    public Task AddEventAsync(UserActivityEvent userEvent)
    {
        _events.Add(userEvent);
        _logger.LogInformation("Event stored: {EventId} - {EventType} for User: {UserId}",
            userEvent.EventId, userEvent.EventType, userEvent.UserId);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<UserActivityEvent>> GetAllEventsAsync()
    {
        return Task.FromResult(_events.AsEnumerable());
    }

    public Task<IEnumerable<UserActivityEvent>> GetEventsByUserIdAsync(string userId)
    {
        var filtered = _events.Where(e => e.UserId == userId);
        return Task.FromResult(filtered);
    }

    public Task<IEnumerable<UserActivityEvent>> GetEventsByTypeAsync(string eventType)
    {
        var filtered = _events.Where(e => e.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(filtered);
    }
}