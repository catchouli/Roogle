namespace Roogle.RoogleSpider
{
  /// <summary>
  /// The web spider service, which is fed through the PagesToScrapeQueue,
  /// and outputs through the PagesScrapedQueue and UrlsDiscoveredQueue
  /// </summary>
  public interface IWebSpiderService
  {
    /// <summary>
    /// Start crawling provided urls
    /// </summary>
    void Start();
  }
}
