using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace FreakStrike2;

public partial class FreakStrike2
{
    /// <summary>
    /// 플레이어 큐포인트 순위를 표시합니다.
    /// </summary>
    /// <param name="player">출력할 플레이어 객체</param>
    /// <param name="top">출력 최대 순위</param>
    private void PrintRankOfQueuePoints(CCSPlayerController? player, int top = 5)
    {
        if (top > 10)
            top = 10;
        
        int rank = 0, prevPoints = int.MinValue, count = 0;

        var rankedPlayers = PlayerQueuePoints.Where(pair =>
            //  유효한 플레이어만 필터
            {
                var pl = Utilities.GetPlayerFromSlot(pair.Key);
                return pl != null && pl.IsValid && !pl.IsBot;
            })
            //  Points 내림차순
            .OrderByDescending(pair => pair.Value.Points)
            .Select(pair => new
                { Name = Utilities.GetPlayerFromSlot(pair.Key)!.PlayerName, Points = pair.Value.Points })
            .Select(pair =>
            {
                count++;
                if (pair.Points != prevPoints)
                {
                    rank = count;
                    prevPoints = pair.Points;
                }

                return new { Rank = rank, pair.Name, pair.Points };
            })
            .ToList();
        
        if (player == null || !player.IsValid) Server.PrintToConsole($"[FS2] -- Queue Points Rank TOP {top}!");
        else player.PrintToChat($"[FS2] -- 큐포인트 순위 TOP {top}!");
        
        for (var i = 1; i <= top; i++)
        {
            var playersAtRank = rankedPlayers.Where(p => p.Rank == i).ToList();
            if (playersAtRank.Any())
            {
                foreach (var p in playersAtRank)
                {
                    if (player == null || !player.IsValid) Server.PrintToConsole($"[FS2] -- #{p.Rank} | {p.Name} | {p.Points} QP");
                    else player.PrintToChat($"[FS2] -- #{p.Rank} | {p.Name} | {p.Points} QP");
                }
            }
            else
            {
                if (player == null || !player.IsValid) Server.PrintToConsole($"[FS2] -- #-- | -- | -- QP");
                else player.PrintToChat($"[FS2] -- #-- | -- | -- QP");
            }
        }
    }
}