namespace Roogle.RoogleSpider.Workers
{
  /// <summary>
  /// The worker interface
  /// </summary>
  public interface IWorker
  {
    /// <summary>
    /// The entry point for the worker
    /// </summary>
    void ThreadProc();
  }
}
