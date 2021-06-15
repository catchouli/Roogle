namespace Roogle.RoogleSpider.Services
{
  /// <summary>
  /// Interface for page indexer service
  /// </summary>
  public interface IPageIndexerService
  {
    /// <summary>
    /// Start indexing pages
    /// </summary>
    void StartWorkers();
  }
}
