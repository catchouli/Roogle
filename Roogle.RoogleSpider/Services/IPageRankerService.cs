namespace Roogle.RoogleSpider.Services
{
  /// <summary>
  /// Interface for page ranker service
  /// </summary>
  public interface IPageRankerService
  {
    /// <summary>
    /// Start ranking pages
    /// </summary>
    void StartWorkers();
  }
}
