
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IConfiguration _config;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IConfiguration config)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _config = config;
    }

    // ===== DTOs =====
    public record RegisterRequest(string Email, string Password, string? DisplayName, string? PreferredLanguage);
    public record LoginRequest(string Email, string Password);
    public record AuthResponse(string Token, DateTime ExpiresAtUtc, string UserId, string? DisplayName, string? PreferredLanguage);
    public record UpdateMeRequest(string? DisplayName, string? PreferredLanguage); // ✅ هنا مكانه

    // ===== Register =====
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req)
    {
        var email = (req.Email ?? "").Trim();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Email and Password are required.");

        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null) return Conflict("Email already registered.");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            DisplayName = string.IsNullOrWhiteSpace(req.DisplayName) ? email : req.DisplayName,
            PreferredLanguage = string.IsNullOrWhiteSpace(req.PreferredLanguage) ? "ar" : req.PreferredLanguage!,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, req.Password);
        if (!createResult.Succeeded)
            return BadRequest(string.Join("; ", createResult.Errors.Select(e => e.Description)));

        // Ensure 'User' role exists and assign
        if (!await _roleManager.RoleExistsAsync("User"))
            await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = "User", NormalizedName = "USER" });
        await _userManager.AddToRoleAsync(user, "User");

        var (token, exp) = await GenerateJwtAsync(user);
        return Ok(new AuthResponse(token, exp, user.Id.ToString(), user.DisplayName, user.PreferredLanguage));
    }

    // ===== Login =====
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        var user = await _userManager.FindByEmailAsync(req.Email);
        if (user is null) return Unauthorized("Invalid credentials.");

        var passOk = await _userManager.CheckPasswordAsync(user, req.Password);
        if (!passOk) return Unauthorized("Invalid credentials.");

        var (token, exp) = await GenerateJwtAsync(user);
        return Ok(new AuthResponse(token, exp, user.Id.ToString(), user.DisplayName, user.PreferredLanguage));
    }

    // ===== Get Me =====
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<object>> Me()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdStr)) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userIdStr);
        if (user is null) return Unauthorized();

        return Ok(new
        {
            userId = user.Id,
            email = user.Email,
            displayName = user.DisplayName,
            preferredLanguage = user.PreferredLanguage,
            createdAt = user.CreatedAt
        });
    }

    // ===== Update Me =====
    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult> UpdateMe([FromBody] UpdateMeRequest req)
    {
        // 1) الحصول على الـUserId من التوكن
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdStr))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userIdStr);
        if (user is null)
            return Unauthorized();

        // 2) تحديث DisplayName إن وجد
        if (!string.IsNullOrWhiteSpace(req.DisplayName))
            user.DisplayName = req.DisplayName!.Trim();

        // 3) تحديث PreferredLanguage (نقيّدها إلى "ar" أو "en")
        if (!string.IsNullOrWhiteSpace(req.PreferredLanguage))
        {
            var lang = req.PreferredLanguage!.Trim().ToLowerInvariant();
            user.PreferredLanguage = lang == "en" ? "en" : "ar";
        }

        // 4) حفظ التغييرات
        var res = await _userManager.UpdateAsync(user);
        if (!res.Succeeded)
            return BadRequest(string.Join("; ", res.Errors.Select(e => e.Description)));

        return NoContent();
    }

    // ===== JWT Generator =====
    private async Task<(string token, DateTime expires)> GenerateJwtAsync(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresMinutes = int.TryParse(_config["Jwt:ExpiresMinutes"], out var m) ? m : 120;
        var expUtc = DateTime.UtcNow.AddMinutes(expiresMinutes);

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.DisplayName ?? user.Email ?? "")
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expUtc,
            signingCredentials: creds
        );
        var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenStr, expUtc);
    }
}
