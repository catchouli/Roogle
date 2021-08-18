using Newtonsoft.Json;
using Roogle.RoogleSpider.Db;
using Roogle.Shared.CrawlConditions;
using Roogle.Shared.Queue;
using Roogle.Shared.Services;
using Serilog;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace Roogle.RoogleUrlConsumer
{
  /// <summary>
  /// The page consumer, which recieves discovered url from the queue
  /// and adds them to the database if they don't already exist
  /// </summary>
  public class DiscoveredUrlConsumerService : IDiscoveredUrlConsumerService
  {
    /// <summary>
    /// The db context
    /// </summary>
    private readonly RoogleSpiderDbContext _dbContext;

    /// <summary>
    /// The queue through which we receive urls that have been discovered
    /// </summary>
    private readonly IQueue _linksDiscoveredQueue;

    /// <summary>
    /// The crawler condition that stops us just endlessly crawling the web
    /// </summary>
    private readonly IUrlCrawlerCondition _crawlerCondition;

    /// <summary>
    /// The url canonicalization service
    /// </summary>
    private readonly ICanonicalUrlService _urlService;

    /// <summary>
    /// The request throttle service
    /// </summary>
    private readonly IRequestThrottleService _throttleService;

    /// <summary>
    /// Constructor
    /// </summary>
    public DiscoveredUrlConsumerService(RoogleSpiderDbContext dbContext, IUrlCrawlerCondition crawlerCondition,
      ICanonicalUrlService urlService, IRequestThrottleService throttleService, IQueueConnection queueConnection)
    {
      _dbContext = dbContext;
      _linksDiscoveredQueue = queueConnection.CreateQueue("UrlsDiscovered");
      _crawlerCondition = crawlerCondition;
      _urlService = urlService;
      _throttleService = throttleService;
    }

    /// <inheritdoc/>
    public void Start()
    {
      Log.Logger.Information("Starting {className} ({threadId})", nameof(DiscoveredUrlConsumerService), Thread.CurrentThread.ManagedThreadId);

      _linksDiscoveredQueue.GetConsumer().Received += (model, args) =>
      {
        var message = Encoding.UTF8.GetString(args.Body.ToArray());

        try
        {
          var discoveredLink = JsonConvert.DeserializeObject<(string, string)>(message);
          ProcessDiscoveredLink(discoveredLink);
        }
        catch (Exception e)
        {
          Log.Error("Error when processing job: {messsage} ({exceptionMessage})", message, e.Message);
        }
      };
    }

    /// <summary>
    /// Process a discovered link
    /// </summary>
    /// <param name="discoveredLink">The discovered link</param>
    public void ProcessDiscoveredLink((string From, string To) discoveredLink)
    {
      Log.Information("Processing discovered url {discoveredLink}", discoveredLink);

      // Ignore links that are too long
      if (discoveredLink.To.Length > 250)
        return;

      // Remove anchors and etc from the links so we don't end up with duplicates in the database
      discoveredLink.From = _urlService.CanonicalizeUrl(discoveredLink.From);
      discoveredLink.To = _urlService.CanonicalizeUrl(discoveredLink.To);

      if (!_crawlerCondition.ShouldCrawl(discoveredLink.To))
      {
        Log.Information("ShouldCrawl == false for url, skipping");
        return;
      }

      // Check if the linked page is already in the db and add it if not
      if (!_dbContext.Pages.Any(page => page.Url == discoveredLink.To))
      {
        Log.Information("Adding discovered url {discoveredUrl} to the db", discoveredLink.To);

        // Add it
        _dbContext.Pages.Add(new Page
        {
          Url = discoveredLink.To,
          ContentType = "",
          Contents = "",
          Title = discoveredLink.To,
          PageHash = 0,
          PageRank = 0,
          ExpiryTime = DateTime.MinValue,
          UpdatedTime = DateTime.MinValue,
          PageRankUpdatedTime = DateTime.MinValue,
          ContentsChanged = false,
          StatusCode = 0
        });
        _dbContext.SaveChanges();
        _throttleService.IncRequests();
      }

      // Get linked page ids
      var fromPageId = _dbContext.Pages.Where(page => page.Url == discoveredLink.From).Select(page => page.Id);
      if (!fromPageId.Any())
      {
        Log.Error("Linked from page was not found in db: {fromUrl}", discoveredLink.From);
        return;
      }

      var toPageId = _dbContext.Pages.Where(page => page.Url == discoveredLink.To).Select(page => page.Id);
      if (!toPageId.Any())
      {
        Log.Error("Linked to page was not found in db: {toUrl}", discoveredLink.To);
        return;
      }

      // Find out if there's a link already stored and if not store it
      var link = _dbContext.Links.SingleOrDefault(link => link.FromPage == fromPageId.Single() && link.ToPage == toPageId.Single());
      if (link != null)
      {
        link.LastSeenTime = DateTime.Now;
        _dbContext.Links.Update(link);
      }
      else
      {
        _dbContext.Links.Add(new Link
        {
          Id = Guid.NewGuid(),
          FromPage = fromPageId.Single(),
          ToPage = toPageId.Single(),
          LastSeenTime = DateTime.Now
        });
      }

      _dbContext.SaveChanges();
      _throttleService.IncRequests();
    }
  }
}
