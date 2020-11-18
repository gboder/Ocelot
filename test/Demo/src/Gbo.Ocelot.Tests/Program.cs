using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using Ocelot.Configuration;
using Ocelot.DownstreamRouteFinder;
using Ocelot.Middleware;

namespace gw
{
    public class Program
    {
        public static void Main(string[] args)
        {// NLog: setup the logger first to catch all errors
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Debug("init main");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {

            var hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config
                        .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                        .AddJsonFile("appsettings.json", false, true)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true)
                        .AddJsonFile("ocelot.json", false, true)
                        .AddEnvironmentVariables();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
                .UseNLog();

            return hostBuilder;
        }
    }

    public class HostInjectorDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HostInjectorDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.UpsertHost(_httpContextAccessor.HttpContext.Items
                .DownstreamRouteHolder()
                .UpstreamHeadersFindAndReplace()
                .HostHeaderReplacer().Replace);
            return base.SendAsync(request, cancellationToken);
        }
    }

    public static class HttpRequestExtensions
    {
        public static void UpsertHost(this HttpRequestMessage input, string value)
        {
            UpsertHeader(input, "Host", value);
        }

        public static void UpsertHeader(this HttpRequestMessage input, string key, string value)
        {
            if (input != null)
            {
                if (input.Headers.Contains(key))
                {
                    input.Headers.Remove(key);
                }
                input.Headers.Add(key, value);
            }
        }
    }

    public static class OcelotHttpExtensions
    {
        public static HeaderFindAndReplace HostHeaderReplacer(this List<HeaderFindAndReplace> input)
        {
            return input?.SingleOrDefault(hfar => hfar.Key.Equals("host", StringComparison.OrdinalIgnoreCase));
        }

        public static List<HeaderFindAndReplace> UpstreamHeadersFindAndReplace(this DownstreamRouteHolder input)
        {
            return input?.Route.DownstreamRoute.FirstOrDefault()?.UpstreamHeadersFindAndReplace;
        }

        public static DownstreamRouteHolder DownstreamRouteHolder(this IDictionary<object, object> input)
        {
            return input.Get<DownstreamRouteHolder>("DownstreamRouteHolder");
        }

        private static T Get<T>(this IDictionary<object, object> input, string key)
        {
            if (input.TryGetValue(key, out var value))
            {
                return (T)value;
            }

            return default(T);
        }
    }
}
