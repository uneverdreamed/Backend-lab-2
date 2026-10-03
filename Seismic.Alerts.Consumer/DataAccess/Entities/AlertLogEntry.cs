namespace Seismic.Alerts.Consumer.DataAccess.Entities;

public sealed class AlertLogEntry
{
    public int Id { get; set; }
    public Guid MessageId { get; set; }
    public long EarthquakeId { get; set; }
    public string Region { get; set; } = string.Empty;
    public double Magnitude { get; set; }
    public string Severity { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
}
