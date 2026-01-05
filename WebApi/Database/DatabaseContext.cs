using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using WebApi.Models.ModelsImpl;

public class DatabaseContext : IdentityDbContext<IdentityUser>
{
    public DbSet<SaveChickenRequest> SaveChickenRequests { get; set; }
    public DbSet<SaveChickenAction> SaveChickenActions { get; set; }
    public DbSet<Farm> Farms { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<StoredFile> Files { get; set; }
    public DbSet<BlackListedPerson> BlackListedPersons { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var Database = "savethechicken";
        var Username = "savethechicken";
        var Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "savethechicken";
        var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        var Host = isDocker ? "postgres" : "localhost";
        optionsBuilder.UseNpgsql($"Host={Host};Username={Username};Password={Password};Database={Database}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Driver>(entity =>
        {
            entity
            .Property(b => b.SearchVector)
            .HasComputedColumnSql(
                @"to_tsvector('german', 
                                coalesce(""CarMake"", '')
                            )", stored: true);

            entity.HasIndex(e => e.SearchVector)
                .HasMethod("GIN");

        });

        modelBuilder.Entity<Farm>(entity =>
        {
            entity
            .Property(b => b.SearchVector)
            .HasComputedColumnSql(
                @"to_tsvector('german', 
                                coalesce(""Size"", '') || ' ' ||
                                coalesce(""Color"", '') || ' ' ||
                                coalesce(""GeneralInformation"", '') || ' ' ||
                                coalesce(""Name"", '')
                            )", stored: true);
            entity.HasIndex(e => e.SearchVector)
    .HasMethod("GIN");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity
            .Property(b => b.SearchVector)
            .HasComputedColumnSql(
                @"to_tsvector('german', 
                                coalesce(""FirstName"", '') || ' ' ||
                                coalesce(""LastName"", '') || ' ' ||
                                coalesce(""Email"", '') || ' ' ||
                                coalesce(""PhoneNumber"", '')
                            )", stored: true);

            entity.HasIndex(e => e.SearchVector)
    .HasMethod("GIN");

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

        modelBuilder.Entity<Address>(entity =>
        {
            entity
            .Property(b => b.SearchVector)
            .HasComputedColumnSql(
                @"to_tsvector('german', 
                                coalesce(""Street"", '') || ' ' ||
                                coalesce(""City"", '') || ' ' ||
                                coalesce(""PostalCode"", '')
                            )", stored: true);

            entity.HasIndex(e => e.SearchVector)
    .HasMethod("GIN");

            entity.OwnsOne(e => e.GeoCoordinate);
        });

        modelBuilder.Entity<SaveChickenRequest>(entity =>
        {
            entity
                .Property(b => b.SearchVector)
                .HasComputedColumnSql(
                    @"to_tsvector('german', 
                        coalesce(""DescriptionOfPlaceForChickens"", '') || ' ' ||
                        coalesce(""Message"", '') || ' ' ||
                        coalesce(""Color"", '')
                    )", stored: true);

            entity.HasIndex(e => e.SearchVector)
    .HasMethod("GIN");

            entity.Property(e => e.DatesForHandOver)
                .HasConversion(
                    v => string.Join(";", v.Select(d => d.ToString("yyyy-MM-dd"))),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => DateOnly.Parse(s)).ToList()
                );

            entity.Property(e => e.BlackListedPersonIds)
                .HasConversion(
                    v => string.Join(";", v),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse).ToList()
                );
        });

        base.OnModelCreating(modelBuilder);
    }
}
