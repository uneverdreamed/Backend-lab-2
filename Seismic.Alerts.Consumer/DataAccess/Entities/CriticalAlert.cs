namespace Seismic.Alerts.Consumer.DataAccess.Entities;

public sealed class CriticalAlert
{
    public int Id { get; set; }
    public Guid MessageId { get; set; }
    public long EarthquakeId { get; set; }
    public string Region { get; set; } = string.Empty;
    public double Magnitude { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTimeOffset ProcessedAt { get; set; }
}