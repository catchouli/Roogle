using Roogle.RoogleSpider.Db;
using Roogle.RoogleSpider.Queues;
using Roogle.RoogleSpider.Utils;
using Serilog;
using System;
using System.Linq;
using System.Threading;

namespace Roogle.RoogleSpider.Workers
{
  /// <summary>
  /// The web spider worker
  /// </summary>
  public class SpiderFeederWorker : IWorker
  {
    /// <summary>
    /// Max items to add to the queue at once
    /// </summary>
    private readonly int _maxItemsInQueue;

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
    private readonly PagesToScrapeQueue _pagesToScrapeQueue;

    /// <summary>
    /// Temp, the urls discovered queue
    /// </summary>
    private readonly LinksDiscoveredQueue _urlsDiscoveredQueue;

    /// <summary>
    /// The db context
    /// </summary>
    private readonly RoogleSpiderDbContext _dbContext;

    /// <summary>
    /// Create the web spider worker
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <param name="dbContext">The db context</param>
    /// <param name="pagesToScrapeQueue">The incoming queue for pages to scrape</param>
    /// <param name="urlsDiscoveredQueue">The urls discovered queue</param>
    /// <param name="maxItemsInCrawlQueue">The maximum number of items in the crawl queue</param>
    /// <param name="pageExpiryTimeMinutes">The expiry time before which we want to recrawl a page</param>
    public SpiderFeederWorker(CancellationToken cancellationToken, RoogleSpiderDbContext dbContext,
      PagesToScrapeQueue pagesToScrapeQueue, LinksDiscoveredQueue urlsDiscoveredQueue, int maxItemsInCrawlQueue,
      int pageExpiryTimeMinutes)
    {
      _cancellationToken = cancellationToken;
      _dbContext = dbContext;
      _pagesToScrapeQueue = pagesToScrapeQueue;
      _urlsDiscoveredQueue = urlsDiscoveredQueue;
      _maxItemsInQueue = maxItemsInCrawlQueue;
      _expireAfter = TimeSpan.FromMinutes(pageExpiryTimeMinutes);
    }

    /// <summary>
    /// The thread entrypoint
    /// </summary>
    public void ThreadProc()
    {
      while (!_cancellationToken.IsCancellationRequested)
      {
        if (_pagesToScrapeQueue.Queue.Count < _maxItemsInQueue)
        {
          int maxToSend = _maxItemsInQueue - _pagesToScrapeQueue.Queue.Count;

          // Request some items from the db
          var expiredItems = _dbContext.Pages.Where(page => page.ExpiryTime < DateTime.Now).Take(maxToSend).ToList();

          if (expiredItems.Any())
          {
            Log.Information("Space in {queueName}, sending {urlCount} pages to crawler", nameof(PagesToScrapeQueue), expiredItems.Count);
          }

          // Iterate each item
          expiredItems.ForEach(page =>
          {
            // Update the expiry time to a few minutes from now so we don't get this more than once by accident
            page.ExpiryTime = DateTime.Now + _expireAfter;
            _dbContext.Pages.Update(page);

            // Send it to the queue
            _pagesToScrapeQueue.Queue.Enqueue((page.Id, page.Url));
          });
          _dbContext.SaveChanges();
        }

        Thread.Sleep(1000);
      }

      Log.Logger.Information("{className} thread {threadId} edited", nameof(SpiderFeederWorker), Thread.CurrentThread.ManagedThreadId);
    }
  }
}
