using System.Collections.Generic;
using System.Text.Json.Serialization;

// Al ser una clase de datos pura, no necesita heredar de Node
public class LevelData 
{
    [JsonPropertyName("family_name")]
    public string FamilyName { get; set; }

    [JsonPropertyName("objects")]
    public List<string> Objects { get; set; }

    public LevelData() { }

    public LevelData(string familyName, List<string> objects) 
    {
        FamilyName = familyName;
        Objects = objects;
    }
}
