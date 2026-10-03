using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Seismic.Alerts.Consumer.DataAccess;
using Seismic.Alerts.Consumer.DataAccess.Entities;
using Seismic.Alerts.Consumer.Options;
using Seismic.Alerts.Contracts;

namespace Seismic.Alerts.Consumer.BackgroundServices;

public sealed class AuditConsumer(
    IOptions<ConnectionFactory> connectionFactory,
    IOptions<RabbitMqTopologyOptions> topology,
    IServiceScopeFactory scopeFactory,
    ILogger<AuditConsumer> logger)
    : RabbitMqConsumerBase(connectionFactory, topology, logger)
{
    protected override QueueBinding Queue => Topology.AuditQueue;

    protected override async Task HandleAsync(EarthquakeAlert alert, string routingKey, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();

        db.AlertLog.Add(new AlertLogEntry
        {
            MessageId = alert.MessageId,
            EarthquakeId = alert.EarthquakeId,
            Region = alert.Region,
            Magnitude = alert.Magnitude,
            Severity = alert.Severity,
            OccurredAt = alert.OccurredAt,
            ReceivedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "[audit] Logged alert {MessageId} (routing key {RoutingKey})",
            alert.MessageId, routingKey);
    }
}
