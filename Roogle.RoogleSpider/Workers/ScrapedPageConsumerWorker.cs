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
    /// The random number generato
    /// </summary>
    private readonly Random _rng = new Random();

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
        page.StatusCode = scrapedPage.StatusCode;
      }

      // Update the expiry time regardless and save changes
      page.ExpiryTime = DateTime.Now + _expireAfter + TimeSpan.FromSeconds(_rng.Next(-60, 60));
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
      // https://stackoverflow.com/a/263416/1474712
      unchecked // Overflow is fine, just wrap
      {
        int hash = (int)2166136261;
        // Use stable hash codes, the hash codes for strings in .net aren't persistent...
        hash = (hash * 16777619) ^ GetStableHashCode(scrapedPage?.Title ?? "");
        hash = (hash * 16777619) ^ GetStableHashCode(scrapedPage?.Contents ?? "");
        hash = (hash * 16777619) ^ GetStableHashCode(scrapedPage?.ContentType ?? "");
        return hash;
      }
    }

    /// <summary>
    /// Get a stable hashcode for a string (no randomization etc)
    /// </summary>
    /// <param name="str">The string</param>
    /// <returns>The hash code</returns>
    public static int GetStableHashCode(string str)
    {
      unchecked
      {
        int hash1 = 5381;
        int hash2 = hash1;

        for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
        {
          hash1 = ((hash1 << 5) + hash1) ^ str[i];
          if (i == str.Length - 1 || str[i + 1] == '\0')
            break;
          hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
        }

        return hash1 + (hash2 * 1566083941);
      }
    }
  }
}
