using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace common
{
    public class Constants
    {
        public const string LoggerCategory = "ConsulAppExtensions";

        public const string ServiceAddressEnvironmentKey = "SERVICE_ADDRESS";

        public const string ServerFeaturesKey = "server.Features";
    }
    public static class ConsulExtensions
    {
        public static IServiceCollection AddConsul(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var consulUri = new Uri($"http://{Environment.GetEnvironmentVariable("APP_CONSUL_HOST")}");

            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                consulConfig.Address = consulUri;
                consulConfig.Token = Environment.GetEnvironmentVariable("APP_CONSUL_TOKEN");
            }));

            return services;
        }
        public static IApplicationBuilder UseConsul(this IApplicationBuilder app, string address)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            IConsulClient consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
            ILogger logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger(Constants.LoggerCategory);
#pragma warning disable CS0618 // Type or member is obsolete
            // we cannot use IHostApplicationLifetime as it in .NET Core 3.0 (not netstandard2)
            IApplicationLifetime lifetime = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();
#pragma warning restore CS0618 // Type or member is obsolete

            var serviceName = Environment.GetEnvironmentVariable("APP_SERVICE_NAME");
            serviceName = string.IsNullOrWhiteSpace(serviceName) ? "app" : serviceName;

            AgentServiceRegistration registration = new AgentServiceRegistration
            {
                // ID = $"{Configuration.ServiceName}-{uri.Port}",
                // Name = Configuration.ServiceName,
                // Address = $"{uri.Host}",
                // Port = uri.Port,
                ID = $"{serviceName}@{address}:80",
                Name = serviceName,
                Address = address,
                Port = 80,
            };

            registration.Checks = new AgentServiceCheck[]{
                new AgentServiceCheck
                {
                    HTTP = $"http://{address}/info/env",
                    Timeout = TimeSpan.FromMilliseconds(100),
                    Interval = TimeSpan.FromSeconds(1),
                    Header = new Dictionary<string, List<string>>{ {"Host",new List<string>{"app.pr114.isago.ch"} } },
                    Method = "GET",
                    Notes=$"http://{address}/info/env",
                },
                new AgentServiceCheck{
                    HTTP = $"http://{address}/info/header",
                    Timeout = TimeSpan.FromMilliseconds(100),
                    Interval = TimeSpan.FromSeconds(1),
                    Header = new Dictionary<string, List<string>>{ {"Host",new List<string>{"app.pr114.isago.ch"} } },
                    Method = "GET",
                    Notes = $"http://{address}/info/header",
                },
                new AgentServiceCheck{
                    HTTP = $"http://{address}/weatherforecast",
                    Timeout = TimeSpan.FromMilliseconds(100),
                    Interval = TimeSpan.FromSeconds(1),
                    Header = new Dictionary<string, List<string>>{ {"Host",new List<string>{"app.pr114.isago.ch"} } },
                    Method = "GET",
                    Notes = $"http://{address}/info/header",
                },
                new AgentServiceCheck{
                    HTTP = $"http://{address}/info/health",
                    Timeout = TimeSpan.FromMilliseconds(100),
                    Interval = TimeSpan.FromSeconds(1),
                    Header = new Dictionary<string, List<string>>{ {"Host",new List<string>{"app.pr114.isago.ch"} } },
                    Method = "GET",
                    Name = "info/health",
                    Notes = $"http://{address}/info/header",
                }
            };

            // registration.Tags = new string[] { "http://app" };
            // registration.Meta = new Dictionary<string, string> { { "host-header", "http://app:80/" } };
            var consulUri = new Uri($"http://{Environment.GetEnvironmentVariable("APP_CONSUL_HOST")}");

            logger.LogInformation($"Registering Service {registration.Name} with URL {registration.Address}:{registration.Port} to {consulUri}");
            consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
            consulClient.Agent.ServiceRegister(registration).ConfigureAwait(true);

            lifetime.ApplicationStopping.Register(() =>
                {
                    logger.LogInformation($"Unregistering Service {registration.Name} with URL {registration.Address}:{registration.Port}");
                    consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
                });

            return app;
        }
    }
}
