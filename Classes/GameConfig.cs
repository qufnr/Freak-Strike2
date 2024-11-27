using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace FreakStrike2.Classes;

public class GameConfig : BasePluginConfig
{
    [JsonPropertyName("FindInterval")] public int FindInterval { get; set; } = 10;
    [JsonPropertyName("CanResetQueuepoints")] public bool CanResetQueuepoints { get; set; } = true;
    [JsonPropertyName("UnrealityFallDamage")] public bool UnrealityFallDamage { get; set; } = true;
    [JsonPropertyName("RoundTime")] public float RoundTime { get; set; } = 3f;
}