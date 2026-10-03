using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seismic.Alerts.Consumer.DataAccess;

namespace Seismic.Alerts.Consumer.Controllers;

[ApiController]
[Route("alert-log")]
public sealed class AlertLogController(NotificationsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await db.AlertLog
            .OrderByDescending(a => a.ReceivedAt)
            .Take(200)
            .ToListAsync(cancellationToken));
}