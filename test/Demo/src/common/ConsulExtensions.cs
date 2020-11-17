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

            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                consulConfig.Address = new Uri("http://consul01:8500");
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


            AgentServiceRegistration registration = new AgentServiceRegistration
            {
                // ID = $"{Configuration.ServiceName}-{uri.Port}",
                // Name = Configuration.ServiceName,
                // Address = $"{uri.Host}",
                // Port = uri.Port,
                ID = $"app@{address}:80",
                Name = "app",
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

                },
                new AgentServiceCheck{
                    HTTP = $"http://{address}/info/header",
                    Timeout = TimeSpan.FromMilliseconds(100),
                    Interval = TimeSpan.FromSeconds(1),
                    Header = new Dictionary<string, List<string>>{ {"Host",new List<string>{"app.pr114.isago.ch"} } },
                    Method = "GET",
                },
                new AgentServiceCheck{
                    HTTP = $"http://{address}/weatherforecast",
                    Timeout = TimeSpan.FromMilliseconds(100),
                    Interval = TimeSpan.FromSeconds(1),
                    Header = new Dictionary<string, List<string>>{ {"Host",new List<string>{"app.pr114.isago.ch"} } },
                    Method = "GET",
                },
                new AgentServiceCheck{
                    HTTP = $"http://{address}/info/health",
                    Timeout = TimeSpan.FromMilliseconds(100),
                    Interval = TimeSpan.FromSeconds(1),
                    Header = new Dictionary<string, List<string>>{ {"Host",new List<string>{"app.pr114.isago.ch"} } },
                    Method = "GET",
                    Name = "info/health",
                    Notes="GET info/health",
                }
            };

            // registration.Tags = new string[] { "http://app" };
            // registration.Meta = new Dictionary<string, string> { { "host-header", "http://app:80/" } };

            logger.LogInformation($"Registering Service {registration.Name} with URL {registration.Address}:{registration.Port}");
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
