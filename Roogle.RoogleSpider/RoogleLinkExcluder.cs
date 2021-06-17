using Roogle.RoogleSpider.Utils;
using System.Linq;

namespace Roogle.RoogleSpider
{
  /// <summary>
  /// A custom crawler condition for roogle
  /// </summary>
  public class RoogleLinkExcluder : IUrlCrawlerCondition
  {
    private readonly string[] ExcludePrefixes = new[]
    {
      "https://wiki.talkhaus.com/index.php",
      "https://wiki.talkhaus.com/api.php",
      "https://wiki.talkhaus.com/wiki/special:",
      "https://wiki.talkhaus.com/wiki/module:",
      "https://wiki.talkhaus.com/wiki/template:"
    };

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
      if (ExcludePrefixes.Any(prefix => url.ToLowerInvariant().StartsWith(prefix)))
        return false;

      return _baseCondition.ShouldCrawl(url);
    }
  }
}
