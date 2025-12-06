using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly DatabaseContext _db;
    private readonly TokenService _tokenService;
    public AuthController(UserManager<IdentityUser> userManager, DatabaseContext db, TokenService tokenService)
    {
        _userManager = userManager;
        _db = db;
        _tokenService = tokenService;
    }


    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(RegistrationRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var result = await _userManager.CreateAsync(
            new IdentityUser { UserName = request.Username, Email = request.Email },
            request.Password
        );
        if (result.Succeeded)
        {
            request.Password = "";
            return CreatedAtAction(nameof(Register), new { email = request.Email }, request);
        }
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }
        return BadRequest(ModelState);
    }


    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<AuthResponseDto>> Authenticate([FromBody] AuthRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var managedUser = await _userManager.FindByEmailAsync(request.Email) ??
                          await _userManager.FindByNameAsync(request.Email);

        if (managedUser == null)
        {
            return BadRequest("User not found");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(managedUser, request.Password);
        if (!isPasswordValid)
        {
            return BadRequest("Login failed");
        }

        await _db.SaveChangesAsync();
        return Ok(await BuildAuthResponse(managedUser));
    }

    [HttpGet, Authorize]
    [Route("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken()
    {
        if (User.Identity == null || !User.Identity.IsAuthenticated)
        {
            return BadRequest("User not authenticated");
        }

        var claimsWithId = User.Claims.Where(c => c.Type == "UserId");
        var foundUser = await _userManager.FindByIdAsync(claimsWithId.First().Value);

        if (foundUser == null)
        {
            return BadRequest("User not found");
        }

        return Ok(await BuildAuthResponse(foundUser));
    }

    private async Task<AuthResponseDto> BuildAuthResponse(IdentityUser user)
    {
        var accessToken = await _tokenService.CreateToken(user);
        return new AuthResponseDto
        {
            UserId = user.Id,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Token = accessToken,
            Roles = (await _userManager.GetRolesAsync(user)).ToList(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(TokenService.ExpirationMinutes)
        };
    }

    [HttpGet, Authorize]
    [Route("is-logged-in")]
    public ActionResult<AuthResponseDto> CheckIfLoggedIn()
    {
        return Ok();
    }
}
