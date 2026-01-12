
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/history")]
public class HistoryController : ControllerBase
{
    private readonly SilentSignalsDbContext _db;
    public HistoryController(SilentSignalsDbContext db) => _db = db;

    [Authorize]
    [HttpGet("my")]
    public async Task<ActionResult<object>> MyHistory()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdStr)) return Unauthorized();

        var guid = Guid.Parse(userIdStr);

        var last50 = await _db.ConversionHistories
            .Where(h => h.UserId == guid)
            .OrderByDescending(h => h.CreatedAt)
            .Take(50)
            .Select(h => new {
                h.Id, h.SourceText, h.TargetSystem, h.ResultPayload, h.CreatedAt
            })
            .ToListAsync();

        return Ok(last50);
    }
}
