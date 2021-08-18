using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Roogle.Shared;
using Roogle.Shared.CrawlConditions;
using Roogle.Shared.Services;
using System.Threading.Tasks;

namespace Roogle.RoogleUrlConsumer
{
  /// <summary>
  /// The Roogle url consumer
  /// </summary>
  public class RoogleUrlConsumer
  {
    /// <summary>
    /// Our entrypoint
    /// </summary>
    public static async Task Main(string[] args)
    {
      using var host = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostingContext, configuration) =>
        {
          var serviceProvider = ServiceInitHelper.InitServices(hostingContext, configuration, (services, config) =>
          {
            services.AddRoogleDatabase(config);
            services.AddRoogleQueue(config);
            services.AddSingleton<IUrlCrawlerCondition, RoogleLinkExcluder>();
            services.AddSingleton<ICanonicalUrlService, CanonicalUrlService>();
            services.AddSingleton<IRequestThrottleService, RequestThrottleService>();
            services.AddSingleton<IDiscoveredUrlConsumerService, DiscoveredUrlConsumerService>();
          });

          // Start url consumer service
          serviceProvider.GetRequiredService<IDiscoveredUrlConsumerService>().Start();
        }).Build();

      await host.RunAsync();
    }
  }
}
