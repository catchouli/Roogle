using Microsoft.Extensions.Configuration;
using Roogle.RoogleSpider.Db;
using Roogle.RoogleSpider.Queues;
using Roogle.RoogleSpider.Workers;
using System;
using System.Threading;

namespace Roogle.RoogleSpider.Services
{
  /// <summary>
  /// The spider feeder service
  /// </summary>
  public class SpiderFeederService : WorkerServiceBase, ISpiderFeederService, IDisposable
  {
    /// <summary>
    /// Create a new spider feeder
    /// </summary>
    /// <param name="configuration">The configuration</param>
    /// <param name="dbContext">The db context</param>
    /// <param name="pagesToScrapeQueue">The queue for receiving pages to scrape</param>
    /// <param name="urlsDiscoveredQueue">The queue for sending discovered urls to</param>
    public SpiderFeederService(IConfiguration configuration, RoogleSpiderDbContext dbContext,
      PagesToScrapeQueue pagesToScrapeQueue, LinksDiscoveredQueue urlsDiscoveredQueue)
    {
      int maxItemsInCrawlQueue = configuration.GetValue<int>("MaxItemsInCrawlQueue");
      int pageExpiryTimeMinutes = configuration.GetValue<int>("PageExpiryTimeMinutes");
      Worker = new SpiderFeederWorker(CancellationTokenSource.Token, dbContext, pagesToScrapeQueue,
        urlsDiscoveredQueue, maxItemsInCrawlQueue, pageExpiryTimeMinutes);
    }
  }
}
