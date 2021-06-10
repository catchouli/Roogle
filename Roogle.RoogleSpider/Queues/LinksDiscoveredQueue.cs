using Roogle.RoogleSpider.Db;
using System.Collections.Concurrent;

namespace Roogle.RoogleSpider.Queues
{
  /// <summary>
  /// A queue for urls that have been discovered by the spider
  /// </summary>
  public class LinksDiscoveredQueue
  {
    /// <summary>
    /// The queue
    /// </summary>
    public ConcurrentQueue<(string From, string To)> Queue { get; } = new ConcurrentQueue<(string, string)>();
  }
}
