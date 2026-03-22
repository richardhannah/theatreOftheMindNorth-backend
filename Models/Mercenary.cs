namespace TheatreOfTheMind.Models;

public class Mercenary
{
    public Guid MercenaryId { get; set; }
    public string Type { get; set; } = "";
    public string Race { get; set; } = "";
    public int Count { get; set; }
}
