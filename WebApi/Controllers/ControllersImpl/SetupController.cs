using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    public SetupController(
        DatabaseContext db,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // ---------------------------------------------------------------------
    // SEED DOMAIN DATA
    // ---------------------------------------------------------------------
    [HttpGet("seed")]
    public async Task<ActionResult> Seed()
    {

        var action = new SaveChickenAction
        {
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Dates = new List<DateOnly>
            {
                new DateOnly(2026, 4, 27),
                new DateOnly(2026, 4, 28),
                new DateOnly(2026, 4, 29),
                new DateOnly(2026, 4, 30)
            },
            Title = "Test - Rettungsaktion April 2026",
            Description = "Test",
            IsActive = true
        };

        _db.SaveChickenActions.Add(action);
        await _db.SaveChangesAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new UtcDateTimeConverter());

        // Read and seed SaveChickenRequests
        var saveChickenRequests = await ReadSeedFile<SaveChickenRequest>("SaveChickenRequestSeed.json", options);
        if (saveChickenRequests != null && saveChickenRequests.Count > 0)
        {
            // Assign the action ID to each request
            foreach (var req in saveChickenRequests)
            {
                req.SaveChickenActionId = action.Id;
            }

            _db.SaveChickenRequests.AddRange(saveChickenRequests);
            await _db.SaveChangesAsync();
        }

        // Read and seed Farms
        var farms = await ReadSeedFile<Farm>("FarmSeed.json", options);
        if (farms != null && farms.Count > 0)
        {
            // Assign the action ID to each farm
            foreach (var farm in farms)
            {
                farm.SaveChickenActionId = action.Id;
            }

            _db.Farms.AddRange(farms);
            await _db.SaveChangesAsync();
        }

        return Ok();
    }

    private async Task<List<T>?> ReadSeedFile<T>(string fileName, JsonSerializerOptions options) where T : class
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", fileName);
        if (!System.IO.File.Exists(path))
            return null;

        var json = await System.IO.File.ReadAllTextAsync(path);
        var items = JsonSerializer.Deserialize<List<T>>(json, options);

        return items;
    }

    // ---------------------------------------------------------------------
    // SETUP IDENTITY
    // ---------------------------------------------------------------------
    [HttpGet("setup")]
    public async Task<ActionResult> Setup()
    {
        await CreateRoles();
        await CreateAdminUser();
        await CreateRettetDasHuhnUser();

        // Only create test user in testing environment
        var isTestingEnvironment = Environment.GetEnvironmentVariable("TESTING_ENVIRONMENT") == "true";
        if (isTestingEnvironment)
        {
            await CreateTestUser();
        }

        return Ok();
    }

    private async Task CreateRoles()
    {
        foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
        {
            if (!await _roleManager.RoleExistsAsync(role.ToString()))
            {
                var result = await _roleManager.CreateAsync(
                    new IdentityRole(role.ToString()));

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }

    private async Task CreateAdminUser()
    {
        const string email = "s.raess@me.com";
        var user = await EnsureUserExists(email, "ADMIN_PASSWORD");
        if (user != null)
        {
            await EnsureRole(user, UserRole.Admin);
            await EnsureRole(user, UserRole.User);
        }
    }

    private async Task CreateRettetDasHuhnUser()
    {
        const string email = "rettetdashuhn@stinah.ch";
        var user = await EnsureUserExists(email, "RETTET_DAS_HUHN_PASSWORD");

        if (user != null)
        {
            await EnsureRole(user, UserRole.Admin);
            await EnsureRole(user, UserRole.User);
        }
    }

    private async Task CreateTestUser()
    {
        const string email = "test@example.com";
        var user = await EnsureUserExists(email, "TEST_PASSWORD");

        if (user != null)
        {
            await EnsureRole(user, UserRole.Admin);
            await EnsureRole(user, UserRole.User);
        }
    }

    // ---------------------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------------------
    private async Task<IdentityUser?> EnsureUserExists(string email, string passwordEnvVar)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user != null)
            return user;

        var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        var password = isDocker
            ? Environment.GetEnvironmentVariable(passwordEnvVar)
            : "password";

        user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password!);
        if (!result.Succeeded)
        {
            return null;
        }

        return user;
    }

    private async Task EnsureRole(IdentityUser user, UserRole role)
    {
        var roleName = role.ToString();
        if (!await _userManager.IsInRoleAsync(user, roleName))
        {
            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to add role '{roleName}' to '{user.Email}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}
