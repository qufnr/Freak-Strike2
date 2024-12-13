using System.Text.Json;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Exceptions;
using FreakStrike2.Utils.Helpers;

namespace FreakStrike2.Models.Human;

/// <summary>
/// 인간 진영 클래스 객체
/// </summary>
public class FSHuman
{
    private static string Filename = "Humans.json";
        
    public static CsTeam Team = CsTeam.Terrorist;
    public static int NormalMaxHealth = 100;
    public static int NormalArmorValue = 100;
    
    [JsonPropertyName("Name")] public required string Name { get; set; }
    [JsonPropertyName("DesignerName")] public required string DesignerName { get; set; }
    [JsonPropertyName("Description")] public Dictionary<string, string> Description { get; set; } = new();
    
    [JsonPropertyName("Model")] public string? Model { get; set; }
    [JsonPropertyName("Weapons")] public List<string> Weapons { get; set; } = new() { ":weapon_knife" };
    [JsonPropertyName("MaxHealth")] public int MaxHealth { get; set; } = NormalMaxHealth;
    [JsonPropertyName("ArmorValue")] public int ArmorValue { get; set; } = NormalArmorValue;
    [JsonPropertyName("Laggedmovement")] public float Laggedmovement { get; set; } = 1f;
    [JsonPropertyName("GravityScale")] public float GravityScale { get; set; } = 1f;

    public static List<FSHuman> JsonToList()
    {
        var fullPath = Path.Combine(Server.GameDirectory, FreakStrike2.PluginConfigDirectory);
        var humans = CommonUtils.JsonFileToObject<List<FSHuman>>(fullPath, Filename);
        if (humans == null || humans.Count == 0)
            throw new SaxtonHaleUndefinedException();
        return humans;
    }
}