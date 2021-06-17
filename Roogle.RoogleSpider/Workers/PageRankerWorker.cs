using Roogle.RoogleSpider.Db;
using Serilog;
using System;
using System.Linq;
using System.Threading;

namespace Roogle.RoogleSpider.Workers
{
  /// <summary>
  /// The ranker worker, responsible for building the search ranks
  /// </summary>
  public class PageRankerWorker : IWorker
  {
    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// The db context
    /// </summary>
    private readonly RoogleSpiderDbContext _dbContext;

    /// <summary>
    /// The expiry time for pageranks
    /// </summary>
    private readonly TimeSpan _pageRankExpiryTime;

    /// <summary>
    /// Create the ranker worker
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <param name="dbContext">The db context</param>
    /// <param name="pageRankExpiryTimeMinutes">The expiry time for page ranks</param>
    public PageRankerWorker(CancellationToken cancellationToken, RoogleSpiderDbContext dbContext,
      int pageRankExpiryTimeMinutes)
    {
      _cancellationToken = cancellationToken;
      _dbContext = dbContext;
      _pageRankExpiryTime = TimeSpan.FromMinutes(pageRankExpiryTimeMinutes);
    }

    /// <summary>
    /// The thread entrypoint
    /// </summary>
    public void ThreadProc()
    {
      Log.Logger.Information("Starting {className} ({threadId})", nameof(PageRankerWorker), Thread.CurrentThread.ManagedThreadId);

      while (!_cancellationToken.IsCancellationRequested)
      {
        Page page = null;
        while ((page = GetNextUnrankedPage()) != null)
        {
          Log.Information("Updating page rank for {pageUrl}", page.Url);

          page.PageRank = _dbContext.Links.Count(link => link.ToPage == page.Id);

          // A quick hack - derank wiki pages significantly so that user sites show up higehr
          if (page.Url.StartsWith("https://wiki.talkhaus.com"))
            page.PageRank -= 1000000;

          page.PageRankUpdatedTime = DateTime.Now;
          _dbContext.Pages.Update(page);
          _dbContext.SaveChanges();
        }

        Thread.Sleep(100);
      }

      Log.Logger.Information("{className} thread {threadId} edited", nameof(PageRankerWorker), Thread.CurrentThread.ManagedThreadId);
    }

    /// <summary>
    /// Gets the next unranked page (or page where the pagerank has expired)
    /// </summary>
    /// <returns>The next page to update the rank of</returns>
    private Page GetNextUnrankedPage()
    {
      var oldestTime = DateTime.Now - _pageRankExpiryTime;
      var x = _dbContext.Pages.FirstOrDefault(page => page.PageRankUpdatedTime < oldestTime);
      var updated = x?.PageRankUpdatedTime;
      return x;
    }
  }
}
