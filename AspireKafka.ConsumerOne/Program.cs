using AspireKafka.Domain;
using AspireKafka.Infrastructure;
using AspireKafka.Infrastructure.Messaging;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddMassTransit(x =>
{
    // we aren't using the bus, only Kafka
    x.UsingInMemory();

    x.AddRider(r =>
    {
        r.AddKafkaComponents();

        r.AddConsumer<UserCreatedEventConsumer>();

        r.UsingKafka((context, cfg) =>
        {
            cfg.Host(context, builder.Configuration);

            cfg.ClientId = "ConsumerOne";

            cfg.TopicEndpoint<string, UserCreatedEvent>(Const.UserTopicName, Const.GroupId, e =>
            {
                e.AutoOffsetReset = AutoOffsetReset.Earliest;

                //e.SetValueDeserializer(new AvroDeserializer<WarehouseEvent>(context.GetRequiredService<ISchemaRegistryClient>()).AsSyncOverAsync());

                // the number of concurrent messages, per partition
                e.ConcurrentMessageLimit = 10;

                // create up to two Confluent Kafka consumers, increases throughput with multiple partitions
                e.ConcurrentConsumerLimit = 2;

                // delivery only one message per key value within a partition at a time (default)
                e.ConcurrentDeliveryLimit = 1;

                // Adding this filter allows AVRO union messages to be consumed directly
                //e.UseAvroUnionMessageTypeFilter<UserCreatedEvent>(m => m.Event);

                e.UseSampleRetryConfiguration();

                // ignore these messages, move past them, not used, but avoid errors it's one of the AVRO union types
                // because DiscardSkippedMessages doesn't seem to work properly with topic endpoint
                e.Handler<UserCreatedEvent>(_ => Task.CompletedTask);

                //e.ConfigureSaga<ContainerState>(context);
            });
        });
    });
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
