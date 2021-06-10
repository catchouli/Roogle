using Roogle.RoogleSpider.Db;
using System.Collections.Generic;

namespace Roogle.RoogleFrontend.Models
{
  /// <summary>
  /// The search results data structure
  /// </summary>
  public class SearchResults
  {
    /// <summary>
    /// The pages in the current result page
    /// </summary>
    public IList<Page> Pages { get; set; }

    /// <summary>
    /// The current page
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// The total pages
    /// </summary>
    public int TotalResults { get; set; }

    /// <summary>
    /// The number of results per page
    /// </summary>
    public int ResultsPerPage { get; set; }
  }
}
