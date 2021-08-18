using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Roogle.RoogleSpider.Db;
using Roogle.Shared.Queue;
using Serilog;
using System;
using System.IO;

namespace Roogle.Shared
{
  /// <summary>
  /// A utility class for initialising services
  /// </summary>
  public static class ServiceInitHelper
  {
    /// <summary>
    /// Initialise services with a callback and return a service provider,
    /// automatically sets up logging and configuration
    /// </summary>
    /// <returns></returns>
    public static IServiceProvider InitServices(HostBuilderContext hostingContext, IConfigurationBuilder configuration,
      Action<IServiceCollection, IConfiguration> callback)
    {
      // Initialise logging
      Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateLogger();

      // Create service provider and initialise services
      var services = new ServiceCollection();

      // Add configuration provided by appsettings.json
      IHostEnvironment env = hostingContext.HostingEnvironment;
      string appsettingsName = $"appsettings.{env.EnvironmentName}.json";
      Log.Information("Starting environment {env}, loading {appsettingsName}", env.EnvironmentName, appsettingsName);
      configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile(appsettingsName, true, true);
      var config = configuration.Build();
      services.AddSingleton<IConfiguration>(config);

      Log.Information("Configuration values:");
      Log.Information(config.GetDebugView());

      // Call callback for app-specific initialisation
      callback(services, config);

      return services.BuildServiceProvider();
    }

    /// <summary>
    /// ServiceCollection extension method for setting up our database connection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="config">The configuration</param>
    public static void AddRoogleDatabase(this IServiceCollection services, IConfiguration config)
    {
      // Add mysql
      var connString = config.GetConnectionString("DefaultConnection");
      var dbUser = File.ReadAllText("/run/secrets/roogle_db_user");
      var dbPass = File.ReadAllText("/run/secrets/roogle_db_pass");
      connString = $"{connString};user={dbUser};password={dbPass}";
      services.AddDbContext<RoogleSpiderDbContext>(options =>
        options.UseMySql(connString, ServerVersion.AutoDetect(connString)),
        ServiceLifetime.Transient);
    }

    /// <summary>
    /// ServiceCollection extension method for setting up our queue connection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="config">The configuration</param>
    public static void AddRoogleQueue(this IServiceCollection services, IConfiguration config)
    {
      // TODO: we could have the queue connection params come from the configuration
      services.AddSingleton<IQueueConnection, QueueConnection>();
    }
  }
}
