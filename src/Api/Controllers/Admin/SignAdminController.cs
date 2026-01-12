
using Domain.Entities;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers.Admin;

[ApiController]
[Route("api/admin/signs")]
[Authorize(Roles = "Admin")]
public class SignAdminController : ControllerBase
{
    private readonly SilentSignalsDbContext _db;
    public SignAdminController(SilentSignalsDbContext db) => _db = db;

    public record UpsertSignDto(string TokenType, string Token, string? ImageUrl, string? DescriptionAR, string? DescriptionEN);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SignGesture>>> GetAll()
        => Ok(await _db.SignGestures.OrderBy(s => s.TokenType).ThenBy(s => s.Token).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SignGesture>> GetById(int id)
        => (await _db.SignGestures.FindAsync(id)) is { } e ? Ok(e) : NotFound();

    [HttpPost]
    public async Task<ActionResult<SignGesture>> Create([FromBody] UpsertSignDto dto)
    {
        var type = (dto.TokenType ?? "Letter").Trim();
        var token = (dto.Token ?? "").Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(token)) return BadRequest("Token required.");

        if (await _db.SignGestures.AnyAsync(s => s.TokenType == type && s.Token == token))
            return Conflict("Token exists.");

        var e = new SignGesture
        {
            TokenType = type,
            Token = token,
            ImageUrl = dto.ImageUrl,
            DescriptionAR = dto.DescriptionAR,
            DescriptionEN = dto.DescriptionEN
        };
        _db.SignGestures.Add(e);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = e.Id }, e);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpsertSignDto dto)
    {
        var e = await _db.SignGestures.FindAsync(id);
        if (e is null) return NotFound();

        var type = (dto.TokenType ?? "Letter").Trim();
        var token = (dto.Token ?? "").Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(token)) return BadRequest("Token required.");

        var exists = await _db.SignGestures.AnyAsync(s => s.TokenType == type && s.Token == token && s.Id != id);
        if (exists) return Conflict("Token exists.");

        e.TokenType = type;
        e.Token = token;
        e.ImageUrl = dto.ImageUrl;
        e.DescriptionAR = dto.DescriptionAR;
        e.DescriptionEN = dto.DescriptionEN;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var e = await _db.SignGestures.FindAsync(id);
        if (e is null) return NotFound();
        _db.SignGestures.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
