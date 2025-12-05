using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

public class DatabaseContext : IdentityDbContext<IdentityUser>
{
    // public DbSet<Horse> Horses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var Username = "savethechicken";
        var Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        var Database = "savethechicken";
        optionsBuilder.UseNpgsql($"Host=postgres;Username={Username};Password={Password};Database={Database}");
    }
}
