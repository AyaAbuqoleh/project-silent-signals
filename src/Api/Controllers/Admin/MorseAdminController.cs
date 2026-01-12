
using Domain.Entities;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers.Admin;

[ApiController]
[Route("api/admin/morse")]
[Authorize(Roles = "Admin")]
public class MorseAdminController : ControllerBase
{
    private readonly SilentSignalsDbContext _db;
    public MorseAdminController(SilentSignalsDbContext db) => _db = db;

    public record UpsertMorseDto(string Char, string Pattern);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MorseMapping>>> GetAll()
        => Ok(await _db.MorseMappings.OrderBy(m => m.Char).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MorseMapping>> GetById(int id)
        => (await _db.MorseMappings.FindAsync(id)) is { } m ? Ok(m) : NotFound();

    [HttpPost]
    public async Task<ActionResult<MorseMapping>> Create([FromBody] UpsertMorseDto dto)
    {
        var ch = (dto.Char ?? "").Trim().ToUpperInvariant();
        var pattern = (dto.Pattern ?? "").Trim();
        if (ch.Length != 1 || string.IsNullOrWhiteSpace(pattern)) return BadRequest("Invalid input.");

        if (await _db.MorseMappings.AnyAsync(x => x.Char == ch))
            return Conflict($"Char '{ch}' already exists.");

        var entity = new MorseMapping { Char = ch, Pattern = pattern };
        _db.MorseMappings.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpsertMorseDto dto)
    {
        var entity = await _db.MorseMappings.FindAsync(id);
        if (entity is null) return NotFound();

        var ch = (dto.Char ?? "").Trim().ToUpperInvariant();
        var pattern = (dto.Pattern ?? "").Trim();
        if (ch.Length != 1 || string.IsNullOrWhiteSpace(pattern)) return BadRequest("Invalid input.");

        var exists = await _db.MorseMappings.AnyAsync(x => x.Char == ch && x.Id != id);
        if (exists) return Conflict($"Char '{ch}' already exists.");

        entity.Char = ch;
        entity.Pattern = pattern;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var entity = await _db.MorseMappings.FindAsync(id);
        if (entity is null) return NotFound();
        _db.MorseMappings.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
