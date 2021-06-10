namespace Roogle.RoogleSpider.Utils
{
  /// <summary>
  /// A url crawler condition that crawls any url
  /// </summary>
  public class AnyUrlCrawlerCondition : IUrlCrawlerCondition
  {
    /// <summary>
    /// Always returns true because this crawler condition accepts any url
    /// </summary>
    /// <param name="url">ignored</param>
    /// <returns>true</returns>
    public bool ShouldCrawl(string url)
    {
      return true;
    }
  }
}
