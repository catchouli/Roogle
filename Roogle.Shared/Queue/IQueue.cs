using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Roogle.Shared.Queue
{
  /// <summary>
  /// The queue interface
  /// </summary>
  public interface IQueue
  {
    /// <summary>
    /// Send a message to the queue
    /// </summary>
    /// <typeparam name="T">The type of message to send</typeparam>
    /// <param name="message">The message to send</param>
    void SendMessage<T>(T message);

    /// <summary>
    /// Get the consumer for this queue for subscribing to events
    /// </summary>
    /// <returns>The queue's consumer</returns>
    EventingBasicConsumer GetConsumer();
  }
}
