using AspireKafka.DbMigrator;
using AspireKafka.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

builder.Services.AddInfrastructure(builder.Configuration);

var host = builder.Build();
host.Run();
