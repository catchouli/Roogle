using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Roogle.Shared.Queue
{
  /// <summary>
  /// The queue class
  /// </summary>
  public class Queue : IQueue
  {
    /// <summary>
    /// The queue connection
    /// </summary>
    private readonly IQueueConnection _connection;

    /// <summary>
    /// The queue's name
    /// </summary>
    private readonly string _queueName;

    /// <summary>
    /// The consumer, created on demand if needed
    /// </summary>
    private EventingBasicConsumer _consumer;

    /// <summary>
    /// Construct a queue with the given parameters
    /// </summary>
    /// <param name="queueName">The queue's name</param>
    public Queue(IQueueConnection connection, string queueName)
    {
      _connection = connection;
      _queueName = queueName;
    }

    /// <inheritdoc/>
    public void SendMessage<T>(T message)
    {
      var json = GetEncodedValue(message);
      var body = Encoding.UTF8.GetBytes(json);

      _connection.Channel.BasicPublish(exchange: "",
        routingKey: _queueName,
        basicProperties: null,
        body: body);
    }

    /// <inheritdoc/>
    public EventingBasicConsumer GetConsumer()
    {
      if (_consumer == null)
      {
        _consumer = new EventingBasicConsumer(_connection.Channel);
        _connection.Channel.BasicConsume(queue: _queueName, autoAck: true, consumer: _consumer);
      }

      return _consumer;
    }

    /// <summary>
    /// Encode a value to a serialized string for adding to the queue
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    /// <param name="value">The value</param>
    /// <returns>The value as a serialized string</returns>
    private string GetEncodedValue<T>(T value)
    {
      if (typeof(T) == typeof(string))
        return value as string;
      else
        return JsonConvert.SerializeObject(value);
    }
  }
}
