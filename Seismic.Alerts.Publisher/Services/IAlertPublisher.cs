using Seismic.Alerts.Contracts;

namespace Seismic.Alerts.Publisher.Services;
public interface IAlertPublisher
{
    Task PublishAsync(EarthquakeAlert alert, CancellationToken cancellationToken);
}