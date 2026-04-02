using Microsoft.AspNetCore.Mvc;
using TheatreOfTheMind.Auth;
using TheatreOfTheMind.Models;
using TheatreOfTheMind.Repositories;
using TheatreOfTheMind.Services;

namespace TheatreOfTheMind.Controllers;

[ApiController]
[Route("api/users")]
[TokenAuth]
[RequireRole(UserRole.Admin)]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILoginRepository _loginRepository;

    public UsersController(IUserRepository userRepository, ILoginRepository loginRepository)
    {
        _userRepository = userRepository;
        _loginRepository = loginRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userRepository.GetAllWithLoginsAsync();
        var result = users.Select(u => new
        {
            u.UserId,
            Username = u.Login?.Username ?? "",
            Role = u.Role.ToString()
        });
        return Ok(result);
    }

    [HttpPut("{userId}/role")]
    public async Task<IActionResult> UpdateRole(Guid userId, [FromBody] UpdateRoleDto dto)
    {
        if (!Enum.TryParse<UserRole>(dto.Role, true, out var role))
            return BadRequest(new { error = "Invalid role. Use 'Guest', 'User', 'GamesMaster', or 'Admin'." });

        var user = await _userRepository.GetByUserIdAsync(userId);
        if (user == null)
            return NotFound(new { error = "User not found." });

        await _userRepository.UpdateRoleAsync(userId, role);
        return Ok(new { message = "Role updated." });
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> Delete(Guid userId)
    {
        var user = await _userRepository.GetByUserIdAsync(userId);
        if (user == null)
            return NotFound(new { error = "User not found." });

        await _userRepository.DeleteAsync(userId);
        return Ok(new { message = "User deleted." });
    }

    [HttpPost("{userId}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid userId, [FromBody] ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NewPassword))
            return BadRequest(new { error = "Password cannot be empty." });

        var user = await _userRepository.GetByUserIdAsync(userId);
        if (user == null)
            return NotFound(new { error = "User not found." });

        var salt = PasswordHasher.GenerateSalt();
        var hashedPassword = PasswordHasher.HashPassword(dto.NewPassword, salt);

        await _loginRepository.UpdatePasswordAsync(userId, hashedPassword, salt);
        return Ok(new { message = "Password reset." });
    }
}
