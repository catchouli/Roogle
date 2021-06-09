using System;

namespace Roogle.RoogleSpider.Db
{
  /// <summary>
  /// A page that's been discovered
  /// </summary>
  public class Page
  {
    /// <summary>
    /// The page url
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// The page contents
    /// </summary>
    public string Contents { get; set; }

    /// <summary>
    /// The hash of the page contents
    /// </summary>
    public int ContentsHash { get; set; }

    /// <summary>
    /// The expiry time
    /// </summary>
    public DateTime ExpiryTime { get; set; }
  }
}
