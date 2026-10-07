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
        return Ok(new { Message = "Migration no longer needed - SaveChickenActionId removed from Farm." });
    }
}
