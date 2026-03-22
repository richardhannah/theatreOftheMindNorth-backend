using System.Text.Json.Serialization;

namespace TheatreOfTheMind.Models;

public class PackAnimal
{
    public Guid PackAnimalId { get; set; }
    public string Type { get; set; } = "";
    public string Name { get; set; } = "";
    public string Notes { get; set; } = "";
    public Guid? AssignedWagonId { get; set; }

    [JsonIgnore]
    public Wagon? AssignedWagon { get; set; }
}
