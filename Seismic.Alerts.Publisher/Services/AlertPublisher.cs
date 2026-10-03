using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Seismic.Alerts.Contracts;
using Seismic.Alerts.Publisher.Options;

namespace Seismic.Alerts.Publisher.Services;

public sealed class AlertPublisher : IAlertPublisher, IAsyncDisposable
{
    private readonly RabbitMqTopologyOptions _topology;
    private readonly ConnectionFactory _connectionFactory;
    private readonly ILogger<AlertPublisher> _logger;

    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public AlertPublisher(
        IOptions<ConnectionFactory> connectionFactory,
        IOptions<RabbitMqTopologyOptions> topology,
        ILogger<AlertPublisher> logger)
    {
        _connectionFactory = connectionFactory.Value;
        _topology = topology.Value;
        _logger = logger;
    }

    public async Task PublishAsync(EarthquakeAlert alert, CancellationToken cancellationToken)
    {
        if (_channel is null)
        {
            await EnsureInitializedAsync(cancellationToken);
        }

        var routingKey = $"quake.{alert.Region}.{alert.Severity}";
        var body = JsonSerializer.SerializeToUtf8Bytes(alert);
        var properties = new BasicProperties
        {
            Persistent = true,
            MessageId = alert.MessageId.ToString(),
            ContentType = "application/json",
            Timestamp = new AmqpTimestamp(alert.PublishedAt.ToUnixTimeSeconds()),
        };

        await _channel!.BasicPublishAsync(
            exchange: _topology.ExchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Published alert {MessageId} to {Exchange} with routing key {RoutingKey} (M={Magnitude}, region={Region})",
            alert.MessageId, _topology.ExchangeName, routingKey, alert.Magnitude, alert.Region);
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_channel is not null) return;

            _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.ExchangeDeclareAsync(
                exchange: _topology.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Connected to RabbitMQ at {Host}:{Port}, exchange {Exchange} ready",
                _connectionFactory.HostName, _connectionFactory.Port, _topology.ExchangeName);
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
        _initLock.Dispose();
    }
}