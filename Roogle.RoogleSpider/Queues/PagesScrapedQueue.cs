using Roogle.RoogleSpider.Models;
using System.Collections.Concurrent;

namespace Roogle.RoogleSpider.Queues
{
  /// <summary>
  /// A queue for pages that have been scraped
  /// </summary>
  public class PagesScrapedQueue
  {
    /// <summary>
    /// The queue
    /// </summary>
    public ConcurrentQueue<ScrapedPage> Queue { get; } = new ConcurrentQueue<ScrapedPage>();
  }
}
