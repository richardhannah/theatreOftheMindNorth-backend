using Microsoft.AspNetCore.Mvc;
using TheatreOfTheMind.Models;
using TheatreOfTheMind.Services;

namespace TheatreOfTheMind.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly ILoginService _loginService;

    public LoginController(ILoginService loginService)
    {
        _loginService = loginService;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username))
            return BadRequest(new { error = "Username is required." });

        if (dto.Action.Equals("logout", StringComparison.OrdinalIgnoreCase))
        {
            return Ok(new { message = "Logged out." });
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { error = "Password is required." });

        var result = await _loginService.LoginOrRegisterAsync(dto);

        if (result == null)
            return Unauthorized(new { error = "Invalid password." });

        return Ok(result);
    }
}
