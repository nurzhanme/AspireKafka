using System.IO.Compression;
using AspireKafka.Domain;
using AspireKafka.Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddInfrastructure(builder.Configuration);


builder.Host.UseMassTransit((hostContext, x) =>
{
    x.AddEntityFrameworkOutbox<DataContext>(options =>
    {
        options.QueryDelay = TimeSpan.FromSeconds(1);
        options.UsePostgres();
    });

    //x.TryAddScoped<IUserValidationService, UserValidationService>();

    x.UsingInMemory();

    x.AddRider(r =>
    {
        r.AddKafkaComponents();

        r.AddProducer<string, UserCreatedEvent>(Const.UserTopicName, (context, cfg) =>
        {
            // Configure the AVRO serializer, with the schema registry client
            //cfg.SetValueSerializer(new AvroSerializer<UserCreatedEvent>(context.GetRequiredService<ISchemaRegistryClient>()));
        });

        r.UsingKafka((context, cfg) =>
        {
            cfg.ClientId = "ApiService";

            cfg.Host(context, builder.Configuration);
        });
    });
});


builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "AspireKafka.ApiService v1");
    });
    app.MapScalarApiReference(options =>
    {
        options.Servers = [];
    });
}

app.UseCors("AllowAll");

app.MapControllers();

app.Run();

public record UserCreate(string Username);
