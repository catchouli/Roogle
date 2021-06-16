using System;

namespace Roogle.RoogleSpider.Models
{
  /// <summary>
  /// The model for a scraped page
  /// </summary>
  public class ScrapedPage
  {
    /// <summary>
    /// The guid for this page, if it has one
    /// </summary>
    public Guid Guid { get; set; }

    /// <summary>
    /// The url of this page
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// The scraped page's content type
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// The page title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The page contents
    /// </summary>
    public string Contents { get; set; }

    /// <summary>
    /// The status code
    /// </summary>
    public int StatusCode { get; set; }
  }
}
