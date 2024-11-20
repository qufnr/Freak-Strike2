using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace FreakStrike2.Utils;

public class PlayerUtils
{
    /// <summary>
    /// 플레이어가 콘솔인지 유무를 반환합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <returns>콘솔이면 true, 아니면 false 반환</returns>
    public static bool PlayerIsConsole(CCSPlayerController? player) => player is null || player.Slot <= 0;
    
    /// <summary>
    /// 살아있는 플레이어들을 반환합니다.
    /// </summary>
    /// <returns>살아있는 플레이어들</returns>
    public static List<CCSPlayerController> GetValidPawnAlivePlayers() => 
        Utilities.GetPlayers().Where(p => p.IsValid && p.PawnIsAlive).ToList();

    /// <summary>
    /// 살아있는 플레이어 중에서 무작위로 한 명을 뽑습니다.
    /// </summary>
    /// <remarks>
    /// 이는 null 을 반환할 수 있습니다.
    /// </remarks>
    /// <returns>무작위로 뽑힌 한 명의 플레이어</returns>
    public static CCSPlayerController? GetRandomAlivePlayer()
    {
        var pawnAlivePlayers = new List<CCSPlayerController>();
        GetValidPawnAlivePlayers().ForEach(player =>
        {
            if(player.IsValid && player.PawnIsAlive)
                pawnAlivePlayers.Add(player);
        });
        
        return pawnAlivePlayers.Count > 0 ? pawnAlivePlayers[new Random().Next(pawnAlivePlayers.Count)] : null;
    }
}