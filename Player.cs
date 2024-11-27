using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Classes;
using FreakStrike2.Models;
using FreakStrike2.Utils;

namespace FreakStrike2;
public partial class FreakStrike2
{
    /// <summary>
    /// 클라이언트 접속 시 팀 변경
    /// </summary>
    /// <param name="client">클라이언트</param>
    private void TeamChangeOnClientPutInServer(int client)
    {
        var player = Utilities.GetPlayerFromSlot(client);
        if (player is not null && player.IsValid && !player.IsHLTV)
        {
            switch (InGameStatus)
            {
                case GameStatus.PlayerWaiting:
                case GameStatus.Ready:
                    player.ChangeTeam(CsTeam.Terrorist);
                    Server.NextFrame(() =>
                    {
                        if (!player.PawnIsAlive)
                        {
                            player.Respawn();
                        }
                    });
                    break;
                default:
                    player.ChangeTeam(CsTeam.Terrorist);
                    break;
            }
        }
    }

    /// <summary>
    /// 피해자가 가해자로 부터 피해를 입을 때 넉백을 처리합니다.
    /// </summary>
    /// <param name="victim">피해자</param>
    /// <param name="attacker">가해자</param>
    /// <param name="damage">피해량</param>
    /// <param name="weapon">무기 이름</param>
    /// <param name="hitgroup">히트 그룹</param>
    private void KnockbackOnPlayerHurt(CCSPlayerController victim, CCSPlayerController attacker, int damage, string weapon, int hitgroup)
    {
        if (victim == attacker || (!BaseHalePlayers[victim.Slot].IsHale && BaseHalePlayers[attacker.Slot].IsHale))
            return;

        var victimPawn = victim.PlayerPawn.Value;
        var attackerPawn = attacker.PlayerPawn.Value;

        if (victimPawn == null || attackerPawn == null)
            return;

        var victimPosition = victimPawn.AbsOrigin ?? new Vector();
        var attackerPosition = attackerPawn.AbsOrigin ?? new Vector();

        var direction = VectorUtils.NormalizeVector(victimPosition - attackerPosition);
        
        //  TODO : 무기 설정에 따라 넉백량 처리

        victimPawn.AbsVelocity.Add(direction * damage);
        // victimPawn.Teleport(null, null, direction * damage);
    }

    /// <summary>
    /// 헤일에게 피해를 입힐 시 피해량을 추가합니다.
    /// </summary>
    /// <param name="victim">피해자 (헤일)</param>
    /// <param name="attacker">가해자 (인간)</param>
    /// <param name="damage">피해량</param>
    private void AddDamageOnPlayerHurt(CCSPlayerController victim, CCSPlayerController attacker, int damage)
    {
        if (!victim.PawnIsAlive || victim == attacker || !BaseHalePlayers[victim.Slot].IsHale && BaseHalePlayers[attacker.Slot].IsHale)
            return;

        BaseGamePlayers[attacker.Slot].Damages += damage;
    }

    /// <summary>
    /// 플레이어에게 피해량 순위를 출력합니다.
    /// </summary>
    /// <param name="duration">표시 시간</param>
    /// <param name="top">표시 순위권</param>
    private void PrintRankOfDamagesToAll(int duration = 10, int top = 3)
    {
        if (top > 5)
            top = 5;

        var rankedPlayers = BaseGamePlayers.Where(pair =>
            {
                var pl = Utilities.GetPlayerFromSlot(pair.Key);
                return pl != null && pl.IsValid;
            })
            .Select(pair => new { Name = Utilities.GetPlayerFromSlot(pair.Key)!.PlayerName, Damages = pair.Value.Damages })
            .OrderByDescending(pair => pair.Damages)
            .ThenBy(pair => pair.Name)
            .Select((pair, index) => new { Rank = index + 1, Name = pair.Name, Damages = pair.Damages })
            .ToList();

        var text = $"가장 피해를 많이 입힌 플레이어 TOP {top}!";

        for (var i = 0; i < top; i++)
        {
            if (i < rankedPlayers.Count)
            {
                var data = rankedPlayers[i];
                text += $"<br />#{data.Rank} | {data.Name} | {data.Damages} DMG";
            }
            else
                text += $"<br />#-- | -- | -- DMG";
        }
        
        Server.NextFrame(() =>
        {
            foreach(var player in Utilities.GetPlayers().Where(player => player.IsValid && !player.IsBot))
                player.PrintToCenterHtml(text, duration);
        });
        
    }

    /// <summary>
    /// 가장 피해를 많이 입힌 플레이어 순서대로 딕셔너리 자료형으로 반환합니다.
    /// </summary>
    /// <returns>피해량 순서 플레이어 딕셔너리</returns>
    private Dictionary<string, int> GetPlayersDamageRank()
    {
        var ranking = new Dictionary<string, int>();
        foreach(var player in Utilities.GetPlayers().Where(player => player.IsValid))
            ranking.Add(player.PlayerName, BaseGamePlayers[player.Slot].Damages);
        
        return ranking.OrderByDescending(pair => pair.Value)
            .ToDictionary();
    }
}