namespace Seismic.Alerts.Contracts
{
    public sealed class EarthquakeAlert(Guid messageId,
    DateTimeOffset occurredAt,
    DateTimeOffset publishedAt,
    long earthquakeId,
    string region,
    double magnitude,
    double latitude,
    double longitude,
    string severity,
    bool forceFail = false)
    {
        public Guid MessageId { get; } = messageId;
        public DateTimeOffset OccurredAt { get; } = occurredAt;
        public DateTimeOffset PublishedAt { get; } = publishedAt; // когда сообщение было опубликовано в брокер
        public long EarthquakeId { get; } = earthquakeId;
        public string Region { get; } = region;
        public double Magnitude { get; } = magnitude;
        public double Latitude { get; } = latitude;
        public double Longitude { get; } = longitude;
        public string Severity { get; } = severity; // light, moderate, strong

        public bool ForceFail { get; } = forceFail;

    }
}
