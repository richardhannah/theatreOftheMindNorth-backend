namespace TheatreOfTheMind.Models;

public class User
{
    public Guid UserId { get; set; }
    public UserRole Role { get; set; } = UserRole.Guest;

    public Login Login { get; set; } = null!;
}
