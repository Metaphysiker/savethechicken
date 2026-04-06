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
    public DbSet<StoredFile> Files { get; set; }
    public DbSet<Person> Persons { get; set; }
    public DbSet<SaveChickenDriveRequest> SaveChickenDriveRequests { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var Database = "savethechicken";
        var Username = "savethechicken";
        var Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "savethechicken";
        var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        var Host = isDocker ? "postgres" : "localhost";
        optionsBuilder.UseNpgsql($"Host={Host};Username={Username};Password={Password};Database={Database}")
            .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

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

            entity.HasOne(e => e.Contact)
                .WithOne(c => c.Farm)
                .HasForeignKey<Farm>(e => e.ContactId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Address)
                .WithOne(a => a.Farm)
                .HasForeignKey<Farm>(e => e.AddressId)
                .OnDelete(DeleteBehavior.Cascade);
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
                                coalesce(""PhoneNumber"", '') || ' ' ||
                                coalesce(""CarMake"", '')
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

            entity.Property(e => e.BlackListedPersonIds)
                .HasConversion(
                    v => string.Join(";", v),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse).ToList()
                );
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity
                .Property(b => b.SearchVector)
                .HasComputedColumnSql(
                    @"to_tsvector('german', '')", stored: true);

            entity.HasIndex(e => e.SearchVector)
                .HasMethod("GIN");

            entity.HasMany(e => e.SaveChickenRequests)
                .WithOne(r => r.Person)
                .HasForeignKey(e => e.PersonId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.SaveChickenDriveRequests)
                .WithOne(e => e.Person)
                .HasForeignKey(e => e.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Contact)
                .WithOne(c => c.Person)
                .HasForeignKey<Person>(e => e.ContactId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Address)
                .WithOne(a => a.Person)
                .HasForeignKey<Person>(e => e.AddressId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SaveChickenDriveRequest>(entity =>
        {
            entity
                .Property(b => b.SearchVector)
                .HasComputedColumnSql(
                    @"to_tsvector('german',
                        coalesce(""CarMake"", '') || ' ' ||
                        coalesce(""Message"", '')
                    )", stored: true);

            entity.HasIndex(e => e.SearchVector)
                .HasMethod("GIN");

            entity.Property(e => e.AvailableDates)
                .HasConversion(
                    v => string.Join(";", v.Select(d => d.ToString("yyyy-MM-dd"))),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => DateOnly.Parse(s)).ToList()
                );
        });

        modelBuilder.Entity<SaveChickenAction>(entity =>
        {
            entity.HasMany(e => e.Farms)
                .WithOne(f => f.SaveChickenAction)
                .HasForeignKey(f => f.SaveChickenActionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.SaveChickenDriveRequests)
                .WithOne(r => r.SaveChickenAction)
                .HasForeignKey(r => r.SaveChickenActionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(e => e.Dates)
                .HasConversion(
                    v => string.Join(";", v.Select(d => d.ToString("yyyy-MM-dd"))),
                    // Handle empty, semicolon-separated, or legacy PostgreSQL array format
                    v => string.IsNullOrWhiteSpace(v) || v == "{}"
                        ? new List<DateOnly>()
                        : v.TrimStart('{').TrimEnd('}')  // Remove PostgreSQL array braces
                            .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => DateOnly.Parse(s.Trim())).ToList()
                );
        });

        base.OnModelCreating(modelBuilder);
    }
}
