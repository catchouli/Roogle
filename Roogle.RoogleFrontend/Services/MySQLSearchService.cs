using Microsoft.EntityFrameworkCore;
using Roogle.RoogleFrontend.Models;
using Roogle.RoogleSpider.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Roogle.RoogleFrontend.Services
{
  /// <summary>
  /// The MySQL search service
  /// </summary>
  public class MySQLSearchService : ISearchService
  {
    /// <summary>
    /// The results to show per page
    /// </summary>
    private const int ResultsPerPage = 10;

    /// <summary>
    /// The db context
    /// </summary>
    private readonly RoogleSpiderDbContext _dbContext;

    /// <summary>
    /// The random number generator
    /// </summary>
    private readonly Random _random;

    /// <summary>
    /// The regex for stripping non-alphanumeric characters
    /// </summary>
    private readonly Regex _nonAlphanumericRegex = new Regex("[^A-z0-9]", RegexOptions.Compiled);

    /// <summary>
    /// Create the mysql search service
    /// </summary>
    /// <param name="dbContext">The db context</param>
    public MySQLSearchService(RoogleSpiderDbContext dbContext)
    {
      _dbContext = dbContext;
      _random = new Random();
    }

    /// <inheritdoc/>
    public SearchResults GetRandomPages(int page)
    {
      int count = _dbContext.Pages
        .Where(page => EF.Functions.Like(page.ContentType, "text/html%"))
        .Count();

      // A random offset for random pages
      int offset = _random.Next(0, Math.Max(0, count - ResultsPerPage));

      var pages = _dbContext.Pages
        .Where(page => EF.Functions.Like(page.ContentType, "text/html%"))
        .Skip(offset)
        .Take(ResultsPerPage)
        .ToList();

      // Shuffle it now it's not an efcore query anymore
      pages = pages.OrderBy(_ => Guid.NewGuid()).ToList();

      return new SearchResults
      {
        Pages = pages,
        CurrentPage = page,
        TotalResults = count,
        ResultsPerPage = ResultsPerPage
      };
    }

    /// <inheritdoc/>
    public SearchResults GetRecentUpdates(int page)
    {
      int count = _dbContext.Pages
        .Where(page => EF.Functions.Like(page.ContentType, "text/html%"))
        .Count();

      var pages = _dbContext.Pages
        .Where(page => EF.Functions.Like(page.ContentType, "text/html%"))
        .OrderByDescending(page => page.UpdatedTime)
        .Skip(page * ResultsPerPage)
        .Take(ResultsPerPage)
        .ToList();

      return new SearchResults
      {
        Pages = pages,
        CurrentPage = page,
        TotalResults = count,
        ResultsPerPage = ResultsPerPage
      };
    }

    /// <inheritdoc/>
    public SearchResults Search(string query, int page)
    {
      // Convert query to uppercase words
      var queryWords = _nonAlphanumericRegex.Replace(query, " ")
        .ToUpperInvariant()
        .Split(' ')
        .Where(word => !string.IsNullOrWhiteSpace(word))
        .ToHashSet();

      // Select pages that contain all provided words
      var pageIds = _dbContext.SearchIndex
        .Where(entry => queryWords.Contains(entry.Word))
        .GroupBy(entry => entry.Page)
        .Select(g => new { Id = g.Key, Count = g.Count() })
        .Where(g => g.Count == queryWords.Count)
        .Select(g => g.Id)
        .ToHashSet();

      // Select pages and do pagination
      var pages = _dbContext.Pages
        .Where(page => pageIds.Contains(page.Id))
        .OrderByDescending(page => page.PageRank)
        .Skip(page * ResultsPerPage)
        .Take(ResultsPerPage)
        .ToList();

      return new SearchResults
      {
        Pages = pages,
        CurrentPage = page,
        TotalResults = pageIds.Count,
        ResultsPerPage = ResultsPerPage
      };
    }
  }
}
