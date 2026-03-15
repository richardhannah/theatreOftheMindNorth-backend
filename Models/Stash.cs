using System.Text.Json.Serialization;

namespace TheatreOfTheMind.Models;

public class Stash
{
    public Guid StashId { get; set; }
    public Guid? CharacterId { get; set; }

    public string Name { get; set; } = "";
    public bool Removable { get; set; } = true;
    public bool Shared { get; set; }
    public int SortOrder { get; set; }

    // Treasure
    public int Platinum { get; set; }
    public int Gold { get; set; }
    public int Electrum { get; set; }
    public int Silver { get; set; }
    public int Copper { get; set; }

    // Equipment (JSON)
    public string Equipment { get; set; } = "[]";

    // Navigation
    [JsonIgnore]
    public Character? Character { get; set; }
}
