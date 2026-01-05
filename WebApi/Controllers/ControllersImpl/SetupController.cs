using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos.DtosImpl;
using System.Text.Json;
using WebApi.Models.ModelsImpl;
using WebApi.Services.ServicesImpl;

[ApiController]
[Route("api/[controller]")]
public class SetupController : ControllerBase
{
    private readonly DatabaseContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public SetupController(DatabaseContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet("seed")]
    public async Task<ActionResult> Seed()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new UtcDateTimeConverter());

        var saveChickenRequestSeedPath = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", "SaveChickenRequestSeed.json");
        Console.WriteLine($"Checking SaveChickenRequestSeed.json at: {saveChickenRequestSeedPath}");
        var requests = new List<SaveChickenRequest>();
        if (System.IO.File.Exists(saveChickenRequestSeedPath))
        {
            var json = System.IO.File.ReadAllText(saveChickenRequestSeedPath);
            requests = JsonSerializer.Deserialize<List<SaveChickenRequest>>(json, options) ?? new List<SaveChickenRequest>();
            Console.WriteLine($"Loaded {requests.Count} SaveChickenRequests from seed file.");
            if (requests.Count > 0)
            {
                _db.SaveChickenRequests.AddRange(requests);
                await _db.SaveChangesAsync();
                Console.WriteLine($"Seeded {requests.Count} SaveChickenRequests.");
            }
        }
        else
        {
            Console.WriteLine("SaveChickenRequestSeed.json not found.");
        }

        var driverSeedPath = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", "DriverSeed.json");
        Console.WriteLine($"Checking DriverSeed.json at: {driverSeedPath}");
        var drivers = new List<Driver>();
        if (System.IO.File.Exists(driverSeedPath))
        {
            var json = System.IO.File.ReadAllText(driverSeedPath);
            drivers = JsonSerializer.Deserialize<List<Driver>>(json, options) ?? new List<Driver>();
            Console.WriteLine($"Loaded {drivers.Count} Drivers from seed file.");
            if (drivers.Count > 0)
            {
                _db.Drivers.AddRange(drivers);
                await _db.SaveChangesAsync();
                Console.WriteLine($"Seeded {drivers.Count} Drivers.");
            }
        }
        else
        {
            Console.WriteLine("DriverSeed.json not found.");
        }

        var farmPath = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", "FarmSeed.json");
        Console.WriteLine($"Checking FarmSeed.json at: {farmPath}");
        var farms = new List<Farm>();
        if (System.IO.File.Exists(farmPath))
        {
            var json = System.IO.File.ReadAllText(farmPath);
            farms = JsonSerializer.Deserialize<List<Farm>>(json, options) ?? new List<Farm>();
            Console.WriteLine($"Loaded {farms.Count} Farms from seed file.");
            if (farms.Count > 0)
            {
                _db.Farms.AddRange(farms);
                await _db.SaveChangesAsync();
                Console.WriteLine($"Seeded {farms.Count} Farms.");
            }
        }
        else
        {
            Console.WriteLine("FarmSeed.json not found.");
        }

        var action = new SaveChickenAction
        {
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Dates = new List<DateOnly>
            {
                new DateOnly(2026, 3, 10),
                new DateOnly(2026, 3, 24)
            },
            Title = "Rettungsaktion April 2026 xxx",
            Description = "Rette Hühner im April 2026! Melde dich jetzt an, um Hühner von befreiten Höfen aufzunehmen und ihnen ein liebevolles Zuhause zu bieten.",
            IsActive = true
        };

        _db.SaveChickenActions.Add(action);
        await _db.SaveChangesAsync();
        Console.WriteLine("Seeded SaveChickenAction.");

        // Only link the newly added drivers, farms, and requests
        foreach (var driver in drivers)
        {
            driver.SaveChickenAction = action;
            driver.SaveChickenActionId = action.Id;
        }
        await _db.SaveChangesAsync();
        Console.WriteLine("Linked SaveChickenAction to Drivers.");

        foreach (var farm in farms)
        {
            farm.SaveChickenAction = action;
            farm.SaveChickenActionId = action.Id;
        }
        await _db.SaveChangesAsync();
        Console.WriteLine("Linked SaveChickenAction to Farms.");

        foreach (var req in requests)
        {
            req.SaveChickenAction = action;
            req.SaveChickenActionId = action.Id;
        }
        await _db.SaveChangesAsync();
        Console.WriteLine("Linked SaveChickenAction to SaveChickenRequests.");

        return Ok();
    }

    [HttpGet("setup")]
    public async Task<ActionResult> Setup()
    {
        await CreateRoles();
        await CreateAdminUser();
        return Ok();
    }

        private async Task CreateRoles()
    {
        UserRole[] roles = (UserRole[])Enum.GetValues(typeof(UserRole));
        foreach (var role in roles)
        {
            var found = await _roleManager.FindByNameAsync(role.ToString());
            if (found == null)
            {
                var identityRole = new IdentityRole { Name = role.ToString() };
                await _roleManager.CreateAsync(identityRole);
            }
        }
    }

    private async Task CreateAdminUser()
    {
        var admin = await _userManager.FindByNameAsync(UserRole.Admin.ToString());
        if (admin == null)
        {
            admin = new IdentityUser { UserName = "s.raess@me.com" };
            var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
            string? password = "password";
            if (isDocker) { password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD"); }
            await _userManager.CreateAsync(admin, password!);
        }

        await _userManager.AddToRoleAsync(admin, UserRole.Admin.ToString());
        await _userManager.AddToRoleAsync(admin, UserRole.User.ToString());
    }
}
