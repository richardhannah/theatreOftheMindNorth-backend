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
        var login = (Login)HttpContext.Items["Login"]!;
        var user = (User)HttpContext.Items["User"]!;
        return Ok(new { message = $"Hello, {login.Username}!", role = user.Role.ToString() });
    }
}
