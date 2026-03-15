namespace TheatreOfTheMind.Models;

public class BeaconPayload
{
    public string Token { get; set; } = "";
    public string Character { get; set; } = "";
    public List<Stash> Stashes { get; set; } = new();
}
