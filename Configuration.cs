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
        if (gameConf.ReadyInterval < 5) gameConf.ReadyInterval = 5;
        if (gameConf.DamageRankRows > 5) gameConf.DamageRankRows = 5;
        if (gameConf.QueuePointRankRows > 10) gameConf.QueuePointRankRows = 10;

        Config = gameConf;
    }
}