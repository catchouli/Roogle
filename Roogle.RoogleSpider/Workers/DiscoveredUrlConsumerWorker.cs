﻿using Roogle.RoogleSpider.Db;
using Roogle.RoogleSpider.Models;
using Roogle.RoogleSpider.Queues;
using Roogle.RoogleSpider.Utils;
using Serilog;
using System;
using System.Linq;
using System.Threading;

namespace Roogle.RoogleSpider.Workers
{
  /// <summary>
  /// The page consumer, which recieves discovered url from the queue
  /// and adds them to the database if they don't already exist
  /// </summary>
  public class DiscoveredUrlConsumerWorker : IWorker
  {
    /// <summary>
    /// The thread cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// The db context
    /// </summary>
    private readonly RoogleSpiderDbContext _dbContext;

    /// <summary>
    /// The queue through which we receive urls that have been discovered
    /// </summary>
    private readonly LinksDiscoveredQueue _linksDiscoveredQueue;

    /// <summary>
    /// The crawler condition that stops us just endlessly crawling the web
    /// </summary>
    private readonly IUrlCrawlerCondition _crawlerCondition;

    /// <summary>
    /// Constructor
    /// </summary>
    public DiscoveredUrlConsumerWorker(CancellationToken cancellationToken, RoogleSpiderDbContext dbContext,
      LinksDiscoveredQueue linksDiscoveredQueue, IUrlCrawlerCondition crawlerCondition)
    {
      _cancellationToken = cancellationToken;
      _dbContext = dbContext;
      _linksDiscoveredQueue = linksDiscoveredQueue;
      _crawlerCondition = crawlerCondition;
    }

    /// <summary>
    /// The thread entry point
    /// </summary>
    public void ThreadProc()
    {
      while (!_cancellationToken.IsCancellationRequested)
      {
        while (_linksDiscoveredQueue.Queue.TryDequeue(out var discoveredLink))
        {
          if (_crawlerCondition.ShouldCrawl(discoveredLink.To))
            ProcessDiscoveredLink(discoveredLink);
        }

        Thread.Sleep(100);
      }
    }

    /// <summary>
    /// Process a discovered link
    /// </summary>
    /// <param name="discoveredLink">The discovered link</param>
    public void ProcessDiscoveredLink((string From, string To) discoveredLink)
    {
      Log.Information("Processing discovered url {discoveredLink}", discoveredLink);

      // Check if the linked page is already in the db and add it if not
      if (!_dbContext.Pages.Any(page => page.Url == discoveredLink.To))
      {
        Log.Information("Adding discovered url {discoveredUrl} to the db", discoveredLink.To);

        // Add it
        _dbContext.Pages.Add(new Page
        {
          Id = Guid.NewGuid(),
          Url = discoveredLink.To,
          Contents = "",
          Title = "",
          PageHash = 0,
          PageRank = 0,
          ExpiryTime = DateTime.Now,
          UpdatedTime = DateTime.Now,
          ContentsChanged = false,
          PageRankDirty = false
        });
        _dbContext.SaveChanges();
      }

      // Get linked page ids
      var fromPageId = _dbContext.Pages.Where(page => page.Url == discoveredLink.From).Select(page => page.Id);
      if (!fromPageId.Any())
      {
        Log.Error("Linked from page was not found in db: {fromUrl}", discoveredLink.From);
        return;
      }

      var toPageId = _dbContext.Pages.Where(page => page.Url == discoveredLink.To).Select(page => page.Id);
      if (!toPageId.Any())
      {
        Log.Error("Linked to page was not found in db: {toUrl}", discoveredLink.To);
        return;
      }

      // Find out if there's a link already stored and if not store it
      var link = _dbContext.Links.SingleOrDefault(link => link.FromPage == fromPageId.Single() && link.ToPage == toPageId.Single());
      if (link != null)
      {
        link.LastSeenTime = DateTime.Now;
        _dbContext.Links.Update(link);
      }
      else
      {
        _dbContext.Links.Add(new Link
        {
          Id = Guid.NewGuid(),
          FromPage = fromPageId.Single(),
          ToPage = toPageId.Single(),
          LastSeenTime = DateTime.Now
        });
      }

      _dbContext.SaveChanges();
    }
  }
}
