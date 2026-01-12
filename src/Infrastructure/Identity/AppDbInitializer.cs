
using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity
{
    public static class AppDbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;

            var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var cfg     = sp.GetRequiredService<IConfiguration>();

            // 1) تأكد من وجود الأدوار
            foreach (var role in new[] { "User", "Admin" })
            {
                if (!await roleMgr.RoleExistsAsync(role))
                    await roleMgr.CreateAsync(new IdentityRole<Guid>(role));
            }

            // 2) قراءة بيانات الأدمن من AdminSeed
            var email = cfg["AdminSeed:Email"];
            var pwd   = cfg["AdminSeed:Password"];
            var name  = cfg["AdminSeed:DisplayName"] ?? "Admin";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pwd))
                return; // لا تنشئ شيء إن لم تكن القيم موجودة

            // 3) أنشئ مستخدم أدمن إن لم يكن موجودًا
            var admin = await userMgr.FindByEmailAsync(email);
            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    UserName = email,
                    DisplayName = name,
                    PreferredLanguage = "ar",
                    EmailConfirmed = true
                };

                var res = await userMgr.CreateAsync(admin, pwd);
                if (res.Succeeded)
                {
                    await userMgr.AddToRoleAsync(admin, "Admin");
                }
                else
                {
                    // لو في خطأ بالإنشاء، ارمي استثناء ليوضح السبب في اللوج
                    var msg = string.Join("; ", res.Errors.Select(e => e.Description));
                    throw new Exception($"Admin seeding failed: {msg}");
                }
            }
            else
            {
                // تأكد أن لديه دور Admin
                if (!await userMgr.IsInRoleAsync(admin, "Admin"))
                    await userMgr.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
