using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Roogle.RoogleIndexer;
using Roogle.Shared;
using Roogle.Shared.Services;
using System.Threading.Tasks;

namespace Roogle.RoogleSpider
{
  /// <summary>
  /// The Roogle indexer
  /// </summary>
  public class RoogleIndexer
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
            services.AddSingleton<IRequestThrottleService, RequestThrottleService>();
            services.AddSingleton<IPageIndexerService, PageIndexerService>();
          });

          // Start indexer service
          serviceProvider.GetRequiredService<IPageIndexerService>().Start();
        }).Build();

      await host.RunAsync();
    }
  }
}
