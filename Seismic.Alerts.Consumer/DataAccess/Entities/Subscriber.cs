namespace Seismic.Alerts.Consumer.DataAccess.Entities
{
    public sealed class Subscriber
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty; // регион по котормоу хочет получать уведомления
        public double MinMagnitude { get; set; } // минимальная интересующая магнитуда

    }
}
