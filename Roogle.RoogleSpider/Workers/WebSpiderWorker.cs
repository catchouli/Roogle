﻿using Flurl;
using Flurl.Http;
using HtmlAgilityPack;
using Roogle.RoogleSpider.Models;
using Roogle.RoogleSpider.Queues;
using Roogle.RoogleSpider.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Roogle.RoogleSpider.Workers
{
  /// <summary>
  /// The web spider worker
  /// </summary>
  public class WebSpiderWorker : IWorker
  {
    /// <summary>
    /// Allowed content types
    /// </summary>
    public static readonly HashSet<string> AllowedContentTypes = new HashSet<string> { "text/html" };

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// The queue for receiving urls to scrape
    /// </summary>
    private readonly PagesToScrapeQueue _pagesToScrapeQueue;

    /// <summary>
    /// The queue for sending crawled pages back
    /// </summary>
    private readonly PagesScrapedQueue _pagesScrapedQueue;

    /// <summary>
    /// The queue for sending discovered urls back
    /// </summary>
    private readonly LinksDiscoveredQueue _urlsDiscoveredQueue;

    /// <summary>
    /// The throttle service
    /// </summary>
    private readonly IRequestThrottleService _throttleService;

    /// <summary>
    /// The regex for trimming excess whitespace
    /// </summary>
    private readonly Regex _whitespaceRegex;

    /// <summary>
    /// Create the web spider worker
    /// </summary>
    /// <param name="throttleService">The throttle service</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <param name="pagesToScrapeQueue">The incoming queue for pages to scrape</param>
    /// <param name="pagesScrapedQueue">The outgoing queue for pages that have been scraped</param>
    /// <param name="urlsDiscoveredQueue">The outgoing queue for discovered urls</param>
    public WebSpiderWorker(IRequestThrottleService throttleService, CancellationToken cancellationToken,
      PagesToScrapeQueue pagesToScrapeQueue, PagesScrapedQueue pagesScrapedQueue, LinksDiscoveredQueue urlsDiscoveredQueue)
    {
      _throttleService = throttleService;
      _cancellationToken = cancellationToken;
      _pagesToScrapeQueue = pagesToScrapeQueue;
      _pagesScrapedQueue = pagesScrapedQueue;
      _urlsDiscoveredQueue = urlsDiscoveredQueue;
      _whitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);
    }

    /// <summary>
    /// The thread entrypoint
    /// </summary>
    public void ThreadProc()
    {
      while (!_cancellationToken.IsCancellationRequested)
      {
        // Continue until we run out of urls to crawl
        while (_pagesToScrapeQueue.Queue.TryDequeue(out var page))
        {
          // Crawl the url
          CrawlUrl(page.Item1, page.Item2).Wait();
        }

        Thread.Sleep(100);
      }

      Log.Logger.Information("Web spider thread {threadId} edited", Thread.CurrentThread.ManagedThreadId);
    }

    /// <summary>
    /// Crawl a url and add linked pages to the open set
    /// </summary>
    /// <param name="url">The url to crawl</param>
    private async Task CrawlUrl(Guid pageGuid, string pageUrl)
    {
      Log.Information("Crawling {url}", pageUrl);

      // Make request for the html
      try
      {
        _throttleService.IncRequests();
        var headerResponse = await new Url(pageUrl)
          .SendAsync(HttpMethod.Get, default, default,
            HttpCompletionOption.ResponseHeadersRead);

        if (!headerResponse.Headers.TryGetFirst("Content-Type", out string contentType) ||
          !AllowedContentTypes.Contains(contentType))
        {
          Log.Information("Received non-html content type {contentType}", contentType);

          // Might as well return it to the queue anyway so we can store the content type
          var uri = new Uri(pageUrl);
          string filename = Path.GetFileName(uri.LocalPath);

          _pagesScrapedQueue.Queue.Enqueue(new ScrapedPage
          {
            Guid = pageGuid,
            Url = pageUrl,
            ContentType = contentType,
            Title = filename,
            Contents = ""
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
        var pageContents = doc.DocumentNode?.SelectSingleNode("//body")?.InnerText?.Trim() ?? "";
        pageContents = _whitespaceRegex.Replace(pageContents, " ");

        // Send page data back to the pages scraped queue
        _pagesScrapedQueue.Queue.Enqueue(new ScrapedPage
        {
          Guid = pageGuid,
          Url = pageUrl,
          ContentType = contentType,
          Title = pageTitle,
          Contents = pageContents
        });

        // Get all links and send them back to the urls discovered queue
        var linkNodes = doc.DocumentNode.SelectNodes("//a[@href]") ?? Enumerable.Empty<HtmlNode>();
        foreach (var linkNode in linkNodes)
        {
          string linkHref = linkNode.Attributes["href"].Value;
          string absoluteUrl = GetAbsoluteUrl(pageUrl, linkHref);
          _urlsDiscoveredQueue.Queue.Enqueue((pageUrl, absoluteUrl));
        }
      }
      catch (FlurlHttpException e)
      {
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
