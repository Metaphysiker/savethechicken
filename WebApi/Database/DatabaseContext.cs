using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using WebApi.Models.ModelsImpl;

public class DatabaseContext : IdentityDbContext<IdentityUser>
{
    public DbSet<SaveChickenRequest> SaveChickenRequests { get; set; }
    public DbSet<SaveChickenAction> SaveChickenActions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var Username = "savethechicken";
        var Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        var Database = "savethechicken";
        optionsBuilder.UseNpgsql($"Host=postgres;Username={Username};Password={Password};Database={Database}");
    }
}
