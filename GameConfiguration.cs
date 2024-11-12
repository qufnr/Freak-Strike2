using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace FreakStrike2;
public class GameConfiguration : BasePluginConfig
{
    [JsonPropertyName("FindInterval")] public int FindInterval { get; set; } = 20;
    [JsonPropertyName("HaleTeleportToSpawn")] public bool HaleTeleportToSpawn { get; set; } = false;
    [JsonPropertyName("CanResetQueuepoints")] public bool CanResetQueuepoints { get; set; } = true;
}

public partial class FreakStrike2
{
    public GameConfiguration Config { get; set; }
    
    /**
     * 설정 파일 불러오기
     */
    public void OnConfigParsed(GameConfiguration gameConf)
    {
        if (gameConf.FindInterval < 10)
        {
            gameConf.FindInterval = 10;
        }

        Config = gameConf;
    }
}