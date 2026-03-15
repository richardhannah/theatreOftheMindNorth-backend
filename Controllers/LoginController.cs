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
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { error = "Username and password are required." });

        if (dto.Action.Equals("logout", StringComparison.OrdinalIgnoreCase))
        {
            var success = await _loginService.LogoutAsync(dto);
            if (!success)
                return Unauthorized(new { error = "Invalid credentials." });

            return Ok(new { message = "Logged out." });
        }

        var result = await _loginService.LoginOrRegisterAsync(dto);

        if (result == null)
            return Unauthorized(new { error = "Invalid password." });

        return Ok(result);
    }
}
