using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace Roogle.Shared.Queue
{
  /// <summary>
  /// The queue connection class to wrap up some rabbitmq connection stuff
  /// </summary>
  public class QueueConnection : IQueueConnection, IDisposable
  {
    /// <inheritdoc/>
    public IConnection Connection { get; private set; }

    /// <inheritdoc/>
    public IModel Channel { get; private set; }

    /// <summary>
    /// Connect to rabbitmq
    /// </summary>
    public QueueConnection()
    {
      var factory = new ConnectionFactory() { HostName = "rabbit-mq" };
      Connection = factory.CreateConnection();
      Channel = Connection.CreateModel();
    }

    /// <inheritdoc/>
    public IQueue CreateQueue(string queueName)
    {
      Channel.QueueDeclare(queue: queueName,
                           durable: false,
                           exclusive: false,
                           autoDelete: false,
                           arguments: null);

      return new Queue(this, queueName);
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          // TODO: dispose managed state (managed objects).
          Channel.Dispose();
          Connection.Dispose();
        }

        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
        // TODO: set large fields to null.

        disposedValue = true;
      }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~QueueConnection()
    // {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
      // TODO: uncomment the following line if the finalizer is overridden above.
      // GC.SuppressFinalize(this);
    }
    #endregion
  }
}
