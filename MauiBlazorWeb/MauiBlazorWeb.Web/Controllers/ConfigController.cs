using Microsoft.AspNetCore.Mvc;

[Route("api/config")]
[ApiController]
public class ConfigController : ControllerBase
{
    [HttpGet]
    public IActionResult GetConfig()
    {
        return Ok(new { ApiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:8081/" });
    }
}
