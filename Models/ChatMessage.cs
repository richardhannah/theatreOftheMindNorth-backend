namespace TheatreOfTheMind.Models;

public class ChatMessage
{
    public string Name { get; set; } = "";
    public string PlayerName { get; set; } = "";
    public string TokenId { get; set; } = "";
    public string Text { get; set; } = "";
    public long Ts { get; set; }
    public bool IsDiceRoll { get; set; }
}
