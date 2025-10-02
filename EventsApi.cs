using Microsoft.AspNetCore.Mvc;
using NetStreamFlowKafka.Models;
using NetStreamFlowKafka.Services;

namespace NetStreamFlowKafka;

public static class EventsApi
{
    public static RouteGroupBuilder MapEventsApi(this RouteGroupBuilder group)
    {
        group.MapPost("/", PublishEvent)
            .Accepts<UserActivityEvent>("application/json")
            .Produces(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Publish Event")
            .WithDescription("Publish a user activity event to Kafka.");

        group.MapGet("/", GetAllEvents)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Get All Events")
            .WithDescription("Retrieve all processed events ordered by timestamp descending.");

        group.MapGet("/user/{userId}", GetEventsByUserId)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Get Events by User")
            .WithDescription("Retrieve events filtered by a specific user ID.");

        group.MapGet("/type/{eventType}", GetEventsByType)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithSummary("Get Events by Type")
            .WithDescription("Retrieve events filtered by event type.");

        return group;
    }

    internal static async Task<IResult> PublishEvent(
        [FromBody] UserActivityEvent userEvent,
        IKafkaProducerService producerService,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("EventsApi");

        if (string.IsNullOrWhiteSpace(userEvent.UserId))
        {
            return Results.BadRequest(new { error = "UserId is required" });
        }

        if (string.IsNullOrWhiteSpace(userEvent.EventType))
        {
            return Results.BadRequest(new { error = "EventType is required" });
        }

        try
        {
            var success = await producerService.PublishEventAsync(userEvent);

            if (success)
            {
                return Results.Accepted($"/api/events/{userEvent.EventId}", new
                {
                    message = "Event published successfully",
                    eventId = userEvent.EventId
                });
            }

            return Results.Problem("Failed to publish event to Kafka", statusCode: 500);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing event");
            return Results.Problem("An unexpected error occurred", statusCode: 500);
        }
    }

    internal static async Task<IResult> GetAllEvents(
        IEventStore eventStore,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("EventsApi");

        try
        {
            var events = await eventStore.GetAllEventsAsync();
            return Results.Ok(new
            {
                count = events.Count(),
                events = events.OrderByDescending(e => e.Timestamp)
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving events");
            return Results.Problem("An unexpected error occurred", statusCode: 500);
        }
    }

    internal static async Task<IResult> GetEventsByUserId(
        string userId,
        IEventStore eventStore,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("EventsApi");

        try
        {
            var events = await eventStore.GetEventsByUserIdAsync(userId);

            if (!events.Any())
            {
                return Results.NotFound(new { message = $"No events found for user {userId}" });
            }

            return Results.Ok(new
            {
                userId,
                count = events.Count(),
                events = events.OrderByDescending(e => e.Timestamp)
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving events for user {UserId}", userId);
            return Results.Problem("An unexpected error occurred", statusCode: 500);
        }
    }

    internal static async Task<IResult> GetEventsByType(
        string eventType,
        IEventStore eventStore,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("EventsApi");

        try
        {
            var events = await eventStore.GetEventsByTypeAsync(eventType);

            if (!events.Any())
            {
                return Results.NotFound(new { message = $"No events found for type {eventType}" });
            }

            return Results.Ok(new
            {
                eventType,
                count = events.Count(),
                events = events.OrderByDescending(e => e.Timestamp)
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving events for type {EventType}", eventType);
            return Results.Problem("An unexpected error occurred", statusCode: 500);
        }
    }
}
