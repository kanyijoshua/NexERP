using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.Hosting;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddSharedEndpoints(this IHostApplicationBuilder builder)
    {
        builder.AddRabbitMQClient(
            connectionName: ABPmicroserviceNames.RabbitMq,
            action =>
                action.ConnectionString = builder.Configuration.GetConnectionString(
                    ABPmicroserviceNames.RabbitMq
                )
        );
        builder.AddRedisDistributedCache(connectionName: ABPmicroserviceNames.Redis);
        builder.AddSeqEndpoint(connectionName: ABPmicroserviceNames.Seq);

        return builder;
    }
}
