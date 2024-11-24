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
        if (victim == attacker || BaseHalePlayers[victim.Slot].IsHale && !BaseHalePlayers[attacker.Slot].IsHale)
            return;

        var victimPawn = victim.PlayerPawn.Value;
        var attackerPawn = attacker.PlayerPawn.Value;

        if (victimPawn == null || attackerPawn == null)
            return;

        var victimPosition = victimPawn.AbsOrigin ?? new Vector();
        var attackerPosition = attackerPawn.AbsOrigin ?? new Vector();

        var direction = VectorUtils.NormalizeVector(victimPosition - attackerPosition);
        
        //  TODO : 무기 설정에 따라 넉백량 처리

        // victimPawn.AbsVelocity.Add(direction * damage);
        victimPawn.Teleport(null, null, direction * damage);
    }

    private void AddDamageOnPlayerHurt(CCSPlayerController victim, CCSPlayerController attacker, int damage)
    {
        if (!victim.PawnIsAlive || victim == attacker || !BaseHalePlayers[victim.Slot].IsHale && BaseHalePlayers[attacker.Slot].IsHale)
            return;

        BaseGamePlayers[attacker.Slot].Damages += damage;
    }
}