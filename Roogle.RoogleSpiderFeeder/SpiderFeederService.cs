using Microsoft.Extensions.Configuration;
using Roogle.RoogleSpider.Db;
using Roogle.Shared.Queue;
using Roogle.Shared.Services;
using Serilog;
using System;
using System.Linq;
using System.Threading;

namespace Roogle.RoogleSpiderFeeder
{
  /// <summary>
  /// The web spider worker
  /// </summary>
  public class SpiderFeederService : ISpiderFeederService
  {
    /// <summary>
    /// The request throttle service
    /// </summary>
    private readonly IRequestThrottleService _throttleService;

    /// <summary>
    /// The expiry time for preventing us from sending the same page more than once
    /// </summary>
    private readonly TimeSpan _expireAfter;

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// The outgoing queue for feeding the spider
    /// </summary>
    private readonly IQueue _pagesToScrapeQueue;

    /// <summary>
    /// Temp, the urls discovered queue
    /// </summary>
    private readonly IQueue _urlsDiscoveredQueue;

    /// <summary>
    /// The db context
    /// </summary>
    private readonly RoogleSpiderDbContext _dbContext;

    /// <summary>
    /// Create the web spider worker
    /// </summary>
    /// <param name="configuration">The config</param>
    /// <param name="dbContext">The db context</param>
    /// <param name="throttleService">The request throttle service</param>
    /// <param name="queueConnection">The queue connect</param>
    public SpiderFeederService(IConfiguration configuration, RoogleSpiderDbContext dbContext,
      IRequestThrottleService throttleService, IQueueConnection queueConnection)
    {
      _dbContext = dbContext;
      _throttleService = throttleService;

      _pagesToScrapeQueue = queueConnection.CreateQueue("PagesToScrape");
      _urlsDiscoveredQueue = queueConnection.CreateQueue("UrlsDiscovered");

      int pageExpiryTimeMinutes = configuration.GetValue<int>("PageExpiryTimeMinutes");
      _expireAfter = TimeSpan.FromMinutes(pageExpiryTimeMinutes);
    }

    /// <inheritdoc/>
    public void Start()
    {
      Log.Logger.Information("Starting {className} ({threadId})", nameof(SpiderFeederService), Thread.CurrentThread.ManagedThreadId);

      while (true)
      {
        // Request some items from the db
        var expiredItems = _dbContext.Pages
          .Where(page => page.ExpiryTime < DateTime.Now)
          .ToList();

        if (expiredItems.Any())
        {
          Log.Information("Space in {queueName}, sending {urlCount} pages to crawler", nameof(SpiderFeederService), expiredItems.Count);
        }

        // Iterate each item
        expiredItems.ForEach(page =>
        {
          // Update the expiry time to a few minutes from now so we don't get this more than once by accident
          page.ExpiryTime = DateTime.Now + _expireAfter;
          _dbContext.Pages.Update(page);

          // Send it to the queue
          _pagesToScrapeQueue.SendMessage(page.Url);
        });

        _dbContext.SaveChanges();
        _throttleService.IncRequests();

        Thread.Sleep(1000);
      }
    }
  }
}
