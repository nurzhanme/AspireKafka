﻿using AspireKafka.Infrastructure.Options;
using Confluent.Kafka;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AspireKafka.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddPersistence(configuration);
             
        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DataContext>(options => options.UseNpgsql(configuration.GetConnectionString("postgres")));
            
        return services;
    }

    /// <summary>
    /// Configure the Confluent Schema Registry for connection to Confluent Cloud
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T AddKafkaComponents<T>(this T services)
        where T : IServiceCollection
    {
        services.AddOptions<KafkaOptions>().BindConfiguration("Kafka");
        //services.AddOptions<SchemaRegistryOptions>().BindConfiguration("SchemaRegistry");

        //services.AddSingleton<ISchemaRegistryClient>(provider =>
        //{
        //    var options = provider.GetRequiredService<IOptions<SchemaRegistryOptions>>().Value;

        //    return new CachedSchemaRegistryClient(new Dictionary<string, string>
        //    {
        //        { "schema.registry.url", options.Server! },
        //        { "schema.registry.basic.auth.credentials.source", "SASL_INHERIT" },
        //        { "sasl.username", options.Username! },
        //        { "sasl.password", options.Password! }
        //    });
        //});

        return services;
    }

    /// <summary>
    /// Just an example, but some retry/kill switch combination to stop processing when the consumer/saga faults repeatedly.
    /// </summary>
    /// <param name="configurator"></param>
    public static void UseSampleRetryConfiguration(this IKafkaTopicReceiveEndpointConfigurator configurator)
    {
        configurator.UseKillSwitch(k => k.SetActivationThreshold(1).SetRestartTimeout(m: 1).SetTripThreshold(0.2).SetTrackingPeriod(m: 1));
        configurator.UseMessageRetry(retry => retry.Interval(1000, TimeSpan.FromSeconds(1)));
    }

    /// <summary>
    /// Configure the Kafka Host using SASL_SSL to connect to Confluent Cloud
    /// </summary>
    /// <param name="configurator"></param>
    /// <param name="context"></param>
    public static void Host(this IKafkaFactoryConfigurator configurator, IRiderRegistrationContext context, IConfiguration configuration)
    {
        var options = context.GetRequiredService<IOptions<KafkaOptions>>().Value;

        configurator.Host(configuration.GetConnectionString("kafka"));
    }
}