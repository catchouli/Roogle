using Microsoft.Extensions.Configuration;
using Roogle.RoogleSpider.Db;
using Roogle.RoogleSpider.Queues;
using Roogle.RoogleSpider.Workers;
using System;
using System.Threading;

namespace Roogle.RoogleSpider.Services
{
  /// <summary>
  /// The scraped page consumer service
  /// </summary>
  public class ScrapedPageConsumerService : WorkerServiceBase, IScrapedPageConsumerService, IDisposable
  {
    /// <summary>
    /// Create a new scraped page consumer
    /// </summary>
    /// <param name="configuration">The configuration</param>
    /// <param name="dbContext">The database context</param>
    /// <param name="pagesScrapedQueue">The scraped pages queue</param>
    public ScrapedPageConsumerService(IConfiguration configuration, RoogleSpiderDbContext dbContext,
      PagesScrapedQueue pagesScrapedQueue)
    {
      int pageExpiryTimeMinutes = configuration.GetValue<int>("PageExpiryTimeMinutes");
      Worker = new ScrapedPageConsumerWorker(CancellationTokenSource.Token, dbContext, pagesScrapedQueue, pageExpiryTimeMinutes);
    }
  }
}
