using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace Roogle.RoogleSpider.Db
{
  /// <summary>
  /// A page that's been discovered
  /// </summary>
  [Index(nameof(Word), nameof(Page), IsUnique = true)]
  public class SearchIndexEntry
  {
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// The word on the page, should be normalized to uppercase
    /// </summary>
    [Required]
    public string Word { get; set; }

    /// <summary>
    /// The page containing the text
    /// </summary>
    [Required]
    public Guid Page { get; set; }
  }
}
