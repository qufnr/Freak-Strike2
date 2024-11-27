using CounterStrikeSharp.API;
using FreakStrike2.Classes;

namespace FreakStrike2;
public partial class FreakStrike2
{
    /// <summary>
    /// 설정 파일 불러오기
    /// </summary>
    /// <param name="gameConf">Configuration</param>
    public void OnConfigParsed(GameConfig gameConf)
    {
        if (gameConf.FindInterval < 5)
            gameConf.FindInterval = 5;

        if (gameConf.RoundTime < .5)
            gameConf.RoundTime = .5f;
        
        Server.ExecuteCommand($"mp_roundtime {gameConf.RoundTime}");
        Server.ExecuteCommand($"mp_roundtime_defuse {gameConf.RoundTime}");
        Server.ExecuteCommand($"mp_roundtime_hostage {gameConf.RoundTime}");

        Config = gameConf;
    }
}