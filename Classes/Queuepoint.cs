using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Models;

namespace FreakStrike2.Classes;

public class Queuepoint
{
    private Dictionary<int, int> _playerQueuepoints;

    public Queuepoint()
    {
        _playerQueuepoints = new Dictionary<int, int>();
    }

    public void Clear()
    {
        var players = Utilities.GetPlayers();
        foreach (var player in players)
        {
            _playerQueuepoints[player.Slot] = 0;
        }
    }

    /// <summary>
    /// Queuepoint Setter
    /// </summary>
    /// <param name="slot">클라이언트 슬롯</param>
    /// <param name="qp">설정할 Queuepoint</param>
    public void SetPlayerQueuepoint(int slot, int qp)
    {
        _playerQueuepoints[slot] = qp;
    }

    /// <summary>
    /// Queuepoint Getter
    /// </summary>
    /// <param name="slot">클라이언트 슬롯</param>
    /// <returns>플레이어의 Queuepoint</returns>
    public int GetPlayerQueuepoint(int slot)
    {
        return _playerQueuepoints[slot];
    }

    /// <summary>
    /// 플레이어 중에 가장 많은 Queuepoint 를 소유하고 있는 플레이어 객체 찾기
    /// </summary>
    /// <returns>Queuepoint 가 가장 높은 플레이어 객체</returns>
    public CCSPlayerController? GetPlayerWithMostQueuepoints()
    {
        var winner = _playerQueuepoints.OrderBy(pq => pq.Value).Reverse().FirstOrDefault();
        return Utilities.GetPlayerFromSlot(winner.Key);
    }

    /// <summary>
    /// Queuepoint 계산 (OnRoundEnd)
    /// </summary>
    /// <param name="playerHaleMap">플레이어 헤일 맵</param>
    /// <param name="gameStatus">게임 상태</param>
    public void Calculate(Dictionary<int, BaseHalePlayer> playerHaleMap, GameStatus gameStatus)
    {
        if (gameStatus is not GameStatus.Start)
            return;
        
        var players = Utilities.GetPlayers();
        foreach (var player in players)
        {
            if (player.IsValid)
            {
                if (playerHaleMap[player.Slot].IsHale())
                {
                    if (player.IsBot || player.IsHLTV)
                    {
                        _playerQueuepoints[player.Slot] = 0;
                    }
                    else
                    {
                        _playerQueuepoints[player.Slot] = -_playerQueuepoints[player.Slot];
                    }
                }
                else
                {
                    if (!player.IsBot && player.Team is not CsTeam.Spectator)
                    {
                        _playerQueuepoints[player.Slot] += 10;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Queuepoint 순위를 계산합니다.
    /// </summary>
    /// <remarks>
    /// 전체 플레이어 중에서 봇은 제외하고 계산됩니다.
    /// </remarks>
    /// <returns>Queuepoint 순위 Map</returns>
    public Dictionary<CCSPlayerController, int>? GetRank()
    {
        if (_playerQueuepoints.Count > 0)
        {
            return _playerQueuepoints
                .Where(pqp =>
                {
                    var player = Utilities.GetPlayerFromSlot(pqp.Key);
                    return player is not null && player.IsValid && !player.IsBot;
                })
                .OrderBy(pqp => pqp.Value)
                .Reverse()
                .ToDictionary(slot => Utilities.GetPlayerFromSlot(slot.Value)!, qp => qp.Value);
        }

        return null;
    }
}