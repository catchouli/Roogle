using Roogle.RoogleSpider.Db;
using Roogle.RoogleSpider.Queues;
using Roogle.RoogleSpider.Utils;
using Roogle.RoogleSpider.Workers;
using System;
using System.Threading;

namespace Roogle.RoogleSpider.Services
{
  /// <summary>
  /// The spider feeder service
  /// </summary>
  public class DiscoveredUrlConsumerService : WorkerServiceBase, IDiscoveredUrlConsumerService, IDisposable
  {
    /// <summary>
    /// Create a new spider feeder
    /// </summary>
    /// <param name="dbContext">The db context</param>
    /// <param name="urlsDiscoveredQueue">The queue for recieving discovered urls to</param>
    /// <param name="crawlerCondition">The url crawler condition that stops us crawling the whole web</param>
    public DiscoveredUrlConsumerService(RoogleSpiderDbContext dbContext, LinksDiscoveredQueue urlsDiscoveredQueue,
      IUrlCrawlerCondition crawlerCondition)
    {
      Worker = new DiscoveredUrlConsumerWorker(CancellationTokenSource.Token, dbContext, urlsDiscoveredQueue,
        crawlerCondition);
    }
  }
}
