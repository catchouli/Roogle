namespace Roogle.RoogleSpider.Services
{
  /// <summary>
  /// Interface for scraped page consumer service
  /// </summary>
  public interface IScrapedPageConsumerService
  {
    /// <summary>
    /// Start consuming scraped pages
    /// </summary>
    void StartWorkers();
  }
}
