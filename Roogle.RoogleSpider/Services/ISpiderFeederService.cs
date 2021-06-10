namespace Roogle.RoogleSpider.Services
{
  /// <summary>
  /// Interface for spider feeder service
  /// </summary>
  public interface ISpiderFeederService
  {
    /// <summary>
    /// Start feeding the spider
    /// </summary>
    void StartWorkers();
  }
}
