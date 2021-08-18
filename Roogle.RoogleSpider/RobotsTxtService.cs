using Flurl;
using Microsoft.Extensions.Configuration;
using Roogle.Shared.Services;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Linq;
using TurnerSoftware.RobotsExclusionTools;

namespace Roogle.RoogleSpider
{
  /// <summary>
  /// The robots.txt service that lets us query robots.txt files and
  /// caches them for some amount of time
  /// </summary>
  public class RobotsTxtService : IRobotsTxtService
  {
    /// <summary>
    /// A robots.txt cache entry
    /// </summary>
    private class RobotsTxtCacheEntry
    {
      /// <summary>
      /// The scheme (e.g. "http" or "https")
      /// </summary>
      public string Scheme { get; set; }

      /// <summary>
      /// The hostname
      /// </summary>
      public string Host { get; set; }

      /// <summary>
      /// The robots.txt file
      /// </summary>
      public RobotsFile RobotsFile { get; set; }

      /// <summary>
      /// The time the robots.txt was last accessed
      /// </summary>
      public DateTime TimeAccessed { get; set; }
    }

    /// <summary>
    /// A queue of robots.txt cache entries
    /// </summary>
    private readonly ConcurrentQueue<RobotsTxtCacheEntry> _robotsTxtCache = new ConcurrentQueue<RobotsTxtCacheEntry>();

    /// <summary>
    /// The time after which cache entries should expire
    /// </summary>
    private readonly TimeSpan _expiryTimespan;

    /// <summary>
    /// The maximum number of robots.txt entries
    /// </summary>
    private readonly int _maxRobotsTxtEntries;

    /// <summary>
    /// The request throttle service
    /// </summary>
    private readonly IRequestThrottleService _throttleService;

    /// <summary>
    /// Construct the RobotsTxtService with the given configuration
    /// </summary>
    /// <param name="configuration">The configuration object</param>
    /// <param name="throttleService">The throttle service</param>
    public RobotsTxtService(IConfiguration configuration, IRequestThrottleService throttleService)
    {
      _expiryTimespan = TimeSpan.FromMinutes(configuration.GetValue<int>("RobotsTxtExpiryTimeMinutes"));
      _maxRobotsTxtEntries = configuration.GetValue<int>("MaxRobotsTxtEntries");
      _throttleService = throttleService;
    }

    /// <summary>
    /// Check if we're allowed to access a given url
    /// </summary>
    /// <param name="url">The url</param>
    /// <returns>Whether we're allowed to access it</returns>
    public bool IsAllowedAccess(string url)
    {
      var uri = new Uri(url);
      return GetRobotsFile(uri).IsAllowedAccess(uri, "TalkhausBot");
    }

    /// <summary>
    /// Get the robots file for a given url
    /// </summary>
    /// <param name="uri">The url</param>
    /// <returns>The robots file for this domain</returns>
    private RobotsFile GetRobotsFile(Uri uri)
    {
      // Work out the expiry cutoff time
      var expiryCutoff = DateTime.Now - _expiryTimespan;

      // Expire any old cache entries
      while (_robotsTxtCache.TryPeek(out var cacheEntry) && cacheEntry.TimeAccessed < expiryCutoff)
        _robotsTxtCache.TryDequeue(out _);

      // Check if we already have an entry
      var entry = _robotsTxtCache.FirstOrDefault(entry => entry.Scheme == uri.Scheme && entry.Host == uri.Host);

      if (entry != null)
        return entry.RobotsFile;

      // Query a new robots file and add it to the queue
      var robotsFileParser = new RobotsFileParser();
      _throttleService.IncRequests();
      var robotsFile = robotsFileParser.FromUriAsync(GetRobotsFileUri(uri)).Result;

      _robotsTxtCache.Enqueue(new RobotsTxtCacheEntry
      {
        Scheme = uri.Scheme,
        Host = uri.Host,
        RobotsFile = robotsFile,
        TimeAccessed = DateTime.Now
      });

      // Remove excess entries
      while (_robotsTxtCache.Count > _maxRobotsTxtEntries)
      {
        if (_robotsTxtCache.TryDequeue(out var dequeued))
          Log.Information("Revoked robots.txt cache for {scheme}://{host}", dequeued.Scheme, dequeued.Host);
      }

      return robotsFile;
    }

    /// <summary>
    /// Get the uri for a robots.txt file from a given uri
    /// </summary>
    /// <param name="uri">The uri</param>
    /// <returns>The robots.txt file url</returns>
    private Uri GetRobotsFileUri(Uri uri)
    {
      var urlWithoutScheme = Url.Combine(new[] { uri.Host, "/robots.txt" });
      return new Uri($"{uri.Scheme}://{urlWithoutScheme}");
    }
  }
}
