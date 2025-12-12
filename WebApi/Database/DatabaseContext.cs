using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using WebApi.Models.ModelsImpl;

public class DatabaseContext : IdentityDbContext<IdentityUser>
{
    public DbSet<SaveChickenRequest> SaveChickenRequests { get; set; }
    public DbSet<SaveChickenAction> SaveChickenActions { get; set; }
    public DbSet<Farm> Farms { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Driver> Drivers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var Database = "savethechicken";
        var Username = "savethechicken";
        var Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "savethechicken";
        var Host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        optionsBuilder.UseNpgsql($"Host={Host};Username={Username};Password={Password};Database={Database}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contact>(entity =>
        {
                entity.Property(e => e.Categories)
                    .HasConversion(
                        v => string.Join(";", v.Select(e => e.ToString())),
                        v => v.Split(';', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => Enum.Parse<ContactCategory>(s)).ToList()
                    );

            entity.Property(e => e.AvailableDates)
                .HasConversion(
                    v => string.Join(";", v.Select(d => d.ToString("yyyy-MM-dd"))),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => DateOnly.Parse(s)).ToList()
                );
        });
        base.OnModelCreating(modelBuilder);
    }
}
