
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

// عدّلي هذه ال Usings حسب أسماء النيم سبيس الفعلية في مشروعك:
using Infrastructure;              // SilentSignalsDbContext
using Domain.Entities;             // MorseMapping
using Api.Controllers;             // ConvertController + Dto إن وُجد

public class ConvertControllerTests
{
    private SilentSignalsDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<SilentSignalsDbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
            .Options;

        var db = new SilentSignalsDbContext(options);

        // Seeding بسيط
        db.MorseMappings.AddRange(
            new MorseMapping{ Char = "S", Pattern = "..." },
            new MorseMapping{ Char = "O", Pattern = "---" }
        );
        db.SaveChanges();

        return db;
    }

    [Fact]
    public async Task TextToMorse_SOS_Returns_Expected()
    {
        // Arrange
        using var db = CreateDb();

        // ملاحظة: مرري الديبندنسيز اللازمة للكنترولر حسب Constructor عندك
        var controller = new ConvertController(db);

        // لو عندك DTO اسمه مختلف، عدّلي السطرين التالية
        var request = new ConvertController.TextConvertRequest("SOS");

        // Act
        var actionResult = await controller.TextToMorse(request);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        dynamic payload = ok.Value!;
        Assert.Contains("... --- ...", (string)payload.Joined);
        Assert.Equal(0, (int)payload.UnknownCount);
    }
}
