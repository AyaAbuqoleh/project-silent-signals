
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/convert")]
public class ConvertController : ControllerBase
{
    private readonly SilentSignalsDbContext _db;

    public ConvertController(SilentSignalsDbContext db)
    {
        _db = db;
    }

    public record TextConvertRequest(string Text);

    public class MorseConvertResponse
    {
        public string InputText { get; set; } = string.Empty;
        public List<string> Tokens { get; set; } = new();
        public List<string> Patterns { get; set; } = new();
        public string Joined { get; set; } = string.Empty;
        public int UnknownCount { get; set; }
        public DateTime ConvertedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public class SignItem
    {
        public string TokenType { get; set; } = "Letter";
        public string Token { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? DescriptionAR { get; set; }
        public string? DescriptionEN { get; set; }
        public bool Unknown { get; set; }
    }

    public class SignConvertResponse
    {
        public string InputText { get; set; } = string.Empty;
        public List<string> Words { get; set; } = new();
        public List<SignItem> Sequence { get; set; } = new();
        public int UnknownCount { get; set; }
        public DateTime ConvertedAtUtc { get; set; } = DateTime.UtcNow;
    }

    // ===== POST /api/convert/text-to-morse =====
    [HttpPost("text-to-morse")]
    public async Task<ActionResult<MorseConvertResponse>> TextToMorse([FromBody] TextConvertRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Text))
            return BadRequest("Text is required.");

        var map = await _db.MorseMappings.ToDictionaryAsync(m => m.Char, m => m.Pattern);
        var input = req.Text.Trim();

        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var tokens = new List<string>();
        var patterns = new List<string>();
        int unknown = 0;

        foreach (var w in words)
        {
            var chars = w.ToUpperInvariant().ToCharArray();
            foreach (var ch in chars)
            {
                var key = ch.ToString();
                if (map.TryGetValue(key, out var p))
                {
                    tokens.Add(key);
                    patterns.Add(p);
                }
                else
                {
                    tokens.Add(key);
                    patterns.Add("?");
                    unknown++;
                }
            }
            patterns.Add("/"); // separator
        }

        if (patterns.Count > 0 && patterns[^1] == "/")
            patterns.RemoveAt(patterns.Count - 1);

        var joined = string.Join(" ", patterns);

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid? uid = null;
        if (Guid.TryParse(userIdStr, out var g)) uid = g;

        _db.ConversionHistories.Add(new ConversionHistory
        {
            UserId = uid,
            SourceText = input,
            TargetSystem = "Morse",
            ResultPayload = joined,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var res = new MorseConvertResponse
        {
            InputText = input,
            Tokens = tokens,
            Patterns = patterns.Where(p => p != "/").ToList(),
            Joined = joined,
            UnknownCount = unknown
        };

        return Ok(res);
    }

    // ===== POST /api/convert/text-to-sign =====
    [HttpPost("text-to-sign")]
    public async Task<ActionResult<SignConvertResponse>> TextToSign([FromBody] TextConvertRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Text))
            return BadRequest("Text is required.");

        var signs = await _db.SignGestures
            .Where(s => s.TokenType == "Letter")
            .ToDictionaryAsync(s => s.Token, s => s);

        var input = req.Text.Trim();
        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var sequence = new List<SignItem>();
        int unknown = 0;

        foreach (var w in words)
        {
            foreach (var ch in w.ToUpperInvariant())
            {
                var key = ch.ToString();
                if (signs.TryGetValue(key, out var g))
                {
                    sequence.Add(new SignItem
                    {
                        TokenType = "Letter",
                        Token = key,
                        ImageUrl = g.ImageUrl,
                        DescriptionAR = g.DescriptionAR,
                        DescriptionEN = g.DescriptionEN,
                        Unknown = false
                    });
                }
                else
                {
                    sequence.Add(new SignItem
                    {
                        TokenType = "Letter",
                        Token = key,
                        ImageUrl = null,
                        DescriptionAR = "غير مدعوم",
                        DescriptionEN = "Unsupported",
                        Unknown = true
                    });
                    unknown++;
                }
            }
            sequence.Add(new SignItem
            {
                TokenType = "Separator",
                Token = "|",
                ImageUrl = null,
                DescriptionAR = "فاصل كلمة",
                DescriptionEN = "Word separator",
                Unknown = false
            });
        }

        if (sequence.Count > 0 && sequence[^1].Token == "|")
            sequence.RemoveAt(sequence.Count - 1);

        var payload = System.Text.Json.JsonSerializer.Serialize(sequence.Select(s => new { s.TokenType, s.Token }));

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid? uid = null;
        if (Guid.TryParse(userIdStr, out var parsedGuid)) uid = parsedGuid;
        _db.ConversionHistories.Add(new ConversionHistory
        {
            UserId = uid,
            SourceText = input,
            TargetSystem = "Sign",
            ResultPayload = payload,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var res = new SignConvertResponse
        {
            InputText = input,
            Words = words.ToList(),
            Sequence = sequence,
            UnknownCount = unknown,
            ConvertedAtUtc = DateTime.UtcNow
        };

        return Ok(res);
    }
}
