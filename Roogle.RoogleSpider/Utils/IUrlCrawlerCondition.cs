namespace Roogle.RoogleSpider.Utils
{
  /// <summary>
  /// An interface that indicates whether or not a given url should be called
  /// </summary>
  public interface IUrlCrawlerCondition
  {
    /// <summary>
    /// Returns whether a given url should be crawled
    /// </summary>
    /// <param name="url">The url</param>
    /// <returns>Whether the url should be crawled</returns>
    bool ShouldCrawl(string url);
  }
}
