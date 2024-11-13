using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using FreakStrike2.Classes;

namespace FreakStrike2;
public partial class FreakStrike2
{
    public GameConfig Config { get; set; }
    
    /// <summary>
    /// 설정 파일 불러오기
    /// </summary>
    /// <param name="gameConf">Configuration</param>
    public void OnConfigParsed(GameConfig gameConf)
    {
        if (gameConf.FindInterval < 10)
        {
            gameConf.FindInterval = 10;
        }

        Config = gameConf;
    }
}