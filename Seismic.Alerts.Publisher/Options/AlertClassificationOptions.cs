namespace Seismic.Alerts.Publisher.Options
{
    public sealed class AlertClassificationOptions
    {
        public double StrongThreshold { get; init; } = 6.0;
        public double ModerateThreshold { get; init; } = 4.5;
    }
}
