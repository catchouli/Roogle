using Roogle.RoogleFrontend.Models;
using System.Collections.Generic;

namespace Roogle.RoogleFrontend.Services
{
  /// <summary>
  /// The search service
  /// </summary>
  public interface ISearchService
  {
    /// <summary>
    /// Search with a query
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="page">The page (zero indexed)</param>
    /// <returns>The results</returns>
    SearchResults Search(string query, int page);

    /// <summary>
    /// Gets random pages
    /// </summary>
    /// <param name="page">The page (zero indexed)</param>
    /// <returns>The results</returns>
    SearchResults GetRandomPages(int page);

    /// <summary>
    /// Gets recently updated pages
    /// </summary>
    /// <param name="page">The page (zero indexed)</param>
    /// <returns>The results</returns>
    SearchResults GetRecentUpdates(int page);
  }
}
