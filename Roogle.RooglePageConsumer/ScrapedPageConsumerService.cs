using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Roogle.RoogleSpider.Db;
using Roogle.Shared.Models;
using Roogle.Shared.Queue;
using Roogle.Shared.Services;
using Serilog;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace Roogle.RooglePageConsumer
{
  /// <summary>
  /// The page consumer, which recieves scraped pages from the queue
  /// and updates them in the database
  /// </summary>
  public class ScrapedPageConsumerService : IScrapedPageConsumerService
  {
    /// <summary>
    /// The expiry time
    /// </summary>
    private readonly TimeSpan _expireAfter;

    /// <summary>
    /// The db context
    /// </summary>
    private readonly RoogleSpiderDbContext _dbContext;

    /// <summary>
    /// The queue through which we receive pages that have been scraped by the spider
    /// </summary>
    private readonly IQueue _pagesScrapedQueue;

    /// <summary>
    /// Pages to index queue
    /// </summary>
    private readonly IQueue _pagesToIndexQueue;

    /// <summary>
    /// Pages to rank queue
    /// </summary>
    private readonly IQueue _pagesToRankQueue;

    /// <summary>
    /// The request throttle service
    /// </summary>
    private readonly IRequestThrottleService _throttleService;

    /// <summary>
    /// The random number generato
    /// </summary>
    private readonly Random _rng = new Random();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configuration">The config</param>
    /// <param name="dbContext">The db context</param>
    /// <param name="throttleService">The request throttle service</param>
    /// <param name="queueConnection">The queue connection</param>
    public ScrapedPageConsumerService(IConfiguration configuration, RoogleSpiderDbContext dbContext,
      IRequestThrottleService throttleService, IQueueConnection queueConnection)
    {
      _dbContext = dbContext;
      _throttleService = throttleService;

      _pagesScrapedQueue = queueConnection.CreateQueue("PagesScraped");
      _pagesToIndexQueue = queueConnection.CreateQueue("PagesToIndex");
      _pagesToRankQueue = queueConnection.CreateQueue("PagesToRank");

      int pageExpiryTimeMinutes = configuration.GetValue<int>("PageExpiryTimeMinutes");
      _expireAfter = TimeSpan.FromMinutes(pageExpiryTimeMinutes);
    }

    /// <summary>
    /// Start the ScrapedPageConsumerService
    /// </summary>
    public void Start()
    {
      Log.Logger.Information("Starting {className} ({threadId})", nameof(ScrapedPageConsumerService), Thread.CurrentThread.ManagedThreadId);

      _pagesScrapedQueue.GetConsumer().Received += (model, args) =>
      {
        var message = Encoding.UTF8.GetString(args.Body.ToArray());

        try
        {
          var scrapedPage = JsonConvert.DeserializeObject<ScrapedPage>(message);
          ProcessScrapedPage(scrapedPage);
        }
        catch (Exception e)
        {
          Log.Error("Error when processing job: {messsage} ({exceptionMessage})", message, e.Message);
        }
      };
    }

    /// <summary>
    /// Process a scraped page
    /// </summary>
    /// <param name="scrapedPage">The scraped page</param>
    public void ProcessScrapedPage(ScrapedPage scrapedPage)
    {
      Log.Information("Processing scraped page {pageTitle}", scrapedPage.Title);

      // Get existing page in db (it should exist by now if it's been fed to the crawler with a guid)
      var page = _dbContext.Pages.SingleOrDefault(page => page.Url == scrapedPage.Url);

      if (page == null)
      {
        Log.Error("Scraped page does not exist in db");
        return;
      }

      // Calculate new page hash and check if it's been updated
      // If it hasn't changed, update everything
      int pageHash = CalculatePageHash(scrapedPage);
      Log.Error("Checking if page changed");
      bool pageUpdated = (pageHash != page.PageHash);
      if (pageUpdated)
      {
        Log.Error("Page changed (old={oldHash}, new={newHash}), updating contents", page.PageHash, pageHash);
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

      // Now that we saved it, let's notify the PagesToIndex and PagesToRank queue
      if (pageUpdated)
      {
        Log.Information("Page updated, notifying PagesToIndex and PagesToRank queue");
        _pagesToIndexQueue.SendMessage(page.Url);
        _pagesToRankQueue.SendMessage(page.Url);
      }

      _throttleService.IncRequests();
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
        hash = hash * 16777619 ^ GetStableHashCode(scrapedPage?.Title ?? "");
        hash = hash * 16777619 ^ GetStableHashCode(scrapedPage?.Contents ?? "");
        hash = hash * 16777619 ^ GetStableHashCode(scrapedPage?.ContentType ?? "");
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
          hash1 = (hash1 << 5) + hash1 ^ str[i];
          if (i == str.Length - 1 || str[i + 1] == '\0')
            break;
          hash2 = (hash2 << 5) + hash2 ^ str[i + 1];
        }

        return hash1 + hash2 * 1566083941;
      }
    }
  }
}
