using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Classes;
using FreakStrike2.Models;

namespace FreakStrike2;

public partial class FreakStrike2
{
    /// <summary>
    /// 큐포인트를 배분합니다.
    /// </summary>
    private void DistributeQueuePointsOnRoundEnd()
    {
        //  라운드 종료 시 게임 상태를 End 로 바꾸기 전이라서 Start 가 아닐 경우 큐포인트 배분하지 않음
        if (InGameStatus != GameStatus.Start)
            return;
        
        foreach (var player in Utilities.GetPlayers().Where(pl => pl.IsValid))
        {
            if (BaseHalePlayers[player.Slot].IsHale)
                PlayerQueuePoints[player.Slot].Points = 0;
            else if(player.Team == CsTeam.Terrorist)
                PlayerQueuePoints[player.Slot].Points += 10;
        }
    }
    
    /// <summary>
    /// 플레이어 큐포인트 순위를 표시합니다.
    /// </summary>
    /// <param name="player">출력할 플레이어 객체</param>
    /// <param name="top">출력 최대 순위</param>
    private void PrintRankOfQueuePoints(CCSPlayerController? player, int top = 5)
    {
        if (top > 10)
            top = 10;
        
        var rankedPlayers = PlayerQueuePoints.Where(pair =>
                //  유효한 플레이어만 필터
            {
                var pl = Utilities.GetPlayerFromSlot(pair.Key);
                return pl != null && pl.IsValid;
            })
            .Select(pair => new { Name = Utilities.GetPlayerFromSlot(pair.Key)!.PlayerName, Points = pair.Value.Points })
            .OrderByDescending(pair => pair.Points)
            .ThenBy(pair => pair.Name)
            .Select((pair, index) => new { Rank = index + 1, Name = pair.Name, Points = pair.Points })
            .ToList();
        
        if (player == null || !player.IsValid) Server.PrintToConsole($"[FS2] -- Queue Points Rank TOP {top}!");
        else player.PrintToChat($"[FS2] -- 큐포인트 순위 TOP {top}!");
        
        for (var i = 0; i < top; i++)
        {
            if (i < rankedPlayers.Count)
            {
                var data = rankedPlayers[i];
                if (player == null || !player.IsValid) Server.PrintToConsole($"[FS2] -- #{data.Rank} | {data.Name} | {data.Points} QP");
                else player.PrintToChat($"[FS2] -- #{data.Rank} | {data.Name} | {data.Points} QP");
            }
            else
            {
                if (player == null || !player.IsValid) Server.PrintToConsole($"[FS2] -- #-- | -- | -- QP");
                else player.PrintToChat($"[FS2] -- #-- | -- | -- QP");
            }
        }
    }
}