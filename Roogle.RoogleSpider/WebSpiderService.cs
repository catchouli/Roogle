using Flurl;
using Flurl.Http;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Roogle.Shared.Models;
using Roogle.Shared.Queue;
using Roogle.Shared.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Roogle.RoogleSpider
{
  /// <summary>
  /// The web spider service
  /// </summary>
  public class WebSpiderService : IWebSpiderService
  {
    /// <summary>
    /// Allowed content types
    /// </summary>
    /// TODO: we could change this via config
    public static readonly HashSet<string> AllowedContentTypes = new HashSet<string> { "text/html" };

    /// <summary>
    /// The queue for receiving urls to scrape
    /// </summary>
    private readonly IQueue _pagesToScrapeQueue;

    /// <summary>
    /// The queue for sending crawled pages back
    /// </summary>
    private readonly IQueue _pagesScrapedQueue;

    /// <summary>
    /// The queue for sending discovered urls back
    /// </summary>
    private readonly IQueue _urlsDiscoveredQueue;

    /// <summary>
    /// The throttle service
    /// </summary>
    private readonly IRequestThrottleService _throttleService;

    /// <summary>
    /// The robots.txt service
    /// </summary>
    private readonly IRobotsTxtService _robotsTxtService;

    /// <summary>
    /// The regex for trimming excess whitespace
    /// </summary>
    private readonly Regex _whitespaceRegex;

    /// <summary>
    /// Create a new web spider
    /// </summary>
    /// <param name="throttleService">The throttle service</param>
    /// <param name="robotsTxtService">The robots.txt service</param>
    /// <param name="queueConnection">The queue connection</param>
    public WebSpiderService(IRequestThrottleService throttleService, IRobotsTxtService robotsTxtService,
      IQueueConnection queueConnection)
    {
      _throttleService = throttleService;
      _robotsTxtService = robotsTxtService;

      _pagesToScrapeQueue = queueConnection.CreateQueue("PagesToScrape");
      _pagesScrapedQueue = queueConnection.CreateQueue("PagesScraped");
      _urlsDiscoveredQueue = queueConnection.CreateQueue("UrlsDiscovered");

      _whitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);
    }

    /// <inheritdoc/>
    public void Start()
    {
      Log.Logger.Information("Starting {className} ({threadId})", nameof(WebSpiderService), Thread.CurrentThread.ManagedThreadId);

      _pagesToScrapeQueue.GetConsumer().Received += async (model, args) =>
      {
        var message = Encoding.UTF8.GetString(args.Body.ToArray());

        try
        {
          await CrawlUrl(message);
        }
        catch (Exception e)
        {
          Log.Error("Error when processing job: {messsage} ({exceptionMessage})", message, e.Message);
        }
      };
    }

    /// <summary>
    /// Crawl a url and add linked pages to the open set
    /// </summary>
    /// <param name="url">The url to crawl</param>
    private async Task CrawlUrl(string pageUrl)
    {
      Log.Information("Crawling {url}", pageUrl);

      // Make request for the html
      try
      {
        // Make robots.txt request
        if (!_robotsTxtService.IsAllowedAccess(pageUrl))
        {
          Log.Information("Robots.txt disallows us to access {pageUrl}", pageUrl);

          _pagesScrapedQueue.SendMessage(new ScrapedPage
          {
            Url = pageUrl,
            ContentType = "robots.txt denied",
            Title = "",
            Contents = "",
            StatusCode = 0
          });

          return;
        }

        // Make page request
        _throttleService.IncRequests();
        var headerResponse = await new Url(pageUrl)
          .SendAsync(HttpMethod.Get, default, default,
            HttpCompletionOption.ResponseHeadersRead);

        if (!headerResponse.Headers.TryGetFirst("Content-Type", out string contentType) ||
          !AllowedContentTypes.Any(type => contentType.StartsWith(type)))
        {
          Log.Information("Received non-html content type {contentType}", contentType);

          // Might as well return it to the queue anyway so we can store the content type
          var uri = new Uri(pageUrl);
          string filename = Path.GetFileName(uri.LocalPath);

          _pagesScrapedQueue.SendMessage(new ScrapedPage
          {
            Url = pageUrl,
            ContentType = contentType,
            Title = filename,
            Contents = "",
            StatusCode = headerResponse.StatusCode
          });

          return;
        }

        // Read html
        var html = await headerResponse.GetStringAsync();

        // Parse page html
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Get page title
        var pageTitle = doc.DocumentNode?.SelectSingleNode("//title")?.InnerText ?? pageUrl;
        Log.Information("Page title: {pageTitle}", pageTitle);

        // Get page contents and trim extra whitespace using regex
        var pageContents = doc.DocumentNode?.SelectSingleNode("//body")?.InnerText?.Trim() ?? doc.DocumentNode?.InnerText ?? "";
        pageContents = _whitespaceRegex.Replace(pageContents, " ");

        // Send page data back to the pages scraped queue
        _pagesScrapedQueue.SendMessage(new ScrapedPage
        {
          Url = pageUrl,
          ContentType = contentType,
          Title = pageTitle,
          Contents = pageContents,
          StatusCode = headerResponse.StatusCode
        });

        // Get all links and send them back to the urls discovered queue
        var linkNodes = doc.DocumentNode.SelectNodes("//a[@href]") ?? Enumerable.Empty<HtmlNode>();
        foreach (var linkNode in linkNodes)
        {
          string linkHref = linkNode.Attributes["href"].Value;
          string absoluteUrl = GetAbsoluteUrl(pageUrl, linkHref);
          _urlsDiscoveredQueue.SendMessage((pageUrl, absoluteUrl));
        }
      }
      catch (FlurlHttpTimeoutException e)
      {
        Log.Error("Request timeed out: {url}: {msg}", pageUrl, e.Message);
      }
      catch (FlurlHttpException e)
      {
        _pagesScrapedQueue.SendMessage(new ScrapedPage
        {
          Url = pageUrl,
          ContentType = "",
          Title = "",
          Contents = "",
          StatusCode = e.StatusCode ?? 0
        });

        if (e.StatusCode != null)
          Log.Error("Recording status code {statusCode} for url {url}", e.StatusCode, pageUrl);
        else
          Log.Error("Encountered flurl exception {msg} when requesting {url}", e.Message, pageUrl);
      }
    }

    /// <summary>
    /// Get the aboslute url from a link href
    /// </summary>
    /// <param name="parentUrl">The url the page was loaded from</param>
    /// <param name="linkHref">The href of the link</param>
    /// <returns>The absolute url</returns>
    private static string GetAbsoluteUrl(string parentUrl, string linkHref)
    {
      var uri = new Uri(linkHref, UriKind.RelativeOrAbsolute);
      if (!uri.IsAbsoluteUri)
        uri = new Uri(new Uri(parentUrl), uri);
      return uri.ToString();
    }

  }
}
