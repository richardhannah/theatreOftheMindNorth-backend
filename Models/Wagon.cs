using System.Text.Json.Serialization;

namespace TheatreOfTheMind.Models;

public class Wagon
{
    public Guid WagonId { get; set; }
    public string Type { get; set; } = "";
    public string Name { get; set; } = "";
    public string Notes { get; set; } = "";

    [JsonIgnore]
    public List<PackAnimal> AssignedAnimals { get; set; } = new();
}
