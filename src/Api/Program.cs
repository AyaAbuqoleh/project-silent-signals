
using System.Text;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Identity;                 // ✅ لاستدعاء الـSeeder
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ================== Diagnostics (تشخيص مبكر) ==================
var envName   = builder.Environment.EnvironmentName;
var connSqlite = builder.Configuration.GetConnectionString("DefaultSqlite");
var connDefault = builder.Configuration.GetConnectionString("Default");
var resolvedConn =
       !string.IsNullOrWhiteSpace(connSqlite) ? connSqlite
     : !string.IsNullOrWhiteSpace(connDefault) ? connDefault
     : "Data Source=SilentSignals.db";

Console.WriteLine($"[CFG] ASPNETCORE_ENVIRONMENT = {envName}");
Console.WriteLine($"[CFG] ConnectionStrings:DefaultSqlite = {connSqlite ?? "<null>"}");
Console.WriteLine($"[CFG] ConnectionStrings:Default       = {connDefault ?? "<null>"}");
Console.WriteLine($"[CFG] Resolved SQLite Connection      = {resolvedConn}");

// ================== Services ==================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== DbContext (SQLite) =====
// يعتمد على resolvedConn لمنع أي قيمة قديمة بصيغة Server=...
builder.Services.AddDbContext<SilentSignalsDbContext>(options =>
    options.UseSqlite(resolvedConn));

// ===== Identity Core (Guid) + Roles =====
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddRoles<IdentityRole<Guid>>()
.AddEntityFrameworkStores<SilentSignalsDbContext>()
.AddDefaultTokenProviders();

// ===== JWT Auth =====
var jwtKey      = builder.Configuration["Jwt:Key"]
                  ?? throw new InvalidOperationException("JWT Key missing in configuration.");
var jwtIssuer   = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var keyBytes    = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // للتجريب المحلي
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer      = jwtIssuer,
        ValidAudience    = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ClockSkew        = TimeSpan.FromMinutes(2)
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// ===== CORS للـVite =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVite", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ================== Pipeline ==================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// مبدئيًا نُبقي التحويل إلى HTTPS. إن واجهتِ مشكلة شهادة أثناء التجربة، شغّلي عبر:
// dotnet run --project src/Api/Api.csproj --no-launch-profile --urls "http://localhost:5038"
app.UseHttpsRedirection();

app.UseCors("AllowVite");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ================== Seed Roles/Admin ==================
using (var scope = app.Services.CreateScope())
{
    try
    {
        await AppDbInitializer.SeedAsync(scope.ServiceProvider);
        Console.WriteLine("[Seed] Roles/Admin seeding completed.");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"[Seed] Failed: {ex.Message}");
        // لو أردتِ استمرار الإقلاع رغم فشل الـSeeder، لا تعيدي الرمي.
        // throw; // إن أحببتِ إيقاف التطبيق عند فشل الـSeeder
    }
}

await app.RunAsync();
