using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Roogle.RoogleSpider.Db;
using Roogle.Shared;
using Roogle.Shared.Services;
using Serilog;
using System.Threading.Tasks;

namespace Roogle.RooglePageConsumer
{
  /// <summary>
  /// The Roogle page consumer
  /// </summary>
  public class RooglePageConsumer
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
            services.AddSingleton<IScrapedPageConsumerService, ScrapedPageConsumerService>();
          });

          // Run migrations, this app specifically is responsible for this since the db is shared between multiple apps
          Log.Information("Running data migrations");
          serviceProvider.GetRequiredService<RoogleSpiderDbContext>().Database.Migrate();

          // Start page consumer service
          serviceProvider.GetRequiredService<IScrapedPageConsumerService>().Start();
        }).Build();

      await host.RunAsync();
    }
  }
}
