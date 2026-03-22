namespace TheatreOfTheMind.Models;

public class ExpeditionState
{
    public string Id { get; set; } = "singleton";
    public string ForemanName { get; set; } = "";
    public string ForemanTokenId { get; set; } = "";
    public string DisabledRaces { get; set; } = "[]";
    public string Notes { get; set; } = "";
}
