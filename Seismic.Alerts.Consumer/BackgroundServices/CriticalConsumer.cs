using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Seismic.Alerts.Consumer.DataAccess;
using Seismic.Alerts.Consumer.DataAccess.Entities;
using Seismic.Alerts.Consumer.Options;
using Seismic.Alerts.Contracts;

namespace Seismic.Alerts.Consumer.BackgroundServices;

public sealed class CriticalConsumer(
    IOptions<ConnectionFactory> connectionFactory,
    IOptions<RabbitMqTopologyOptions> topology,
    IServiceScopeFactory scopeFactory,
    ILogger<CriticalConsumer> logger)
    : RabbitMqConsumerBase(connectionFactory, topology, logger)
{
    protected override QueueBinding Queue => Topology.CriticalQueue;

    protected override async Task HandleAsync(EarthquakeAlert alert, string routingKey, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();

        db.CriticalAlerts.Add(new CriticalAlert
        {
            MessageId = alert.MessageId,
            EarthquakeId = alert.EarthquakeId,
            Region = alert.Region,
            Magnitude = alert.Magnitude,
            Latitude = alert.Latitude,
            Longitude = alert.Longitude,
            ProcessedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);

        Logger.LogWarning(
            "[critical] CRITICAL earthquake {EarthquakeId} in {Region}, M={Magnitude}",
            alert.EarthquakeId, alert.Region, alert.Magnitude);
    }
}