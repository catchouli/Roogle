using Newtonsoft.Json;
using Roogle.RoogleSpider.Db;
using Roogle.Shared.Queue;
using Roogle.Shared.Services;
using Serilog;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Roogle.RoogleIndexer
{
  /// <summary>
  /// The indexer worker, responsible for building the search index
  /// </summary>
  public class PageIndexerService : IPageIndexerService
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
    /// The regex for stripping non-alphanumeric characters
    /// </summary>
    private readonly Regex _nonAlphanumericRegex = new Regex("[^A-z0-9]", RegexOptions.Compiled);

    /// <summary>
    /// The pages to index queue
    /// </summary>
    private readonly IQueue _pagesToIndexQueue;

    /// <summary>
    /// Create the indexer worker
    /// </summary>
    /// <param name="dbContext">The db context</param>
    /// <param name="throttleService">The request throttle service</param>
    /// <param name="queueConnection">The pages to index queue</param>
    public PageIndexerService(RoogleSpiderDbContext dbContext, IRequestThrottleService throttleService,
      IQueueConnection queueConnection)
    {
      _dbContext = dbContext;
      _throttleService = throttleService;

      _pagesToIndexQueue = queueConnection.CreateQueue("PagesToIndex");
    }

    /// <inheritdoc/>
    public void Start()
    {
      Log.Logger.Information("Starting {className} ({threadId})", nameof(PageIndexerService), Thread.CurrentThread.ManagedThreadId);

      _pagesToIndexQueue.GetConsumer().Received += (model, args) =>
      {
        string pageUrl = Encoding.UTF8.GetString(args.Body.ToArray());

        Page page = _dbContext.Pages.SingleOrDefault(page => page.Url == pageUrl);

        if (page == null)
        {
          Log.Error("Page {pageUrl} not found in database", pageUrl);
          return;
        }

        Log.Information("Updating index for {pageUrl}", page.Url);

        string fullContents = page.Title + " " + page.Contents;
        string contentsNoPunctuation = _nonAlphanumericRegex.Replace(fullContents, " ").ToUpperInvariant();
        var words = contentsNoPunctuation.Split(' ').Where(word => !string.IsNullOrWhiteSpace(word)).ToHashSet();

        // Clear the existing index for this page
        _dbContext.SearchIndex.RemoveRange(_dbContext.SearchIndex.Where(entry => entry.Page == page.Id));
        _dbContext.SaveChanges();

        _dbContext.SearchIndex.AddRange(words.Select(word =>
        {
          return new SearchIndexEntry
          {
            Page = page.Id,
            Word = word
          };
        }));

        // Mark contents not changed again
        page.ContentsChanged = false;
        _dbContext.Pages.Update(page);

        _dbContext.SaveChanges();
      };
    }
  }
}
