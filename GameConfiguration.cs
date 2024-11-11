using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace FreakStrike2
{
    public class GameConfiguration : BasePluginConfig
    {
        [JsonPropertyName("findInterval")] 
        public int ConVarFindInterval { get; set; } = 20;

        [JsonPropertyName("foundHaleTeleport")]
        public bool ConVarFoundHaleTeleport { get; set; } = false;
    }

    public partial class FreakStrike2
    {
        public GameConfiguration Config { get; set; }
        
        /**
         * 설정 파일 불러오기
         */
        public void OnConfigParsed(GameConfiguration gameConf)
        {
            if (gameConf.ConVarFindInterval < 10)
            {
                gameConf.ConVarFindInterval = 10;
            }

            Config = gameConf;
        }
    }
}
