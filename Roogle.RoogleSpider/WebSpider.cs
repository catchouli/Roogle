using Flurl;
using Flurl.Http;
using HtmlAgilityPack;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Roogle.RoogleSpider
{
  /// <summary>
  /// The web spidre class where stuff actually happens
  /// </summary>
  public class WebSpider
  {
    /// <summary>
    /// Allowed content types
    /// </summary>
    private HashSet<string> AllowedContentTypes = new HashSet<string> { "text/html" };

    /// <summary>
    /// The amount of time to sleep when throttling, in milliseconds
    /// </summary>
    private int TimeToSleepWhenThrottlingMs = 100;

    /// <summary>
    /// The condition that determines whether or not we should crawl a discovered url
    /// </summary>
    private readonly IUrlCrawlerCondition _urlCondition;

    /// <summary>
    /// The maximum requests per second
    /// </summary>
    private readonly int _maxRequestsPerSecond;

    /// <summary>
    /// The open set of urls to crawl
    /// </summary>
    private ConcurrentQueue<string> _openSet = new ConcurrentQueue<string>();

    /// <summary>
    /// The crawled urls
    /// </summary>
    private HashSet<string> _crawledUrls = new HashSet<string>();

    /// <summary>
    /// The stopwatch for throttling
    /// </summary>
    private Stopwatch _stopwatch = new Stopwatch();

    /// <summary>
    /// The number of urls crawled
    /// </summary>
    private int _requests = 0;

    /// <summary>
    /// Get the number of requests per second since requests started
    /// </summary>
    public double RequestsPerSecond {
      get {
        return _requests / _stopwatch.Elapsed.TotalSeconds;
      }
    }

    /// <summary>
    /// Create a new web spider
    /// </summary>
    /// <param name="maxRequestsPerSecond">The maximum requests per second</param>
    public WebSpider(IUrlCrawlerCondition urlCondition, int maxRequestsPerSecond)
    {
      _urlCondition = urlCondition;
      _maxRequestsPerSecond = maxRequestsPerSecond;
      Log.Information("{className} started", nameof(WebSpider));
    }

    /// <summary>
    /// Start crawling from the given url
    /// </summary>
    public async Task StartCrawling()
    {
      // Start the stopwatch, for throttling
      _stopwatch.Restart();
      _requests = 0;

      // Continue until we run out of urls to crawl
      while (_openSet.TryDequeue(out string url))
      {
        // Throttle if necessary
        ThrottleIfNecessary();

        // Crawl the url
        await CrawlUrl(url);
        ++_requests;
      }

      Log.Information("{className} finished, out of urls to crawl", nameof(WebSpider));
    }

    /// <summary>
    /// Crawl a url and add linked pages to the open set
    /// </summary>
    /// <param name="url">The url to crawl</param>
    private async Task CrawlUrl(string url)
    {
      Log.Information("Crawling {url}", url);
      _crawledUrls.Add(url);

      // Make request for the html
      try
      {
        var headerResponse = await new Url(url)
          .SendAsync(HttpMethod.Get, default, default,
            HttpCompletionOption.ResponseHeadersRead);

        if (!headerResponse.Headers.TryGetFirst("Content-Type", out string contentType) ||
          !AllowedContentTypes.Contains(contentType))
        {
          Log.Information("Received non-html content type {contentType}", contentType);
          return;
        }

        // Read html
        var html = await headerResponse.GetStringAsync();

        // Parse page html
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Get page title
        var titleNode = doc.DocumentNode.SelectNodes("title")?.FirstOrDefault();
        if (titleNode != null)
          Log.Information("Page title: {pageTitle}", titleNode.InnerText);

        // Get all links
        var linkNodes = doc.DocumentNode.SelectNodes("//a[@href]") ?? Enumerable.Empty<HtmlNode>();
        foreach (var linkNode in linkNodes)
        {
          string linkHref = linkNode.Attributes["href"].Value;
          string absoluteUrl = GetAbsoluteUrl(url, linkHref);
          AddUrlToOpenSet(absoluteUrl);
        }
      }
      catch (FlurlHttpException e)
      {
        Log.Error("Encountered flurl exception {msg} when requesting {url}", e.Message, url);
      }
    }

    /// <summary>
    /// Get the aboslute url from a link href
    /// </summary>
    /// <param name="parentUrl">The url the page was loaded from</param>
    /// <param name="linkHref">The href of the link</param>
    /// <returns>The absolute url</returns>
    private string GetAbsoluteUrl(string parentUrl, string linkHref)
    {
      var uri = new Uri(linkHref, UriKind.RelativeOrAbsolute);
      if (!uri.IsAbsoluteUri)
        uri = new Uri(new Uri(parentUrl), uri);
      return uri.ToString();
    }

    /// <summary>
    /// Add a url to the open set
    /// </summary>
    /// <param name="url">The url to add</param>
    public void AddUrlToOpenSet(string url)
    {
      Log.Information("Adding {url} to the open set", url);
      if (_urlCondition.ShouldCrawl(url) && !_openSet.Contains(url) && !_crawledUrls.Contains(url))
      {
        _openSet.Enqueue(url);
      }
    }

    /// <summary>
    /// Throttle if necessary by calling Thread.Sleep
    /// </summary>
    private void ThrottleIfNecessary()
    {
      if (RequestsPerSecond > _maxRequestsPerSecond)
        Log.Information("Requests per second: {requestsPerSecond}, throttling", RequestsPerSecond);

      while (RequestsPerSecond > _maxRequestsPerSecond)
        Thread.Sleep(TimeToSleepWhenThrottlingMs);
    }
  }
}
