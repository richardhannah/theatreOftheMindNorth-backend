namespace TheatreOfTheMind.Models;

public class VttSceneBackup
{
    public int BackupId { get; set; }
    public string SceneId { get; set; } = "";
    public string Name { get; set; } = "";
    public string MapId { get; set; } = "";
    public int GridW { get; set; } = 20;
    public int GridH { get; set; } = 20;
    public double GridOffsetX { get; set; }
    public double GridOffsetY { get; set; }
    public string GridColor { get; set; } = "#ffffff";
    public double GridOpacity { get; set; } = 0.15;
    public int GridThickness { get; set; } = 1;
    public string Counters { get; set; } = "[]";
    public string Tiles { get; set; } = "[]";
    public bool FogEnabled { get; set; }
    public string FogReveals { get; set; } = "[]";
    public bool IsActive { get; set; }
    public DateTime BackupTimestamp { get; set; }
}
