using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace Roogle.RoogleSpider.Db
{
  /// <summary>
  /// A page that's been discovered
  /// </summary>
  [Index(nameof(Url), IsUnique = true)]
  public class Page
  {
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// The page url
    /// </summary>
    [Required]
    public string Url { get; set; }

    /// <summary>
    /// The page content type
    /// </summary>
    [Required]
    public string ContentType { get; set; }

    /// <summary>
    /// The page title
    /// </summary>
    [Required]
    public string Title { get; set; }

    /// <summary>
    /// The page contents
    /// </summary>
    [Required]
    public string Contents { get; set; }

    /// <summary>
    /// The hash of the page contents and title etc
    /// </summary>
    [Required]
    public int PageHash { get; set; }

    /// <summary>
    /// The pagerank of this page
    /// </summary>
    [Required]
    public int PageRank { get; set; }

    /// <summary>
    /// The expiry time
    /// </summary>
    [Required]
    public DateTime ExpiryTime { get; set; }

    /// <summary>
    /// The last time an update to this page was discovered
    /// </summary>
    [Required]
    public DateTime UpdatedTime { get; set; }

    /// <summary>
    /// Whether the contents of this page have changed and the ngrams need to be updated
    /// </summary>
    [Required]
    public bool ContentsChanged { get; set; }

    /// <summary>
    /// Whether of the pagerank of this page needs updating
    /// </summary>
    [Required]
    public bool PageRankDirty { get; set; }
  }
}
