namespace TheatreOfTheMind.Models;

public class LoginDto
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string Action { get; set; } = "login";
}
