using System;

namespace Roogle.Shared.Services
{
  public class CanonicalUrlService : ICanonicalUrlService
  {
    /// <summary>
    /// Canonicalize the url (remove anchors and standardize the formatting)
    /// </summary>
    /// <param name="url">The url</param>
    /// <returns>The fixed url</returns>
    public string CanonicalizeUrl(string url)
    {
      url = RemoveAnchor(url);
      url = new Uri(url).AbsoluteUri;
      return url;
    }

    /// <summary>
    /// Remove the anchor from a url
    /// </summary>
    /// <param name="url">The url</param>
    /// <returns>The now anchorless url</returns>
    private static string RemoveAnchor(string url)
    {
      int index = url.IndexOf('#');
      string newUrl = index >= 0
        ? url.Substring(0, index)
        : url;
      return newUrl;
    }
  }
}
