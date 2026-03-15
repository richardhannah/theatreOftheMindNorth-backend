using Microsoft.AspNetCore.Mvc;
using TheatreOfTheMind.Auth;
using TheatreOfTheMind.Models;

namespace TheatreOfTheMind.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    [HttpGet]
    [TokenAuth]
    public IActionResult Get()
    {
        var user = (Login)HttpContext.Items["User"]!;
        return Ok(new { message = $"Hello, {user.Username}!" });
    }
}
