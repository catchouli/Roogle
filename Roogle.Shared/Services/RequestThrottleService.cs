using Microsoft.Extensions.Configuration;
using Serilog;
using System.Diagnostics;
using System.Threading;

namespace Roogle.Shared.Services
{
  /// <summary>
  /// The service for throttling requests
  /// </summary>
  public class RequestThrottleService : IRequestThrottleService
  {
    /// <summary>
    /// The stopwatch for throttling
    /// </summary>
    private readonly Stopwatch _stopwatch = new Stopwatch();

    /// <summary>
    /// The amount of time to sleep when throttling, in milliseconds
    /// </summary>
    private const int TimeToSleepWhenThrottlingMs = 100;

    /// <summary>
    /// The maximum requests per second
    /// </summary>
    private readonly int _maxRequestsPerSecond;

    /// <summary>
    /// Get the number of requests per second since requests started
    /// </summary>
    public double RequestsPerSecond {
      get {
        return _requests / _stopwatch.Elapsed.TotalSeconds;
      }
    }
    /// <summary>
    /// The number of requests made
    /// </summary>
    private int _requests = 0;

    /// <summary>
    /// Start the request throttle service
    /// </summary>
    /// <param name="configuration">The configuration</param>
    public RequestThrottleService(IConfiguration configuration)
    {
      _maxRequestsPerSecond = configuration.GetValue<int>("MaxRequestsPerSecond");
      _stopwatch.Start();
    }

    public void IncRequests()
    {
      Interlocked.Increment(ref _requests);
      ThrottleIfNecessary();
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
