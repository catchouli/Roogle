using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Roogle.RoogleSpider.Db;
using Roogle.Shared.Queue;
using Roogle.Shared.Services;
using Serilog;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace Roogle.RooglePageRanker
{
  /// <summary>
  /// The ranker worker, responsible for building the search ranks
  /// </summary>
  public class PageRankerService : IPageRankerService
  {
    /// <summary>
    /// The db context
    /// </summary>
    private readonly RoogleSpiderDbContext _dbContext;

    /// <summary>
    /// The request throttle service
    /// </summary>
    private readonly IRequestThrottleService _throttleService;

    /// <summary>
    /// The pages to rank queue
    /// </summary>
    private readonly IQueue _pagesToRankQueue;

    /// <summary>
    /// Create the ranker worker
    /// </summary>
    /// <param name="configuration">The config</param>
    /// <param name="dbContext">The db context</param>
    /// <param name="throttleService">The request throttle service</param>
    /// <param name="queueConnection">The queue connection</param>
    public PageRankerService(IConfiguration configuration, RoogleSpiderDbContext dbContext,
      IRequestThrottleService throttleService, IQueueConnection queueConnection)
    {
      _dbContext = dbContext;
      _throttleService = throttleService;

      _pagesToRankQueue = queueConnection.CreateQueue("PagesToRank");
    }

    /// <inheritdoc/>
    public void Start()
    {
      Log.Logger.Information("Starting {className} ({threadId})", nameof(PageRankerService), Thread.CurrentThread.ManagedThreadId);

      _pagesToRankQueue.GetConsumer().Received += (model, args) =>
      {
        string pageUrl = Encoding.UTF8.GetString(args.Body.ToArray());

        Page page = _dbContext.Pages.SingleOrDefault(page => page.Url == pageUrl);

        if (page == null)
        {
          Log.Error("Page {pageUrl} not found in database", pageUrl);
          return;
        }

        Log.Information("Updating page rank for {pageUrl}", page.Url);

        page.PageRank = _dbContext.Links.Count(link => link.ToPage == page.Id);

        // A quick hack - derank wiki pages significantly so that user sites show up higher
        if (page.Url.StartsWith("https://wiki.talkhaus.com"))
          page.PageRank -= 1000000;

        page.PageRankUpdatedTime = DateTime.Now;
        _dbContext.Pages.Update(page);
        _dbContext.SaveChanges();
      };
    }
  }
}
