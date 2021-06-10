using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Roogle.RoogleFrontend.Services;
using System.Linq;

namespace Roogle.RoogleFrontend.Controllers
{
  /// <summary>
  /// The main controller
  /// </summary>
  public class HomeController : Controller
  {
    /// <summary>
    /// The search modes
    /// </summary>
    public enum SearchMode
    {
      Search,
      RecentUpdates,
      RandomPages
    }

    /// <summary>
    /// The logger
    /// </summary>
    private readonly ILogger<HomeController> _logger;

    /// <summary>
    /// The search service
    /// </summary>
    private readonly ISearchService _searchService;

    /// <summary>
    /// The constructor
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="searchService">The search service</param>
    public HomeController(ILogger<HomeController> logger, ISearchService searchService)
    {
      _logger = logger;
      _searchService = searchService;
    }

    /// <summary>
    /// The index page
    /// </summary>
    public IActionResult Index(string searchQuery, int? page = null, SearchMode mode = SearchMode.Search)
    {
      if (page != null && page < 1)
        page = 1;
      int pageZeroIndexed = (page ?? 1) - 1;

      if (searchQuery != null && mode == SearchMode.Search)
      {
        ViewBag.ResultMode = "Search";
        ViewBag.Results = _searchService.Search(searchQuery, pageZeroIndexed);
        ViewBag.SearchQuery = searchQuery;
        return View("SearchResults");
      }
      else if (mode == SearchMode.RecentUpdates)
      {
        ViewBag.ResultMode = "Recent Updates";
        ViewBag.Results = _searchService.GetRecentUpdates(pageZeroIndexed);
        return View("SearchResults");
      }
      else if (mode == SearchMode.RandomPages)
      {
        ViewBag.ResultMode = "Random Pages";
        ViewBag.Results = _searchService.GetRandomPages(pageZeroIndexed);
        return View("SearchResults");
      }
      else
      {
        return View("Index");
      }
    }
  }
}
