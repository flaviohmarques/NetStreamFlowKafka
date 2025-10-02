# NetStreamFlowKafka

**Event-Driven Backend Service with .NET Core and Apache Kafka**

A scalable microservice demonstrating event-driven architecture using .NET 8, Apache Kafka, and async/await patterns.

# NetStreamFlowKafka

**Event-Driven Backend Service with .NET Core and Apache Kafka**

A scalable microservice demonstrating event-driven architecture using .NET 8, Apache Kafka, and async/await patterns.

---

## ?? Overview

NetStreamFlowKafka is a production-ready reference implementation showcasing how to build event-driven microservices. The service processes user activity events asynchronously through Kafka, demonstrating best practices in distributed systems design.

## ??? Architecture

```
???????????????      ????????????????      ???????????????
?   Client    ????????  REST API    ????????   Kafka     ?
?             ?      ?  (Producer)  ?      ?   Topic     ?
???????????????      ????????????????      ???????????????
                                                   ?
                                                   ?
                                                   ?
                                            ???????????????
                                            ?   Kafka     ?
                                            ?  Consumer   ?
                                            ???????????????
                                                   ?
                                                   ?
                                            ???????????????
                                            ?  In-Memory  ?
                                            ?    Store    ?
                                            ???????????????
```

## Features

- ? **Event Publishing**: REST API endpoint to publish user activity events
- ? **Async Processing**: Kafka-based asynchronous event processing
- ? **Event Storage**: Thread-safe in-memory event store using `ConcurrentBag`
- ? **Event Querying**: REST API endpoints to query processed events
- ? **Scalable Design**: Clean architecture with dependency injection
- ? **Error Handling**: Comprehensive error handling and logging
- ? **Background Processing**: `BackgroundService` for continuous event consumption

## Technologies Used

- .NET 8.0
- ASP.NET Core Web API
- Confluent.Kafka (2.3.0)
- Swagger/OpenAPI
- Docker & Docker Compose

## ?? Project Structure

```
NetStreamFlowKafka/
??? EventsApi.cs                     # Minimal API endpoints
??? Models/
?   ??? UserActivityEvent.cs         # Event model and settings
??? Services/
?   ??? InMemoryEventStore.cs        # Thread-safe event storage
?   ??? KafkaProducerService.cs      # Kafka producer implementation
?   ??? KafkaConsumerService.cs      # Background Kafka consumer
??? Program.cs                       # Application entry point
??? appsettings.json                 # Configuration
??? NetStreamFlowKafka.csproj        # Project file
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Docker and Docker Compose (for local Kafka)

### Option 1: Local Kafka with Docker

1. **Start Kafka**:
```bash
docker-compose up -d
```

This starts:
- Zookeeper (port 2181)
- Kafka (port 9092)
- Kafka UI (port 8080) - Access at http://localhost:8080

2. **Run the application**:
```bash
dotnet restore
dotnet run
```

3. **Access the API**:
- Swagger UI: https://localhost:7000/swagger (or http://localhost:5000/swagger)
- Kafka UI: http://localhost:8080

## API Endpoints

### 1. Publish Event
**POST** `/api/events`

Publishes a user activity event to Kafka.

**Request Body**:
```json
{
  "userId": "user123",
  "eventType": "login",
  "metadata": {
    "ipAddress": "192.168.1.1",
    "userAgent": "Mozilla/5.0"
  }
}
```

**Response** (202 Accepted):
```json
{
  "message": "Event published successfully",
  "eventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Event Types Examples**:
- `login` - User logged in
- `logout` - User logged out
- `purchase` - User made a purchase
- `page_view` - User viewed a page
- `profile_update` - User updated their profile

### 2. Get All Events
**GET** `/api/events`

Retrieves all processed events.

**Response** (200 OK):
```json
{
  "count": 5,
  "events": [
    {
      "eventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "userId": "user123",
      "eventType": "login",
      "timestamp": "2025-10-01T10:30:00Z",
      "metadata": {
        "ipAddress": "192.168.1.1"
      }
    }
  ]
}
```

### 3. Get Events by User ID
**GET** `/api/events/user/{userId}`

Retrieves all events for a specific user.

**Response** (200 OK):
```json
{
  "userId": "user123",
  "count": 3,
  "events": [...]
}
```

### 4. Get Events by Type
**GET** `/api/events/type/{eventType}`

Retrieves all events of a specific type.

**Response** (200 OK):
```json
{
  "eventType": "login",
  "count": 10,
  "events": [...]
}
```

## Testing the Service

### Using cURL

**Publish an event**:
```bash
curl -X POST https://localhost:7000/api/events \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "user123",
    "eventType": "login",
    "metadata": {
      "ipAddress": "192.168.1.1"
    }
  }'
```

**Get all events**:
```bash
curl https://localhost:7000/api/events
```

**Get events by user**:
```bash
curl https://localhost:7000/api/events/user/user123
```

### Using Swagger UI

1. Navigate to https://localhost:7000/swagger
2. Expand the endpoints and click "Try it out"
3. Enter the request data and execute

## Key Design Decisions

### 1. **Async/Await Pattern**
All I/O operations use async/await for better scalability and resource utilization.

### 2. **Thread-Safe Storage**
`ConcurrentBag<T>` provides thread-safe operations without explicit locking.

### 3. **Dependency Injection**
Services are registered in the DI container for testability and maintainability:
- `IEventStore` - Singleton for shared state
- `IKafkaProducerService` - Singleton for connection pooling
- `KafkaConsumerService` - Hosted service for background processing

### 4. **Clean Architecture**
- **Controllers**: HTTP API layer
- **Services**: Business logic and infrastructure
- **Models**: Data structures

### 5. **Error Handling**
- Try-catch blocks in all async operations
- Structured logging with context
- Graceful degradation

### 6. **Kafka Configuration**
- **Producer**: Idempotent with leader acknowledgment
- **Consumer**: Manual offset commit for reliability
- **Auto-create topics**: Enabled for development convenience

## Scalability Considerations

1. **Horizontal Scaling**: Multiple instances can run with same consumer group (Kafka handles partition assignment)
2. **Partition Strategy**: Events are keyed by `userId` for ordered processing per user
3. **Backpressure Handling**: Consumer processes one message at a time with manual commits
4. **State Management**: In-memory store can be replaced with Redis/Database for distributed deployments

## Production Considerations

For production deployment, consider:

1. **Persistent Storage**: Replace `InMemoryEventStore` with a database (PostgreSQL, MongoDB)
2. **Authentication**: Add JWT/OAuth for API endpoints
3. **Rate Limiting**: Implement rate limiting on publish endpoint
4. **Monitoring**: Add metrics (Prometheus, Application Insights)
5. **Distributed Tracing**: Implement OpenTelemetry for request tracing
6. **Configuration Management**: Use Azure Key Vault or AWS Secrets Manager
7. **Health Checks**: Add Kafka connectivity health checks
8. **Circuit Breaker**: Implement Polly for resilience
9. **Dead Letter Queue**: Handle failed messages gracefully

## Monitoring and Observability

### Logs
The application logs:
- Event publication (with partition/offset)
- Event consumption and processing
- Errors and exceptions with context

### Kafka UI
Access the Kafka UI at http://localhost:8080 to:
- Monitor topic lag
- View message details
- Check consumer group status
- Analyze partition distribution

## Troubleshooting

### Kafka Connection Issues
```bash
# Check if Kafka is running
docker ps

# View Kafka logs
docker logs kafka

# Restart Kafka
docker-compose restart kafka
```

### Consumer Not Processing Events
- Check consumer group status in Kafka UI
- Verify topic exists and has messages
- Check application logs for errors
- Ensure consumer group ID is unique

### Events Not Appearing in Store
- Wait a few seconds (consumer polls every 1 second)
- Check Kafka UI to verify messages are in the topic
- Verify consumer service is running (check logs)

## Load Testing Example

```bash
# Install Apache Bench or use this simple bash script
for i in {1..100}; do
  curl -X POST https://localhost:7000/api/events \
    -H "Content-Type: application/json" \
    -d "{\"userId\":\"user$i\",\"eventType\":\"login\"}" &
done
wait

# Check results
curl https://localhost:7000/api/events | jq '.count'
```

## Clean Up

```bash
# Stop and remove containers
docker-compose down

# Remove volumes (deletes all Kafka data)
docker-compose down -v
```

## Author

Flavio Henrique Silva Marques

---

## Quick Start Commands

```bash
# Clone and setup
git clone <repository>
cd NetStreamFlowKafka

# Start Kafka
docker-compose up -d

# Run the service
dotnet run

# Publish test event
curl -X POST https://localhost:7000/api/events \
  -H "Content-Type: application/json" \
  -d '{"userId":"test123","eventType":"login"}'

# View events
curl https://localhost:7000/api/events

# Access Swagger UI
# Open browser: https://localhost:7000/swagger

# Access Kafka UI
# Open browser: http://localhost:8080
```