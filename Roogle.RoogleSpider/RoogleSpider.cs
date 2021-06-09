using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Roogle.RoogleSpider.Db;
using Serilog;
using System.IO;
using System.Threading.Tasks;

namespace Roogle.RoogleSpider
{
  /// <summary>
  /// The RoogleSpider
  /// </summary>
  public class RoogleSpider
  {
    /// <summary>
    /// The number of requests per second to allow
    /// </summary>
    private const int RequestsPerSecond = 5;

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
        .ConfigureAppConfiguration(async (hostingContext, configuration) =>
        {
          // Initialise logging
          Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

          // Add appsettings.json
          IHostEnvironment env = hostingContext.HostingEnvironment;
          configuration
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);
          var config = configuration.Build();

          // Create service provider and initialise services
          var services = new ServiceCollection();

          // Add mysql
          var connString = config.GetConnectionString("DefaultConnection");
          var dbUser = File.ReadAllText("/run/secrets/roogle_db_user");
          var dbPass = File.ReadAllText("/run/secrets/roogle_db_pass");
          connString = $"{connString};user={dbUser};password={dbPass}";
          services.AddDbContext<RoogleSpiderDbContext>(options =>
            options.UseMySql(connString, ServerVersion.AutoDetect(connString)));

          // Add web spider
          services.AddSingleton<IUrlCrawlerCondition>(new BaseHostUrlCrawlerCondition("talkhaus.com"));
          services.AddSingleton(services =>
          {
            var spider = new WebSpider(services.GetRequiredService<IUrlCrawlerCondition>(), RequestsPerSecond);
            spider.AddUrlToOpenSet(StartPage);
            return spider;
          });

          // Build service provider
          var serviceProvider = services.BuildServiceProvider();

          // Start crawling
          var spider = serviceProvider.GetRequiredService<WebSpider>();
          await spider.StartCrawling();
        }).Build();

      await host.RunAsync();
    }
  }
}
