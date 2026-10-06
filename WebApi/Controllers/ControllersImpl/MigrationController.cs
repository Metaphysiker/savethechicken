using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.DtosImpl;
using System.Text.Json;
using WebApi.Models.ModelsImpl;
using WebApi.Services.ServicesImpl;

[ApiController]
[Route("api/[controller]")]
public class MigrationController : ControllerBase
{
    private readonly DatabaseContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public MigrationController(
        DatabaseContext db,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // ---------------------------------------------------------------------

    [HttpGet("migrate")]
    public async Task<ActionResult> Migrate()
    {
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var farms = await _db.Farms
                .Where(f => f.SaveChickenActionId != null)
                .ToListAsync();

            if (farms.Count == 0)
                return Ok(new { Message = "No farms found requiring migration." });

            var existing = (await _db.Set<SaveChickenActionFarm>()
                    .Select(x => new { x.FarmId, x.SaveChickenActionId })
                    .ToListAsync())
                .Select(x => (x.FarmId, x.SaveChickenActionId))
                .ToHashSet();

            var now = DateTime.UtcNow;
            var created = 0;

            foreach (var farm in farms)
            {
                var actionId = farm.SaveChickenActionId!.Value;

                if (existing.Add((farm.Id, actionId)))
                {
                    _db.Set<SaveChickenActionFarm>().Add(new SaveChickenActionFarm
                    {
                        FarmId = farm.Id,
                        SaveChickenActionId = actionId,
                        NumberOfChickensToBeSaved = farm.NumberOfChickens,
                        NumberOfRoostersToBeSaved = farm.NumberOfRoosters,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                    created++;
                }

                farm.SaveChickenActionId = null;
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new { TotalFarmsProcessed = farms.Count, NewJoinEntriesCreated = created });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return StatusCode(500, new { Error = ex.Message, Inner = ex.InnerException?.Message });
        }
    }
}
