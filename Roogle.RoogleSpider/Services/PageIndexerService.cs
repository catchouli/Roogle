using Microsoft.Extensions.Configuration;
using Roogle.RoogleSpider.Db;
using Roogle.RoogleSpider.Workers;
using System;
using System.Threading;

namespace Roogle.RoogleSpider.Services
{
  /// <summary>
  /// The page indexer service
  /// </summary>
  public class PageIndexerService : WorkerServiceBase, IPageIndexerService, IDisposable
  {
    /// <summary>
    /// Create a new page indexer service
    /// </summary>
    /// <param name="dbContext">The database context</param>
    public PageIndexerService(RoogleSpiderDbContext dbContext)
    {
      Worker = new PageIndexerWorker(CancellationTokenSource.Token, dbContext);
    }
  }
}
