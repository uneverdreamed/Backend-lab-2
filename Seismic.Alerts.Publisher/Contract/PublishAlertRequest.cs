using System.ComponentModel.DataAnnotations;

namespace Seismic.Alerts.Publisher.Contract
{
    public sealed class PublishAlertRequest
    {
        [Range(1, long.MaxValue, ErrorMessage = "EarthquakeId должен быть положительным")]
        public long EarthquakeId { get; init; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Region обязателен")]
        [StringLength(50, MinimumLength = 2)]
        public string Region { get; init; } = string.Empty;

        [Range(0.0, 10.0, ErrorMessage = "Магнитуда должна быть в диапазоне [0, 10]")]
        public double Magnitude { get; init; }

        [Range(-90.0, 90.0)]
        public double Latitude { get; init; }

        [Range(-180.0, 180.0)]
        public double Longitude { get; init; }

        [Required]
        public DateTimeOffset OccurredAt { get; init; }
        public bool ForceFail { get; init; } = false; // отладочный флаг для DQL
    }
}
