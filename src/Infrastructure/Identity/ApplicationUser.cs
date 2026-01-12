
using System;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? DisplayName { get; set; }
        public string PreferredLanguage { get; set; } = "ar"; // ar | en
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
