using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seismic.Alerts.Consumer.DataAccess;

namespace Seismic.Alerts.Consumer.Controllers;

[ApiController]
[Route("notifications")]
public sealed class NotificationsController(NotificationsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await db.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(200)
            .ToListAsync(cancellationToken));
}