using Roogle.RoogleSpider.Db;
using Roogle.RoogleSpider.Models;
using Roogle.RoogleSpider.Queues;
using Serilog;
using System;
using System.Threading;

namespace Roogle.RoogleSpider.Workers
{
  /// <summary>
  /// The page consumer, which recieves scraped pages from the queue
  /// and updates them in the database
  /// </summary>
  public class ScrapedPageConsumerWorker : IWorker
  {
    /// <summary>
    /// The expiry time
    /// </summary>
    private readonly TimeSpan _expireAfter;

    /// <summary>
    /// The thread cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// The db context
    /// </summary>
    private readonly RoogleSpiderDbContext _dbContext;

    /// <summary>
    /// The queue through which we receive pages that have been scraped by the spider
    /// </summary>
    private readonly PagesScrapedQueue _pagesScrapedQueue;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for our worker thread proc</param>
    /// <param name="dbContext">The db context</param>
    /// <param name="pagesScrapedQueue">The queue for receiving scraped pages from the spider</param>
    /// <param name="pageExpiryTimeMinutes">The time after which we should expire cached scrape results</param>
    public ScrapedPageConsumerWorker(CancellationToken cancellationToken, RoogleSpiderDbContext dbContext,
      PagesScrapedQueue pagesScrapedQueue, int pageExpiryTimeMinutes)
    {
      _cancellationToken = cancellationToken;
      _dbContext = dbContext;
      _pagesScrapedQueue = pagesScrapedQueue;
      _expireAfter = TimeSpan.FromMinutes(pageExpiryTimeMinutes);
    }

    /// <summary>
    /// The thread entry point
    /// </summary>
    public void ThreadProc()
    {
      Log.Logger.Information("Starting {className} ({threadId})", nameof(ScrapedPageConsumerWorker), Thread.CurrentThread.ManagedThreadId);

      while (!_cancellationToken.IsCancellationRequested)
      {
        while (_pagesScrapedQueue.Queue.TryDequeue(out var scrapedPage))
        {
          ProcessScrapedPage(scrapedPage);
        }

        Thread.Sleep(100);
      }

      Log.Logger.Information("{className} thread {threadId} edited", nameof(ScrapedPageConsumerWorker), Thread.CurrentThread.ManagedThreadId);
    }

    /// <summary>
    /// Process a scraped page
    /// </summary>
    /// <param name="scrapedPage">The scraped page</param>
    public void ProcessScrapedPage(ScrapedPage scrapedPage)
    {
      Log.Information("Processing scraped page {pageTitle}", scrapedPage.Title);

      // Get existing page in db (it should exist by now if it's been fed to the crawler with a guid)
      var page = _dbContext.Pages.Find(scrapedPage.Guid);

      if (page == null)
      {
        Log.Error("Scraped page does not exist in db");
        return;
      }

      // Calculate new page hash and check if it's been updated
      // If it hasn't changed, update everything
      int pageHash = CalculatePageHash(scrapedPage);
      Log.Error("Checking if page changed");
      if (pageHash != page.PageHash)
      {
        Log.Error("Page changed, updating contents");
        page.ContentType = scrapedPage.ContentType;
        page.Title = scrapedPage.Title;
        page.Contents = scrapedPage.Contents;
        page.PageHash = pageHash;
        page.UpdatedTime = DateTime.Now;
        page.ContentsChanged = true;
      }

      // Update the expiry time regardless and save changes
      page.ExpiryTime = DateTime.Now + _expireAfter;
      _dbContext.Pages.Update(page);
      _dbContext.SaveChanges();
    }

    /// <summary>
    /// Get the hash of a scraped page
    /// </summary>
    /// <param name="scrapedPage">The scraped page</param>
    /// <returns>The hash</returns>
    private static int CalculatePageHash(ScrapedPage scrapedPage)
    {
      return HashCode.Combine(scrapedPage.ContentType, scrapedPage.Title, scrapedPage.Contents);
    }
  }
}
