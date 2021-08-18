using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Roogle.RoogleSpiderFeeder;
using Roogle.Shared;
using Roogle.Shared.Services;
using System.Threading.Tasks;

namespace Roogle.RoogleSpider
{
  /// <summary>
  /// The Roogle spider feeder
  /// </summary>
  public class RoogleSpiderFeeder
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
            services.AddSingleton<ISpiderFeederService, SpiderFeederService>();
          });

          // Start spider feeder service
          serviceProvider.GetRequiredService<ISpiderFeederService>().Start();
        }).Build();

      await host.RunAsync();
    }
  }
}
