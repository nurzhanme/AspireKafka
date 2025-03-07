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
            .AddPersistence(configuration)
            .AddCap(configuration)
            ;
             
        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DataContext>(options => options.UseNpgsql(configuration.GetConnectionString("postgres")));
            
        return services;
    }

    private static IServiceCollection AddCap(this IServiceCollection services, IConfiguration configuration) 
    {
        services.AddCap(x =>
        {
            x.UseEntityFramework<DataContext>();
            x.UsePostgreSql("postgres");

            x.UseKafka(configuration.GetConnectionString("kafka")!);
            x.UseDashboard();
        });
        return services;
    }
}