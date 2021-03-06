using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Roogle.RoogleSpider.Db;
using Roogle.Shared;
using Roogle.Shared.Queue;
using Roogle.Shared.Services;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Roogle.RoogleSpider
{
  /// <summary>
  /// The Roogle spider
  /// </summary>
  public class RoogleSpider
  {
    /// <summary>
    /// The start page, in lieu of anything else
    /// </summary>
    private const string StartPage = "https://index.talkhaus.com/";

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
            services.AddSingleton<IWebSpiderService, WebSpiderService>();
            services.AddSingleton<IRobotsTxtService, RobotsTxtService>();
          });

          // Add seed url
          serviceProvider.GetRequiredService<IQueueConnection>()
            .CreateQueue("PagesToScrape")
            .SendMessage(StartPage);

          // Start web spider
          serviceProvider.GetRequiredService<IWebSpiderService>().Start();
        }).Build();

      await host.RunAsync();
    }
  }
}
