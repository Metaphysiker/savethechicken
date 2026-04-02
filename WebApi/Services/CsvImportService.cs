using System.Collections.Concurrent;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shared.CsvRecords;
using Shared.Dtos.DtosImpl;
using WebApi.Database;
using WebApi.Models.ModelsImpl;

namespace WebApi.Services;

public class CsvImportService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CsvImportService> _logger;
    private static readonly ConcurrentDictionary<Guid, ImportStatus> _importJobs = new();

    public CsvImportService(IServiceScopeFactory scopeFactory, ILogger<CsvImportService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public Guid StartImport(string filePath, string entityType)
    {
        var jobId = Guid.NewGuid();
        var status = new ImportStatus
        {
            JobId = jobId,
            EntityType = entityType,
            Status = "Processing",
            StartedAt = DateTime.UtcNow
        };

        _importJobs[jobId] = status;

        // Start background task
        _ = Task.Run(() => ProcessImportAsync(jobId, filePath, entityType));

        return jobId;
    }

    public ImportStatus? GetStatus(Guid jobId)
    {
        return _importJobs.TryGetValue(jobId, out var status) ? status : null;
    }

    private async Task ProcessImportAsync(Guid jobId, string filePath, string entityType)
    {
        var status = _importJobs[jobId];

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            // Read and count records first
            var records = ReadCsvFile(filePath, entityType).ToList();
            status.TotalRecords = records.Count;

            // Process in batches
            const int batchSize = 1000;
            for (int i = 0; i < records.Count; i += batchSize)
            {
                var batch = records.Skip(i).Take(batchSize).ToList();

                await ProcessBatch(dbContext, batch, entityType);

                status.ProcessedRecords += batch.Count;
                status.SuccessCount += batch.Count;

                _logger.LogInformation($"Import {jobId}: Processed {status.ProcessedRecords}/{status.TotalRecords}");
            }

            status.Status = "Completed";
            status.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Import {jobId} failed");
            status.Status = "Failed";
            status.ErrorMessage = ex.Message;
            status.CompletedAt = DateTime.UtcNow;
        }
        finally
        {
            // Clean up temp file
            try { File.Delete(filePath); } catch { /* ignore */ }
        }
    }

    private IEnumerable<object> ReadCsvFile(string filePath, string entityType)
    {
        using var reader = new StreamReader(filePath, System.Text.Encoding.UTF8);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            TrimOptions = CsvHelper.Configuration.TrimOptions.Trim,
            BadDataFound = null  // Ignore bad data instead of throwing
        };

        using var csv = new CsvReader(reader, config);

        return entityType switch
        {
            "Person" => csv.GetRecords<PersonCsvRecord>().Cast<object>().ToList(),
            "SaveChickenRequest" => csv.GetRecords<SaveChickenRequestCsvRecord>().Cast<object>().ToList(),
            "SaveChickenDriveRequest" => csv.GetRecords<SaveChickenDriveRequestCsvRecord>().Cast<object>().ToList(),
            "Farm" => csv.GetRecords<FarmCsvRecord>().Cast<object>().ToList(),
            "RettetDasHuhnArchive" => csv.GetRecords<RettetDasHuhnCsvRecord>().Cast<object>().ToList(),
            "Fahrer" => csv.GetRecords<FahrerCsvRecord>().Cast<object>().ToList(),
            "Betrieb" => csv.GetRecords<BetriebCsvRecord>().Cast<object>().ToList(),
            _ => throw new ArgumentException($"Unknown entity type: {entityType}")
        };
    }

    private async Task ProcessBatch(DatabaseContext dbContext, List<object> batch, string entityType)
    {
        switch (entityType)
        {
            case "Person":
                await ImportPersons(dbContext, batch.Cast<PersonCsvRecord>().ToList());
                break;
            case "SaveChickenRequest":
                await ImportSaveChickenRequests(dbContext, batch.Cast<SaveChickenRequestCsvRecord>().ToList());
                break;
            case "SaveChickenDriveRequest":
                await ImportSaveChickenDriveRequests(dbContext, batch.Cast<SaveChickenDriveRequestCsvRecord>().ToList());
                break;
            case "Farm":
                await ImportFarms(dbContext, batch.Cast<FarmCsvRecord>().ToList());
                break;
            case "RettetDasHuhnArchive":
                await ImportRettetDasHuhnArchive(dbContext, batch.Cast<RettetDasHuhnCsvRecord>().ToList());
                break;
            case "Fahrer":
                await ImportFahrer(dbContext, batch.Cast<FahrerCsvRecord>().ToList());
                break;
            case "Betrieb":
                await ImportBetriebe(dbContext, batch.Cast<BetriebCsvRecord>().ToList());
                break;
        }
    }

    private async Task ImportPersons(DatabaseContext dbContext, List<PersonCsvRecord> records)
    {
        var persons = records.Select(r => new Person
        {
            Contact = new Contact
            {
                FirstName = r.FirstName ?? string.Empty,
                LastName = r.LastName ?? string.Empty,
                Email = r.Email ?? string.Empty,
                PhoneNumber = r.PhoneNumber ?? string.Empty
            },
            Address = new Address
            {
                Street = r.Street ?? string.Empty,
                PostalCode = r.PostalCode ?? string.Empty,
                City = r.City ?? string.Empty
            }
        }).ToList();

        dbContext.Persons.AddRange(persons);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
    }

    private async Task ImportSaveChickenRequests(DatabaseContext dbContext, List<SaveChickenRequestCsvRecord> records)
    {
        var requests = records.Select(r => new SaveChickenRequest
        {
            PersonId = r.PersonId,
            SaveChickenActionId = r.SaveChickenActionId,
            NumberOfChickensToBeSaved = r.NumberOfChickens,
            NumberOfRoostersToBeSaved = r.NumberOfRoosters ?? 0,
            DescriptionOfPlaceForChickens = r.Description ?? string.Empty,
            Message = r.Message ?? string.Empty,
            ConfirmThatIFulfillCriteria = r.ConfirmCriteria,
            AcceptTermsAndConditions = r.AcceptTerms
        }).ToList();

        dbContext.SaveChickenRequests.AddRange(requests);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
    }

    private async Task ImportSaveChickenDriveRequests(DatabaseContext dbContext, List<SaveChickenDriveRequestCsvRecord> records)
    {
        var requests = records.Select(r => new SaveChickenDriveRequest
        {
            PersonId = r.PersonId,
            SaveChickenActionId = r.SaveChickenActionId,
            CarMake = r.CarMake ?? string.Empty,
            CapacityForChickens = r.CapacityForChickens,
            AvailableDates = r.AvailableDates?.Split(';').Select(d => DateOnly.FromDateTime(DateTime.Parse(d))).ToList() ?? new List<DateOnly>(),
            Message = r.Message ?? string.Empty
        }).ToList();

        dbContext.SaveChickenDriveRequests.AddRange(requests);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
    }

    private async Task ImportFarms(DatabaseContext dbContext, List<FarmCsvRecord> records)
    {
        foreach (var r in records)
        {
            var farm = new Farm
            {
                SaveChickenActionId = r.SaveChickenActionId,
                Name = r.FarmName ?? string.Empty,
                GeneralInformation = r.Description ?? string.Empty,
                Contact = new Contact
                {
                    FirstName = string.Empty,
                    LastName = string.Empty
                },
                Address = new Address
                {
                    Street = string.Empty,
                    City = string.Empty,
                    PostalCode = string.Empty
                }
            };

            dbContext.Farms.Add(farm);
        }

        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
    }

    private async Task ImportRettetDasHuhnArchive(DatabaseContext dbContext, List<RettetDasHuhnCsvRecord> records)
    {
        // Import just Person records from the archive CSV
        var persons = records.Select(r => new Person
        {
            Contact = new Contact
            {
                FirstName = r.Vorname ?? string.Empty,
                LastName = r.Name ?? string.Empty,
                Email = r.Email ?? string.Empty,
                PhoneNumber = (r.Telefon1 ?? r.Telefon2) ?? string.Empty
            },
            Address = new Address
            {
                Street = r.Strasse ?? string.Empty,
                City = r.Ort ?? string.Empty,
                PostalCode = r.PLZ ?? string.Empty
            },
            Comment = r.Bemerkungen ?? string.Empty
        }).ToList();

        dbContext.Persons.AddRange(persons);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
    }

    private async Task ImportFahrer(DatabaseContext dbContext, List<FahrerCsvRecord> records)
    {
        // Import driver/transport people as Person records
        // Store vehicle info in Comment: "Fahrzeug: {vehicle}. {Bemerkungen}"
        var persons = records.Select(r => new Person
        {
            Contact = new Contact
            {
                FirstName = r.Vorname ?? string.Empty,
                LastName = r.Name ?? string.Empty,
                Email = r.Email ?? string.Empty,
                PhoneNumber = (r.Telefon1 ?? r.Telefon2) ?? string.Empty
            },
            Address = new Address
            {
                Street = r.Strasse ?? string.Empty,
                City = r.Ort ?? string.Empty,
                PostalCode = r.PLZ ?? string.Empty
            },
            Comment = string.Join(". ", new[] {
                !string.IsNullOrEmpty(r.Fahrzeug) ? $"Fahrzeug: {r.Fahrzeug}" : null,
                !string.IsNullOrEmpty(r.Bemerkungen) ? r.Bemerkungen : null
            }.Where(x => x != null)).Trim()
        }).ToList();

        dbContext.Persons.AddRange(persons);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
    }

    private async Task ImportBetriebe(DatabaseContext dbContext, List<BetriebCsvRecord> records)
    {
        // Import farms (betriebe) as Farm records
        var farms = records.Select(r => new Farm
        {
            Name = $"{r.Vorname} {r.Name}".Trim(),
            Contact = new Contact
            {
                FirstName = r.Vorname ?? string.Empty,
                LastName = r.Name ?? string.Empty,
                PhoneNumber = (r.Telefon1 ?? r.Telefonf2) ?? string.Empty
            },
            Address = new Address
            {
                Street = r.Strasse ?? string.Empty,
                City = r.Ort ?? string.Empty,
                PostalCode = r.PLZ ?? string.Empty
            },
            Color = r.Farbe ?? string.Empty,
            GeneralInformation = string.Join(". ", new[] {
                !string.IsNullOrEmpty(r.Ausstallung) ? $"Ausstallung: {r.Ausstallung}" : null,
                !string.IsNullOrEmpty(r.Bemerkungen) ? r.Bemerkungen : null
            }.Where(x => x != null)).Trim()
        }).ToList();

        dbContext.Farms.AddRange(farms);
        await dbContext.SaveChangesAsync();
       dbContext.ChangeTracker.Clear();
    }
}

// CSV Record classes
public class PersonCsvRecord
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Street { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
}

public class SaveChickenRequestCsvRecord
{
    public int PersonId { get; set; }
    public int? SaveChickenActionId { get; set; }
    public int NumberOfChickens { get; set; }
    public int? NumberOfRoosters { get; set; }
    public string? Description { get; set; }
    public string? Message { get; set; }
    public bool ConfirmCriteria { get; set; }
    public bool AcceptTerms { get; set; }
}

public class SaveChickenDriveRequestCsvRecord
{
    public int PersonId { get; set; }
    public int? SaveChickenActionId { get; set; }
    public string? CarMake { get; set; }
    public int CapacityForChickens { get; set; }
    public string? AvailableDates { get; set; } // Semicolon-separated dates
    public string? Message { get; set; }
}

public class FarmCsvRecord
{
    public int? SaveChickenActionId { get; set; }
    public string? FarmName { get; set; }
    public string? Description { get; set; }
}

public class FahrerCsvRecord
{
    public string? Vorname { get; set; }
    public string? Name { get; set; }
    public string? Zusatz { get; set; }
    public string? Strasse { get; set; }
    public string? PLZ { get; set; }
    public string? Ort { get; set; }
    public string? Telefon1 { get; set; }
    public string? Telefon2 { get; set; }
    public string? Email { get; set; }
    public string? Bemerkungen { get; set; }
    public string? selberHuhner { get; set; }
    public string? Fahrzeug { get; set; }
}

public class BetriebCsvRecord
{
    public string? Vorname { get; set; }
    public string? Name { get; set; }
    public string? Zusatz { get; set; }
    public string? Strasse { get; set; }
    public string? PLZ { get; set; }
    public string? Ort { get; set; }
    public string? Telefon1 { get; set; }
    public string? Telefonf2 { get; set; } // Note the typo in CSV
    public string? Ausstallung { get; set; }
    public string? Bemerkungen { get; set; }
    public string? du { get; set; }
    public string? Farbe { get; set; }
}

public class ImportStatus
{
    public Guid JobId { get; set; }
    public string? EntityType { get; set; }
    public string? Status { get; set; } // Processing, Completed, Failed
    public int TotalRecords { get; set; }
    public int ProcessedRecords { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
