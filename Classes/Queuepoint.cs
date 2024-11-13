using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

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
        var winner = 0;
        var players = Utilities.GetPlayers();
        foreach (var player in players)
        {
            if (!player.IsBot &&
                !player.IsHLTV &&
                player.Team is not CsTeam.Spectator &&
                _playerQueuepoints[player.Slot] >= _playerQueuepoints[winner])
                winner = player.Slot;
        }

        return Utilities.GetPlayerFromSlot(winner);
    }

    /// <summary>
    /// Queuepoint 계산 (OnRoundEnd)
    /// </summary>
    /// <param name="playerHaleMap">플레이어 헤일 맵</param>
    public void CalculatePlayerQueuepoints(Dictionary<int, BaseHalePlayer> playerHaleMap)
    {
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
}