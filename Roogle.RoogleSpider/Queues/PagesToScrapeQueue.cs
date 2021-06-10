using System;
using System.Collections.Concurrent;

namespace Roogle.RoogleSpider.Queues
{
  /// <summary>
  /// A queue for pages to scrape
  /// </summary>
  public class PagesToScrapeQueue
  {
    /// <summary>
    /// The queue
    /// </summary>
    public ConcurrentQueue<(Guid, string)> Queue { get; } = new ConcurrentQueue<(Guid, string)>();
  }
}
