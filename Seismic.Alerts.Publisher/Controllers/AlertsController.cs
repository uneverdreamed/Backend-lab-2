using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Seismic.Alerts.Contracts;
using Seismic.Alerts.Publisher.Common;
using Seismic.Alerts.Publisher.Contract;
using Seismic.Alerts.Publisher.Options;
using Seismic.Alerts.Publisher.Services;

namespace Seismic.Alerts.Publisher.Controllers;

[ApiController]
[Route("alerts")]
public sealed class AlertsController(
    IAlertPublisher publisher,
    IOptions<AlertClassificationOptions> classification,
    ILogger<AlertsController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Publish(
        [FromBody] PublishAlertRequest request,
        CancellationToken cancellationToken)
    {
        var thresholds = classification.Value;
        var severity = request.Magnitude >= thresholds.StrongThreshold ? "strong"
                     : request.Magnitude >= thresholds.ModerateThreshold ? "moderate"
                     : "light";

        var alert = new EarthquakeAlert(
            messageId: Guid.NewGuid(),
            occurredAt: request.OccurredAt,
            publishedAt: DateTimeOffset.UtcNow,
            earthquakeId: request.EarthquakeId, region: request.Region.Trim().ToLowerInvariant(),
            magnitude: request.Magnitude,
            latitude: request.Latitude,
            longitude: request.Longitude,
            severity: severity,
            forceFail: request.ForceFail);

        await publisher.PublishAsync(alert, cancellationToken);

        logger.LogInformation(
            "Accepted alert {MessageId} for earthquake {EarthquakeId}",
            alert.MessageId, alert.EarthquakeId);

        return Accepted(new
        {
            messageId = alert.MessageId,
            severity = alert.Severity,
            publishedAt = alert.PublishedAt,
            createdBy = MeInfo.Line
        });
    }
}
