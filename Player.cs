using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Classes;
using FreakStrike2.Models;
using FreakStrike2.Utils;

namespace FreakStrike2;
public partial class FreakStrike2
{
    private Queuepoint _queuepoint = new Queuepoint();

    /// <summary>
    /// 큐포인트 객체 생성 (OnMapStart)
    /// </summary>
    private void CreatePlayerQueuepoint()
    {
        _queuepoint = new Queuepoint();
    }
    
    /// <summary>
    /// 클라이언트 접속 시 팀 변경
    /// </summary>
    /// <param name="client">클라이언트</param>
    private void TeamChangeOnClientPutInServer(int client)
    {
        var player = Utilities.GetPlayerFromSlot(client);
        if (player is not null && player.IsValid && !player.IsHLTV)
        {
            switch (_gameStatus)
            {
                case GameStatus.PlayerWaiting:
                case GameStatus.PlayerFinding:
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

    private void KnockbackOnPlayerTakeDamage(CCSPlayerController? victim, CCSPlayerController? attacker, float damage, string weapon, int hitgroup)
    {
        if (victim is null || !victim.IsValid || 
            attacker is null || !attacker.IsValid || 
            _halePlayers[victim.Slot].IsHuman() && _halePlayers[attacker.Slot].IsHale())
            return;

        var victimPawn = victim.PlayerPawn.Value!;
        var attackerPawn = attacker.PlayerPawn.Value!;

        var victimPosition = victimPawn.AbsOrigin ?? new Vector();
        var attackerPosition = attackerPawn.AbsOrigin ?? new Vector();

        var direction = VectorUtils.NormalizeVector(victimPosition - attackerPosition);
        
        victimPawn.AbsVelocity.Add(direction * damage);
    }
}