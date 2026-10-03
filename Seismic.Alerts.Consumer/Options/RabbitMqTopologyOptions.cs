namespace Seismic.Alerts.Consumer.Options;

public sealed class RabbitMqTopologyOptions
{
    public string ExchangeName { get; init; } = "seismic.alerts";

    public QueueBinding AuditQueue { get; init; } = new()
    {
        Name = "notifications.audit",
        RoutingKey = "quake.#" // все
    };

    public QueueBinding SubscribersQueue { get; init; } = new()
    {
        Name = "notifications.subscribers",
        RoutingKey = "quake.*.*" // любые событяи
    };

    public QueueBinding CriticalQueue { get; init; } = new()
    {
        Name = "notifications.critical",
        RoutingKey = "quake.*.strong" // только strong
    };

    public ushort PrefetchCount { get; init; } = 1;

    public string DeadLetterExchange { get; init; } = "seismic.alerts.dlx";
    public string DeadLetterQueue { get; init; } = "notifications.dlq"; 

}

public sealed class QueueBinding
{
    public string Name { get; init; } = string.Empty;
    public string RoutingKey { get; init; } = string.Empty;
}