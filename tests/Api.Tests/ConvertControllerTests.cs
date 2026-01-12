
using System;
using System.Threading.Tasks;
using Api.Controllers; // تأكدي من namespace ConvertController
using Domain.Entities;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Api.Tests
{
    public class ConvertControllerTests
    {
        private SilentSignalsDbContext CreateInMemoryDb()
        {
            var opt = new DbContextOptionsBuilder<SilentSignalsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new SilentSignalsDbContext(opt);

            // Seeding بسيط لمورس
            db.MorseMappings.AddRange(new[]
            {
                new MorseMapping{ Id=1, Char="H", Pattern="...."},
                new MorseMapping{ Id=2, Char="E", Pattern="."},
                new MorseMapping{ Id=3, Char="L", Pattern=".-.."},
                new MorseMapping{ Id=4, Char="O", Pattern="---"},
            });
            db.SaveChanges();
            return db;
        }

        [Fact]
        public async Task TextToMorse_Maps_HELLO()
        {
            using var db = CreateInMemoryDb();
            var controller = new ConvertController(db);

            var req = new ConvertController.TextConvertRequest("HELLO");
            var result = await controller.TextToMorse(req);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            dynamic payload = ok.Value!;
            Assert.Contains(".... . .-.. .-.. ---", (string)payload.Joined);
            Assert.Equal(0, (int)payload.UnknownCount);
        }
    }
}
