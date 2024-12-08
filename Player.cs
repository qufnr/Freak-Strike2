using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Classes;
using FreakStrike2.Models;
using FreakStrike2.Utils.Helpers.Entity;
using FreakStrike2.Utils.Helpers.Math;
using FreakStrike2.Utils.Helpers.Server;

namespace FreakStrike2;
public partial class FreakStrike2
{

    /// <summary>
    /// 피해자가 가해자로 부터 피해를 입을 때 넉백을 처리합니다.
    /// </summary>
    private void PlayerKnockbackOnTakeDamage(CCSPlayerPawn victim, CCSPlayerPawn attacker, BaseWeapon? baseWeapon, float damage)
    {
        var victimPosition = victim.AbsOrigin ?? new Vector();
        var attackerPosition = attacker.AbsOrigin ?? new Vector();

        var direction = VectorUtils.NormalizeVector(victimPosition - attackerPosition);

        var vectorScale = direction * damage;

        if (baseWeapon != null)
        {
            vectorScale *= baseWeapon.KnockbackScale;

            var distance = VectorUtils.GetDistance(victimPosition, attackerPosition);

            if (distance >= 0 && baseWeapon.KnockbackMaximumDistance > distance)
                vectorScale *= (baseWeapon.KnockbackMaximumDistance - distance) / 10f;
        }
        
        victim.AbsVelocity.Add(vectorScale);
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
    /// 플레이어에게 피해량 순위를 출력합니다. (라운드 종료 처리 전에 호출함. GameStatus.End 로 되기 전)
    /// </summary>
    private void PrintDamageScoreboard()
    {
        if (InGameStatus != GameStatus.Start || PlayerUtils.FindValidPlayers().Count <= 1)
            return;

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

        var text = $"가장 피해를 많이 입힌 플레이어 TOP {Config.DamageRankRows}!";

        for (var i = 0; i < Config.DamageRankRows; i++)
        {
            if (i < rankedPlayers.Count)
            {
                var data = rankedPlayers[i];
                text += $"<br />#{data.Rank} | {data.Name} | {data.Damages} DMG";
            }
            else
                text += $"<br />#-- | -- | -- DMG";
        }

        var duration = Server.CurrentTime + ConVarUtils.GetRoundRestartDelay() - 1;

        if (DamageRankingTimer != null)
            DamageRankingTimer.Kill();

        DamageRankingTimer = AddTimer(0.1f, () =>
        {
            if (Server.CurrentTime > duration)
            {
                DamageRankingTimer!.Kill();
                DamageRankingTimer = null;
                return;
            }

            foreach (var player in PlayerUtils.FindPlayersWithoutFakeClient())
                player.PrintToCenterHtml(text, 1);
        }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
    }
}