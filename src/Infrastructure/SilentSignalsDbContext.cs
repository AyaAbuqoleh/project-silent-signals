
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class SilentSignalsDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public SilentSignalsDbContext(DbContextOptions<SilentSignalsDbContext> options)
        : base(options) { }

    public DbSet<MorseMapping> MorseMappings => Set<MorseMapping>();
    public DbSet<SignGesture> SignGestures => Set<SignGesture>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<ConversionHistory> ConversionHistories => Set<ConversionHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MorseMapping>()
            .HasIndex(m => m.Char)
            .IsUnique();

        modelBuilder.Entity<SignGesture>()
            .HasIndex(s => new { s.TokenType, s.Token })
            .IsUnique();

        // Morse Seeding (A–Z, 0–9)
        var morse = new List<MorseMapping>
        {
            new() { Id = 1,  Char = "A", Pattern = ".-"    },
            new() { Id = 2,  Char = "B", Pattern = "-..."  },
            new() { Id = 3,  Char = "C", Pattern = "-.-."  },
            new() { Id = 4,  Char = "D", Pattern = "-.."   },
            new() { Id = 5,  Char = "E", Pattern = "."     },
            new() { Id = 6,  Char = "F", Pattern = "..-."  },
            new() { Id = 7,  Char = "G", Pattern = "--."   },
            new() { Id = 8,  Char = "H", Pattern = "...."  },
            new() { Id = 9,  Char = "I", Pattern = ".."    },
            new() { Id = 10, Char = "J", Pattern = ".---"  },
            new() { Id = 11, Char = "K", Pattern = "-.-"   },
            new() { Id = 12, Char = "L", Pattern = ".-.."  },
            new() { Id = 13, Char = "M", Pattern = "--"    },
            new() { Id = 14, Char = "N", Pattern = "-."    },
            new() { Id = 15, Char = "O", Pattern = "---"   },
            new() { Id = 16, Char = "P", Pattern = ".--."  },
            new() { Id = 17, Char = "Q", Pattern = "--.-"  },
            new() { Id = 18, Char = "R", Pattern = ".-."   },
            new() { Id = 19, Char = "S", Pattern = "..."   },
            new() { Id = 20, Char = "T", Pattern = "-"     },
            new() { Id = 21, Char = "U", Pattern = "..-"   },
            new() { Id = 22, Char = "V", Pattern = "...-"  },
            new() { Id = 23, Char = "W", Pattern = ".--"   },
            new() { Id = 24, Char = "X", Pattern = "-..-"  },
            new() { Id = 25, Char = "Y", Pattern = "-.--"  },
            new() { Id = 26, Char = "Z", Pattern = "--.."  },
            new() { Id = 27, Char = "0", Pattern = "-----" },
            new() { Id = 28, Char = "1", Pattern = ".----" },
            new() { Id = 29, Char = "2", Pattern = "..---" },
            new() { Id = 30, Char = "3", Pattern = "...--" },
            new() { Id = 31, Char = "4", Pattern = "....-" },
            new() { Id = 32, Char = "5", Pattern = "....." },
            new() { Id = 33, Char = "6", Pattern = "-...." },
            new() { Id = 34, Char = "7", Pattern = "--..." },
            new() { Id = 35, Char = "8", Pattern = "---.." },
            new() { Id = 36, Char = "9", Pattern = "----." }
        };
        modelBuilder.Entity<MorseMapping>().HasData(morse);

        // Sign Letters Seeding (A–Z)
        var letters = Enumerable.Range('A', 26).Select((c, i) =>
            new SignGesture
            {
                Id = 101 + i,
                TokenType = "Letter",
                Token = ((char)c).ToString(),
                ImageUrl = $"signs/{(char)c}.svg", // اختياري الآن
                DescriptionAR = $"إشارة الحرف {(char)c}",
                DescriptionEN = $"Sign for letter {(char)c}"
            }).ToList();

        modelBuilder.Entity<SignGesture>().HasData(letters);
    }
}
