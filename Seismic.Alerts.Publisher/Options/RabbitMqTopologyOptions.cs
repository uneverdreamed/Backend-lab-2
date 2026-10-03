namespace Seismic.Alerts.Publisher.Options
{
    public sealed class RabbitMqTopologyOptions
    {
        public string ExchangeName { get; init; } = "seismic.alerts";
    }
}
