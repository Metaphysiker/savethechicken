using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
