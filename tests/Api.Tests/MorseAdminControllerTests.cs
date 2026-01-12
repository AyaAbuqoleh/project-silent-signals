
using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Controllers.Admin;
using Domain.Entities;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class MorseAdminControllerTests
{
    private SilentSignalsDbContext CreateDb()
    {
        var opt = new DbContextOptionsBuilder<SilentSignalsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new SilentSignalsDbContext(opt);
        return db;
    }

    [Fact]
    public async Task Create_Then_List_Morse()
    {
        using var db = CreateDb();
        var controller = new MorseAdminController(db);

        // إنشاء إدخال جديد
        var dto = new MorseAdminController.UpsertMorseDto("A", ".-");
        var create = await controller.Create(dto);
        var created = Assert.IsType<CreatedAtActionResult>(create.Result);
        var entity = Assert.IsType<MorseMapping>(created.Value);
        Assert.Equal("A", entity.Char);
        Assert.Equal(".-", entity.Pattern);

        // قراءة القائمة
        var list = await controller.GetAll();
        var ok = Assert.IsType<OkObjectResult>(list.Result);
        var items = Assert.IsAssignableFrom<System.Collections.Generic.IEnumerable<MorseMapping>>(ok.Value);
        Assert.True(items.Any(x => x.Char == "A" && x.Pattern == ".-"));
    }

    [Fact]
    public async Task Update_Detects_Duplicate_Char()
    {
        using var db = CreateDb();
        db.MorseMappings.AddRange(
            new MorseMapping { Char = "A", Pattern = ".-" },
            new MorseMapping { Char = "B", Pattern = "-..." }
        );
        db.SaveChanges();

        var controller = new MorseAdminController(db);
        var first = db.MorseMappings.First(x => x.Char == "A");

        // محاولة تحديث A إلى حرف موجود B
        var dto = new MorseAdminController.UpsertMorseDto("B", ".-");
        var res = await controller.Update(first.Id, dto);
        var conflict = Assert.IsType<ConflictObjectResult>(res);
        Assert.Contains("already exists", conflict.Value!.ToString());
    }
}
