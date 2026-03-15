namespace TheatreOfTheMind.Models;

public class LoginResponse
{
    public Guid Token { get; set; }
    public string Username { get; set; } = "";
    public string Role { get; set; } = "";
}
