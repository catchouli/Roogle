using RabbitMQ.Client;

namespace Roogle.Shared.Queue
{
  /// <summary>
  /// The queue connection interface to wrap up some rabbitmq connection stuff
  /// </summary>
  public interface IQueueConnection
  {
    /// <summary>
    /// The queue connection
    /// </summary>
    IConnection Connection { get; }

    /// <summary>
    /// The queue channel
    /// </summary>
    IModel Channel { get; }

    /// <summary>
    /// Get or create a queue
    /// </summary>
    /// <param name="queueName">The queue's name</param>
    /// <returns>The queue interface for the given queue</returns>
    IQueue CreateQueue(string queueName);
  }
}
