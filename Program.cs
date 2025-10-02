using NetStreamFlowKafka;
using NetStreamFlowKafka.Models;
using NetStreamFlowKafka.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "NetStreamFlowKafka API",
        Version = "v1",
        Description = "Event-Driven Backend Service with .NET Core and Kafka"
    });
});

// Configure Kafka settings
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));

// Register services
builder.Services.AddSingleton<IEventStore, InMemoryEventStore>();
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddHostedService<KafkaConsumerService>();

var app = builder.Build();

app.MapGroup("/api/events")
   .MapEventsApi();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NetStreamFlowKafka API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();