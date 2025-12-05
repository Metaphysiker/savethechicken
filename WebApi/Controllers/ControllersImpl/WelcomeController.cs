using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("")]
public class WelcomeController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> GetWelcomeMessage()
    {
        return Ok("Welcome to the SaveTheChicken API!");
    }
}
