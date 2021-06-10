using Roogle.RoogleSpider.Queues;
using Roogle.RoogleSpider.Workers;
using System;
using System.Threading;

namespace Roogle.RoogleSpider.Services
{
  /// <summary>
  /// The web spider service
  /// </summary>
  public class WebSpiderService : WorkerServiceBase, IWebSpiderService, IDisposable
  {
    /// <summary>
    /// Create a new web spider
    /// </summary>
    /// <param name="pagesToScrapeQueue">The queue for receiving pages to scrape</param>
    /// <param name="pagesScrapedQueue">The queue for sending scraped pages back out</param>
    /// <param name="urlsDiscoveredQueue">The queue for sending discovered urls to</param>
    public WebSpiderService(IRequestThrottleService throttleService, PagesToScrapeQueue pagesToScrapeQueue,
      PagesScrapedQueue pagesScrapedQueue, LinksDiscoveredQueue urlsDiscoveredQueue)
    {
      Worker = new WebSpiderWorker(throttleService, CancellationTokenSource.Token,
        pagesToScrapeQueue, pagesScrapedQueue, urlsDiscoveredQueue);
    }
  }
}
