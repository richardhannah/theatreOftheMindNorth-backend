using System.Text.Json.Serialization;

namespace TheatreOfTheMind.Models;

public class Character
{
    public Guid CharacterId { get; set; }
    public Guid UserId { get; set; }

    // Identity
    public string Name { get; set; } = "";
    public string PlayerName { get; set; } = "";
    public string TokenId { get; set; } = "";
    public string Class { get; set; } = "";
    public int Level { get; set; }
    public int Xp { get; set; }
    public string Alignment { get; set; } = "";
    public string Title { get; set; } = "";

    // Ability Scores
    public int Str { get; set; }
    public int Int { get; set; }
    public int Wis { get; set; }
    public int Dex { get; set; }
    public int Con { get; set; }
    public int Cha { get; set; }

    // Combat
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public int Ac { get; set; }
    public int Thac0 { get; set; }
    public string Movement { get; set; } = "";
    public string Initiative { get; set; } = "";

    // Saving Throws
    public int SavDeathPoison { get; set; }
    public int SavWands { get; set; }
    public int SavParalysisStone { get; set; }
    public int SavBreathAttack { get; set; }
    public int SavSpellsStaffRod { get; set; }

    // JSON columns
    public string ClassAbilities { get; set; } = "[]";
    public string Skills { get; set; } = "[]";
    public string WeaponMasteries { get; set; } = "[]";
    public string PreparedSpells { get; set; } = "[]";
    public string Spellbook { get; set; } = "[]";

    // Notes
    public string Notes { get; set; } = "";

    // Navigation
    [JsonIgnore]
    public User? User { get; set; }
    [JsonIgnore]
    public List<Stash> Stashes { get; set; } = new();
}
