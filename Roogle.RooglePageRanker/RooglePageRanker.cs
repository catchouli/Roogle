using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Roogle.RooglePageRanker;
using Roogle.Shared;
using Roogle.Shared.Services;
using System.Threading.Tasks;

namespace Roogle.RoogleSpider
{
  /// <summary>
  /// The Roogle page ranker
  /// </summary>
  public class RooglePageRanker
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
            services.AddSingleton<IPageRankerService, PageRankerService>();
          });

          // Start page ranker service
          serviceProvider.GetRequiredService<IPageRankerService>().Start();
        }).Build();

      await host.RunAsync();
    }
  }
}
