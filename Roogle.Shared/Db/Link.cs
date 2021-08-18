using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace Roogle.RoogleSpider.Db
{
  /// <summary>
  /// A page that's been discovered
  /// </summary>
  [Index(nameof(FromPage), nameof(ToPage), IsUnique = true)]
  public class Link
  {
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// The page containing the link
    /// </summary>
    [Required]
    public Guid FromPage { get; set; }

    /// <summary>
    /// The page linked to
    /// </summary>
    [Required]
    public Guid ToPage { get; set; }

    /// <summary>
    /// The last time this link was seen
    /// </summary>
    [Required]
    public DateTime LastSeenTime { get; set; }
  }
}
