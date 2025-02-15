using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);


// Uncomment the following line depending on what you need

var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin();


var kafka = builder
    .AddKafka("kafka")
    .WithKafkaUI();

if (builder.Environment.IsDevelopment())
{
    builder.AddProject<Projects.AspireKafka_DbMigrator>("aspirekafka-dbmigrator")
        .WithReference(postgres).WaitFor(postgres)
        .WithReplicas(1);
}

var apiService = builder.AddProject<Projects.AspireKafka_ApiService>("apiservice")
    .WithReference(postgres)
    .WithReference(kafka)
    .WaitFor(kafka);

builder.AddProject<Projects.AspireKafka_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.AddProject<Projects.AspireKafka_ConsumerOne>("aspirekafka-consumerone")
    .WithReference(kafka)
    .WaitFor(kafka);

builder.AddProject<Projects.AspireKafka_ConsumerTwo>("aspirekafka-consumertwo")
    .WithReference(kafka)
    .WaitFor(kafka);

builder.Build().Run();