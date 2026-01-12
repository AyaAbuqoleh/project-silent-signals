
using Domain.Entities;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers.Admin;

[ApiController]
[Route("api/admin/lessons")]
[Authorize(Roles = "Admin")]
public class LessonsAdminController : ControllerBase
{
    private readonly SilentSignalsDbContext _db;
    public LessonsAdminController(SilentSignalsDbContext db) => _db = db;

    public record UpsertLessonDto(string Topic, string TitleAR, string TitleEN, string BodyAR, string BodyEN, string? MediaUrl);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Lesson>>> GetAll()
        => Ok(await _db.Lessons.OrderBy(l => l.Topic).ThenBy(l => l.Id).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Lesson>> GetById(int id)
        => (await _db.Lessons.FindAsync(id)) is { } e ? Ok(e) : NotFound();

    [HttpPost]
    public async Task<ActionResult<Lesson>> Create([FromBody] UpsertLessonDto dto)
    {
        var e = new Lesson
        {
            Topic = (dto.Topic ?? "Morse").Trim(),
            TitleAR = dto.TitleAR, TitleEN = dto.TitleEN,
            BodyAR = dto.BodyAR,   BodyEN = dto.BodyEN,
            MediaUrl = dto.MediaUrl
        };
        _db.Lessons.Add(e);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = e.Id }, e);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpsertLessonDto dto)
    {
        var e = await _db.Lessons.FindAsync(id);
        if (e is null) return NotFound();

        e.Topic   = (dto.Topic ?? e.Topic).Trim();
        e.TitleAR = dto.TitleAR;  e.TitleEN = dto.TitleEN;
        e.BodyAR  = dto.BodyAR;   e.BodyEN  = dto.BodyEN;
        e.MediaUrl = dto.MediaUrl;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var e = await _db.Lessons.FindAsync(id);
        if (e is null) return NotFound();
        _db.Lessons.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
