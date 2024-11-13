using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace FreakStrike2.Classes;

public class GameConfig : BasePluginConfig
{
    [JsonPropertyName("FindInterval")] public int FindInterval { get; set; } = 20;
    [JsonPropertyName("HaleTeleportToSpawn")] public bool HaleTeleportToSpawn { get; set; } = false;
    [JsonPropertyName("CanResetQueuepoints")] public bool CanResetQueuepoints { get; set; } = true;
}