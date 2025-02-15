using AspireKafka.Domain;
using AspireKafka.Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddInfrastructure(builder.Configuration);


    builder.Host.UseMassTransit((hostContext, x) =>
{
    
    //x.TryAddScoped<IUserValidationService, UserValidationService>();

    x.UsingInMemory();

    x.AddRider(r =>
    {
        r.AddKafkaComponents();

        var location = hostContext.Configuration.GetValue<int>("Warehouse:Location");
        if (location == default)
            throw new ConfigurationException("The warehouse location is required and was not configured.");

        var warehouseTopicName = $"events.warehouse.{location}";

        r.AddProducer<string, WarehouseEvent>(warehouseTopicName, (context, cfg) =>
        {
            // Configure the AVRO serializer, with the schema registry client
            cfg.SetValueSerializer(new AvroSerializer<WarehouseEvent>(context.GetRequiredService<ISchemaRegistryClient>()));
        });

        r.AddProducer<string, ShipmentManifestReceived>("events.erp", (context, cfg) =>
        {
            // Configure the AVRO serializer, with the schema registry client
            cfg.SetValueSerializer(new AvroSerializer<ShipmentManifestReceived>(context.GetRequiredService<ISchemaRegistryClient>()));
        });

        r.UsingKafka((context, cfg) =>
        {
            cfg.ClientId = "WarehouseApi";

            cfg.Host(context);
        });
    });
});


// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.MapPost("/user", async ([FromBody] UserCreate user, DataContext context)
    =>
{
    var newUser = User.Create(user.Username);
    await context.Users.AddAsync(newUser);

    await context.SaveChangesAsync();
    return Results.Created($"/user/{user.Username}", user);
});

app.MapDefaultEndpoints();

app.Run();

record UserCreate(string Username);
