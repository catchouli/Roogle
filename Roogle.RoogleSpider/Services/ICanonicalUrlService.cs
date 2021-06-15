namespace Roogle.RoogleSpider.Services
{
  /// <summary>
  /// Interface for a service that converts urls to a canonical form for indexing
  /// </summary>
  public interface ICanonicalUrlService
  {
    /// <summary>
    /// Canonicalize the url (remove anchors and standardize the formatting)
    /// </summary>
    /// <param name="url">The url</param>
    /// <returns>The fixed url</returns>
    string CanonicalizeUrl(string url);
  }
}
