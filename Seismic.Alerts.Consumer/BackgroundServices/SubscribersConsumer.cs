using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Seismic.Alerts.Consumer.DataAccess;
using Seismic.Alerts.Consumer.DataAccess.Entities;
using Seismic.Alerts.Consumer.Options;
using Seismic.Alerts.Contracts;

namespace Seismic.Alerts.Consumer.BackgroundServices;

public sealed class SubscribersConsumer(
    IOptions<ConnectionFactory> connectionFactory,
    IOptions<RabbitMqTopologyOptions> topology,
    IServiceScopeFactory scopeFactory,
    ILogger<SubscribersConsumer> logger)
    : RabbitMqConsumerBase(connectionFactory, topology, logger)
{
    protected override QueueBinding Queue => Topology.SubscribersQueue;

    protected override async Task HandleAsync(EarthquakeAlert alert, string routingKey, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();

        var matched = await db.Subscribers // поиск подписчиков региона
            .Where(s => s.Region == alert.Region && s.MinMagnitude <= alert.Magnitude)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        foreach (var subscriber in matched)
        {
            db.Notifications.Add(new Notification
            {
                SubscriberId = subscriber.Id,
                EarthquakeId = alert.EarthquakeId,
                Region = alert.Region,
                Magnitude = alert.Magnitude,
                Severity = alert.Severity,
                CreatedAt = now
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "[subscribers] Alert {MessageId}: notified {Count} subscriber(s) in region {Region}",
            alert.MessageId, matched.Count, alert.Region);
    }
}