namespace Roogle.RoogleSpider.Services
{
  /// <summary>
  /// Interface for discovered url consumer service
  /// </summary>
  public interface IDiscoveredUrlConsumerService
  {
    /// <summary>
    /// Start consuming discovered urls
    /// </summary>
    void StartWorkers();
  }
}
