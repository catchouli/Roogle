using Roogle.RoogleSpider.Workers;
using Serilog;
using System;
using System.Threading;

namespace Roogle.RoogleSpider
{
  /// <summary>
  /// The base for services that have workers, to use this just
  /// derive from this class and then initialise Workers in your constructor
  /// </summary>
  public class WorkerServiceBase : IDisposable
  {
    /// <summary>
    /// The cancellation token source for our threads
    /// </summary>
    protected CancellationTokenSource CancellationTokenSource { get; private set; }

    /// <summary>
    /// The worker
    /// </summary>
    protected IWorker Worker { get; set; }

    /// <summary>
    /// The worker thread
    /// </summary>
    private Thread _workerThread;

    /// <summary>
    /// Set up the workers
    /// </summary>
    public WorkerServiceBase()
    {
      // Create cancellation token source for canceling worker threads
      CancellationTokenSource = new CancellationTokenSource();

      Log.Information("{className} started", GetType().Name);
    }

    /// <summary>
    /// Start feeding the spider
    /// </summary>
    public void StartWorkers()
    {
      if (_workerThread == null)
      {
        // Start spider feeder thread
        _workerThread = new Thread(Worker.ThreadProc);
        _workerThread.Start();
      }
    }

    /// <summary>
    /// Implements IDisposable
    /// </summary>
    public void Dispose()
    {
      if (_workerThread == null)
      {
        Log.Information("{className} terminating worker threads", GetType().Name);

        // Cancel token and wait for thread to exit
        CancellationTokenSource.Cancel();
        _workerThread.Join();

        CancellationTokenSource = null;
        _workerThread = null;
      }
    }
  }
}
