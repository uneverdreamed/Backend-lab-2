using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seismic.Alerts.Consumer.DataAccess;

namespace Seismic.Alerts.Consumer.Controllers;

[ApiController]
[Route("critical-alerts")]
public sealed class CriticalAlertsController(NotificationsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await db.CriticalAlerts
            .OrderByDescending(c => c.ProcessedAt)
            .ToListAsync(cancellationToken));
}