using System;
using System.Linq;

namespace Roogle.RoogleSpider.Utils
{
  /// <summary>
  /// A url crawler condition class that only crawls urls from a given base hostname
  /// </summary>
  public class BaseHostUrlCrawlerCondition : IUrlCrawlerCondition
  {
    /// <summary>
    /// The required host parts (starting from the right), which means we can match
    /// e.g. "talkhaus.com" base host matches "blah.talkhaus.com" as well as just "talkhaus.com"
    /// </summary>
    private readonly string[] _requiredHostParts;

    /// <summary>
    /// Create the base host url crawler condition
    /// </summary>
    /// <param name="baseHostname">The base hostname</param>
    public BaseHostUrlCrawlerCondition(string baseHostname)
    {
      _requiredHostParts = baseHostname.Split('.');
    }

    /// <summary>
    /// Returns whether the given url matches the base url
    /// </summary>
    /// <param name="url">The url</param>
    /// <returns>Whether the url should be crawled</returns>
    public bool ShouldCrawl(string url)
    {
      // Parse uri
      var uri = new Uri(url);

      // Hacky way of getting the last n parts
      var uriHostParts = uri.Host.Split('.').Reverse().Take(_requiredHostParts.Length).Reverse();

      return uriHostParts.SequenceEqual(_requiredHostParts);
    }
  }
}
