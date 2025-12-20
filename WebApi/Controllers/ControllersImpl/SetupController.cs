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
        if (System.IO.File.Exists(saveChickenRequestSeedPath))
        {
            var json = System.IO.File.ReadAllText(saveChickenRequestSeedPath);

            var requests = JsonSerializer.Deserialize<List<SaveChickenRequest>>(json, options);

            if (requests != null)
            {
                _db.SaveChickenRequests.AddRange(requests);
                await _db.SaveChangesAsync();
            }
        }

        var driverSeedPath = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", "DriverSeed.json");
        if (System.IO.File.Exists(driverSeedPath))
        {
            var json = System.IO.File.ReadAllText(driverSeedPath);
            var drivers = JsonSerializer.Deserialize<List<Driver>>(json, options);

            if (drivers != null)
            {
                _db.Drivers.AddRange(drivers);
                await _db.SaveChangesAsync();
            }
        }

        var farmPath = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", "FarmSeed.json");
        if (System.IO.File.Exists(farmPath))
        {
            var json = System.IO.File.ReadAllText(farmPath);
            var farms = JsonSerializer.Deserialize<List<Farm>>(json, options);

            if (farms != null)
            {
                _db.Farms.AddRange(farms);
                await _db.SaveChangesAsync();
            }
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
            Title = "Rettungsaktion März 2026",
            Description = "Rette Hühner im März 2026! Melde dich jetzt an, um Hühner von befreiten Höfen aufzunehmen und ihnen ein liebevolles Zuhause zu bieten.",
            IsActive = true
        };

        _db.SaveChickenActions.Add(action);
        await _db.SaveChangesAsync();


        // For Drivers
        foreach (var driver in _db.Drivers)
        {
            driver.SaveChickenAction = action;
            driver.SaveChickenActionId = action.Id;
        }
        await _db.SaveChangesAsync();

        // For Farms
        foreach (var farm in _db.Farms)
        {
            farm.SaveChickenAction = action;
            farm.SaveChickenActionId = action.Id;
        }
        await _db.SaveChangesAsync();

        // For SaveChickenRequests
        foreach (var req in _db.SaveChickenRequests)
        {
            req.SaveChickenAction = action;
            req.SaveChickenActionId = action.Id;
        }
        await _db.SaveChangesAsync();

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
            admin = new IdentityUser { UserName = UserRole.Admin.ToString() };
            string? password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
            await _userManager.CreateAsync(admin, password!);
        }

        await _userManager.AddToRoleAsync(admin, UserRole.Admin.ToString());
        await _userManager.AddToRoleAsync(admin, UserRole.User.ToString());
    }
}
