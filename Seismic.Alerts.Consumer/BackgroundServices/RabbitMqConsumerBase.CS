using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Seismic.Alerts.Consumer.Options;
using Seismic.Alerts.Contracts;

namespace Seismic.Alerts.Consumer.BackgroundServices;

public abstract class RabbitMqConsumerBase : BackgroundService
{
    private readonly ConnectionFactory _connectionFactory;
    protected readonly RabbitMqTopologyOptions Topology;
    protected readonly ILogger Logger;

    protected RabbitMqConsumerBase(
        IOptions<ConnectionFactory> connectionFactory,
        IOptions<RabbitMqTopologyOptions> topology,
        ILogger logger)
    {
        _connectionFactory = connectionFactory.Value;
        Topology = topology.Value;
        Logger = logger;
    }

    protected abstract QueueBinding Queue { get; } // КОНКРЕТНАЯ ОЧЕРЕДЬ ДЛЯ CONSUMER
    protected abstract Task HandleAsync(EarthquakeAlert alert, string routingKey, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            exchange: Topology.DeadLetterExchange,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: Topology.DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: Topology.DeadLetterQueue,
            exchange: Topology.DeadLetterExchange,
            routingKey: string.Empty,
            cancellationToken: stoppingToken);
        
        await channel.ExchangeDeclareAsync(
            exchange: Topology.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        var queueArguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = Topology.DeadLetterExchange
        };

        await channel.QueueDeclareAsync(
            queue: Queue.Name,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArguments,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: Queue.Name,
            exchange: Topology.ExchangeName,
            routingKey: Queue.RoutingKey,
            cancellationToken: stoppingToken);

        await channel.BasicQosAsync(0, Topology.PrefetchCount, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, delivery) =>
        {
        EarthquakeAlert? alert = null;
        try
        {
            alert = JsonSerializer.Deserialize<EarthquakeAlert>(delivery.Body.Span)
                ?? throw new InvalidOperationException("Deserialized alert is null");

                if (alert.ForceFail)
                {
                    throw new InvalidOperationException(
                        $"ForceFail=true, intentionally failing message {alert.MessageId}");
                }

            await HandleAsync(alert, delivery.RoutingKey, stoppingToken);
            await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "[{Queue}] Failed to process message {MessageId}, NACK without requeue",
                Queue.Name, alert?.MessageId);
                await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: false);
            }
        };

        await channel.BasicConsumeAsync(
            queue: Queue.Name,
            autoAck: false,
            consumerTag: $"{Queue.Name}-consumer",
            consumer: consumer,
            cancellationToken: stoppingToken);

        Logger.LogInformation(
            "Consumer subscribed to queue {Queue} (routing key {RoutingKey}, prefetch {Prefetch})",
            Queue.Name, Queue.RoutingKey, Topology.PrefetchCount);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}