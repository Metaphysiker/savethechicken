using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

public class TokenService
{
    public static int ExpirationMinutes { get; } = 525600; // 1 year
    private UserManager<IdentityUser> userManager;


    public TokenService(UserManager<IdentityUser> userManager)
    {
        this.userManager = userManager;

    }

    public async Task<string> CreateToken(IdentityUser user)
    {
        var expiration = DateTime.UtcNow.AddMinutes(ExpirationMinutes);
        var token = CreateJwtToken(
            await CreateClaims(user),
            CreateSigningCredentials(),
            expiration
        );
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    private JwtSecurityToken CreateJwtToken(List<Claim> claims, SigningCredentials credentials,
        DateTime expiration) =>
        new(
            "apiWithAuthBackend",
            "apiWithAuthBackend",
            claims,
            expires: expiration,
            signingCredentials: credentials
        );

    private async Task<List<Claim>> CreateClaims(IdentityUser user)
    {
        try
        {

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user.UserName == null)
            {
                throw new ArgumentNullException(nameof(user.UserName));
            }

            var userIdClaim = new Claim(type: "UserId", user.Id);

            var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, "TokenForTheApiWithAuth"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    userIdClaim,
                    new Claim(ClaimTypes.Name, user.UserName),
                };

            if (user.Email != null)
            {
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
            }

            var userRoles = await userManager.GetRolesAsync(user);

            foreach (var role in userRoles)
            {
                Claim newClaim = new Claim(ClaimTypes.Role, role);
                claims.Add(newClaim);
            }

            return claims;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    private SigningCredentials CreateSigningCredentials()
    {

        var key = Environment.GetEnvironmentVariable("SIGNING_KEY");
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        return new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key)
            ),
            SecurityAlgorithms.HmacSha256
        );
    }
}

