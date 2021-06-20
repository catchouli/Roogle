using Roogle.RoogleSpider.Db;
using Roogle.RoogleSpider.Services;
using Serilog;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Roogle.RoogleSpider.Workers
{
  /// <summary>
  /// The indexer worker, responsible for building the search index
  /// </summary>
  public class PageIndexerWorker : IWorker
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
    /// The request throttle service
    /// </summary>
    private readonly IRequestThrottleService _throttleService;

    /// <summary>
    /// The regex for stripping non-alphanumeric characters
    /// </summary>
    private readonly Regex _nonAlphanumericRegex = new Regex("[^A-z0-9]", RegexOptions.Compiled);

    /// <summary>
    /// Create the indexer worker
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <param name="dbContext">The db context</param>
    public PageIndexerWorker(CancellationToken cancellationToken, RoogleSpiderDbContext dbContext,
      IRequestThrottleService throttleService)
    {
      _cancellationToken = cancellationToken;
      _dbContext = dbContext;
      _throttleService = throttleService;
    }

    /// <summary>
    /// The thread entrypoint
    /// </summary>
    public void ThreadProc()
    {
      Log.Logger.Information("Starting {className} ({threadId})", nameof(PageIndexerWorker), Thread.CurrentThread.ManagedThreadId);

      while (!_cancellationToken.IsCancellationRequested)
      {
        Page page = null;
        while ((page = GetNextUnindexedPage()) != null)
        {
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
          _throttleService.IncRequests();
          _throttleService.IncRequests();
        }

        Thread.Sleep(100);
      }

      Log.Logger.Information("{className} thread {threadId} edited", nameof(PageIndexerWorker), Thread.CurrentThread.ManagedThreadId);
    }

    /// <summary>
    /// Gets the next unindexed page (or page where the contents have changed)
    /// </summary>
    /// <returns>The next page to update the index of</returns>
    private Page GetNextUnindexedPage()
    {
      return _dbContext.Pages.FirstOrDefault(page => page.ContentsChanged);
    }

  }
}
