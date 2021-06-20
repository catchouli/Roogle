using Microsoft.Extensions.Configuration;
using Roogle.RoogleSpider.Db;
using Roogle.RoogleSpider.Workers;
using System;
using System.Threading;

namespace Roogle.RoogleSpider.Services
{
  /// <summary>
  /// The page ranker service
  /// </summary>
  public class PageRankerService : WorkerServiceBase, IPageRankerService, IDisposable
  {
    /// <summary>
    /// Create a new page ranker service
    /// </summary>
    /// <param name="configuration">The configuration</param>
    /// <param name="dbContext">The database context</param>
    public PageRankerService(IConfiguration configuration, RoogleSpiderDbContext dbContext, IRequestThrottleService throttleService)
    {
      int pageRankExpiryTimeMinutes = configuration.GetValue<int>("PageRankExpiryTimeMinutes");
      Worker = new PageRankerWorker(CancellationTokenSource.Token, dbContext, pageRankExpiryTimeMinutes, throttleService);
    }
  }
}
