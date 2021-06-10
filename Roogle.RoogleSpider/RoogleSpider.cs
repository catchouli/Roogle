using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Roogle.RoogleSpider.Db;
using Roogle.RoogleSpider.Queues;
using Roogle.RoogleSpider.Services;
using Roogle.RoogleSpider.Utils;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Roogle.RoogleSpider
{
  /// <summary>
  /// The RoogleSpider
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
          // Initialise logging
          Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

          // Create service provider and initialise services
          var services = new ServiceCollection();

          // Add configuration provided by appsettings.json
          IHostEnvironment env = hostingContext.HostingEnvironment;
          configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);
          var config = configuration.Build();
          services.AddSingleton<IConfiguration>(config);

          // Add mysql
          var connString = config.GetConnectionString("DefaultConnection");
          var dbUser = File.ReadAllText("/run/secrets/roogle_db_user");
          var dbPass = File.ReadAllText("/run/secrets/roogle_db_pass");
          connString = $"{connString};user={dbUser};password={dbPass}";
          services.AddDbContext<RoogleSpiderDbContext>(options =>
            options.UseMySql(connString, ServerVersion.AutoDetect(connString)),
            ServiceLifetime.Transient);

          // Add queues
          services.AddSingleton<PagesToScrapeQueue>();
          services.AddSingleton<PagesScrapedQueue>();
          services.AddSingleton<LinksDiscoveredQueue>();

          // Add services
          services.AddSingleton<IUrlCrawlerCondition>(new BaseHostUrlCrawlerCondition("talkhaus.com"));
          services.AddSingleton<IRequestThrottleService, RequestThrottleService>();
          services.AddSingleton<IWebSpiderService, WebSpiderService>();
          services.AddSingleton<ISpiderFeederService, SpiderFeederService>();
          services.AddSingleton<IScrapedPageConsumerService, ScrapedPageConsumerService>();
          services.AddSingleton<IDiscoveredUrlConsumerService, DiscoveredUrlConsumerService>();

          // Build service provider
          var serviceProvider = services.BuildServiceProvider();

          // Run migrations
          Log.Information("Running data migrations");
          serviceProvider.GetRequiredService<RoogleSpiderDbContext>().Database.Migrate();

          // Add seed url
          var dbContext = serviceProvider.GetRequiredService<RoogleSpiderDbContext>();
          if (!dbContext.Pages.Any())
          {
            dbContext.Pages.Add(new Page
            {
              Id = Guid.NewGuid(),
              Url = StartPage,
              Title = "",
              Contents = "",
              PageHash = 0,
              PageRank = 0,
              ExpiryTime = DateTime.Now,
              UpdatedTime = DateTime.Now,
              ContentsChanged = false,
              PageRankDirty = false
            });
            dbContext.SaveChanges();
          }

          // Start services (after we do required db stuff etc)
          serviceProvider.GetRequiredService<IWebSpiderService>().StartWorkers();
          serviceProvider.GetRequiredService<ISpiderFeederService>().StartWorkers();
          serviceProvider.GetRequiredService<IScrapedPageConsumerService>().StartWorkers();
          serviceProvider.GetRequiredService<IDiscoveredUrlConsumerService>().StartWorkers();
        }).Build();

      await host.RunAsync();
    }
  }
}
