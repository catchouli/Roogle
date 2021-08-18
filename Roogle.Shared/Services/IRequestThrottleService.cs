namespace Roogle.Shared.Services
{
  /// <summary>
  /// The service for throttling requests
  /// </summary>
  public interface IRequestThrottleService
  {
    /// <summary>
    /// Increment the request counter, throttling if necessary
    /// </summary>
    void IncRequests();
  }
}
