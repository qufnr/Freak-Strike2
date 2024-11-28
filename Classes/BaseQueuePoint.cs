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
    /// 플레이어 중에 가장 많은 큐포인트를 소유하고 있는 플레이어 객체 찾기
    /// </summary>
    /// <returns>큐포인트가 가장 높은 플레이어 객체</returns>
    public static CCSPlayerController? GetPlayerWithMostQueuePoints()
    {
        var playerQueuePoint = FreakStrike2.Instance.PlayerQueuePoints
            .OrderByDescending(pair => pair.Value.Points)
            .FirstOrDefault();
        return Utilities.GetPlayerFromSlot(playerQueuePoint.Key);
    }
}