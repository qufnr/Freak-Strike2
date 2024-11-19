using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Models;

namespace FreakStrike2.Classes;

public class BaseQueuePoint
{
    public int Points { get; set; } = 0;

    public BaseQueuePoint()
    {
        Points = 0;
    }

    /// <summary>
    /// 플레이어 중에 가장 많은 Queuepoint 를 소유하고 있는 플레이어 객체 찾기
    /// </summary>
    /// <param name="playerQueuePoints">QueuePoint 딕셔너리</param>
    /// <returns>Queuepoint 가 가장 높은 플레이어 객체</returns>
    public static CCSPlayerController? GetPlayerWithMostQueuepoints(Dictionary<int, BaseQueuePoint> playerQueuePoints)
    {
        var playerQueuePoint = playerQueuePoints
            .OrderByDescending(pair => pair.Value.Points)
            .FirstOrDefault();
        return Utilities.GetPlayerFromSlot(playerQueuePoint.Key);
    }

    /// <summary>
    /// 라운드가 종료되었을 때 플레이어 큐포인트를 계산합니다.
    /// </summary>
    /// <param name="playerQueuePoints">플레이어 큐포인트 딕셔너리</param>
    /// <param name="baseHalePlayers">헤일 플레이어 딕셔너리</param>
    /// <param name="gameStatus">현재 게임 상태</param>
    public static void Calculate(Dictionary<int, BaseQueuePoint> playerQueuePoints, Dictionary<int, BaseHalePlayer> baseHalePlayers, GameStatus gameStatus)
    {
        if (gameStatus is not GameStatus.Start)
            return;
        
        Utilities.GetPlayers().ForEach(player =>
        {
            if (player.IsValid)
            {
                if (player.IsBot || player.IsHLTV || baseHalePlayers[player.Slot].IsHale())
                    playerQueuePoints[player.Slot].Points = 0;
                else if (player.Team is not CsTeam.Spectator)
                    playerQueuePoints[player.Slot].Points += 10;
            }
        });
    }

    /// <summary>
    /// PlayerQueuePoints 에 대해 랭크를 매깁니다.
    /// </summary>
    /// <param name="playerQueuePoints">플레이어 큐포인트 딕셔너리</param>
    /// <param name="top">TOP 몇 까지 보여줄지 (Optional, Default: 5)</param>
    /// <returns>큐포인트 랭크</returns>
    public static Dictionary<int, int> GetRank(Dictionary<int, BaseQueuePoint> playerQueuePoints, int top = 5)
    {
        var results = playerQueuePoints
            .Where(queuepoint =>
            {
                var player = Utilities.GetPlayerFromSlot(queuepoint.Key);
                return player is not null && player.IsValid && !player.IsBot && !player.IsHLTV;
            })
            .OrderByDescending(pair => pair.Value.Points)
            .Take(top);
        return results.ToDictionary(entry => entry.Key, entry => entry.Value.Points);
    }
}