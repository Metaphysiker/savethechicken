using Microsoft.AspNetCore.Mvc;

[Route("api/config")]
[ApiController]
public class ConfigController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ConfigController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult GetConfig()
    {
        // Priority: Environment Variable > Configuration > Default
        var apiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL")
            ?? _configuration["API_BASE_URL"]
            ?? "https://localhost:7101/";

        // Ensure trailing slash for proper URL combination
        if (!apiBaseUrl.EndsWith("/"))
        {
            apiBaseUrl += "/";
        }

        return Ok(new { ApiBaseUrl = apiBaseUrl });
    }
}
