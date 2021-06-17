using Roogle.RoogleSpider.Utils;

namespace Roogle.RoogleSpider
{
  /// <summary>
  /// A custom crawler condition for roogle
  /// </summary>
  public class RoogleLinkExcluder : IUrlCrawlerCondition
  {
    /// <summary>
    /// The base condition to include talkhaus.com links
    /// </summary>
    private readonly IUrlCrawlerCondition _baseCondition = new BaseHostUrlCrawlerCondition("talkhaus.com");

    /// <summary>
    /// Exclude some wiki links and include other talkhaus.com links
    /// </summary>
    /// <param name="url">The url</param>
    /// <returns>Whether we should crawl the url</returns>
    public bool ShouldCrawl(string url)
    {
      // Exclude non-wiki-entry wiki links, it results in too many entries
      if (url.StartsWith("https://wiki.talkhaus.com/") && !url.StartsWith("https://wiki.talkhaus.com/wiki/"))
        return false;

      return _baseCondition.ShouldCrawl(url);
    }
  }
}
