namespace Seismic.Alerts.Consumer.DataAccess.Entities

{
    public sealed class Notification
    {
        public int Id { get; set; }
        public int SubscriberId { get; set; }
        public long EarthquakeId { get; set; }
        public string Region { get; set; } = string.Empty;
        public double Magnitude { get; set; }
        public string Severity { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}