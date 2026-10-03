using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seismic.Alerts.Consumer.DataAccess;

namespace Seismic.Alerts.Consumer.Controllers;

[ApiController]
[Route("subscribers")]
public sealed class SubscribersController(NotificationsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await db.Subscribers
            .OrderBy(s => s.Id)
            .ToListAsync(cancellationToken));
}