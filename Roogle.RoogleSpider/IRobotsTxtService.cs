namespace Roogle.RoogleSpider
{
  /// <summary>
  /// The interface for the robots.txt service that lets us query robots.txt files and
  /// caches them for some amount of time
  /// </summary>
  public interface IRobotsTxtService
  {
    /// <summary>
    /// Check if we're allowed to access a given url
    /// </summary>
    /// <param name="url">The url</param>
    /// <returns>Whether we're allowed to access it</returns>
    bool IsAllowedAccess(string url);
  }
}
