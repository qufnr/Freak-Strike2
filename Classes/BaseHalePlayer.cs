using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Models;
using FreakStrike2.Utils;

namespace FreakStrike2.Classes;

public class BaseHalePlayer
{
    private BaseHale? Hale { get; set; } = null;
    private HaleFlags Flags { get; set; } = HaleFlags.None;

    public BaseHalePlayer()
    {
        Hale = null;
        Flags = HaleFlags.None;
    }
    
    public BaseHalePlayer(CCSPlayerController player, BaseHale hale, bool spawnTeleport = true)
    {
        if (!player.PawnIsAlive)
        {
            return;
        }

        if (player.Team is not CsTeam.CounterTerrorist)
        {
            player.SwitchTeam(CsTeam.CounterTerrorist);
        }

        if (spawnTeleport)
        {
            var entities = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_counterterrorist")
                .ToArray();
            var entity = entities[CommonUtils.GetRandomInt(0, entities.Length - 1)];
            if (entity.IsValid)
            {
                var origin = entity.AbsOrigin;
                origin!.Y += 1.0f;
                player.Teleport(origin);
            }
        }
        
        Hale = hale;
        Flags = HaleFlags.Hale;

        var playerCount = Utilities.GetPlayers().Count - 1;
        
        player.PlayerPawn.Value!.MaxHealth = Convert.ToInt32(Math.Round(hale.MaxHealth * (hale.HealthMultiplier * playerCount)));
        player.PlayerPawn.Value!.Health = player.PlayerPawn.Value!.MaxHealth;
        player.PlayerPawn.Value!.ArmorValue = Convert.ToInt32(Math.Round(hale.Armor * (hale.ArmorMultiplier * playerCount)));
        player.PawnHasHelmet = true;
        player.PlayerPawn.Value!.Speed = hale.Laggedmovement;
        player.PlayerPawn.Value!.GravityScale = hale.Gravity;
        
        WeaponUtils.ForceRemoveWeapons(player, false, true);
        if (!WeaponUtils.HasWeaponByDesignerName(player, "knife", true))
        {
            player.GiveNamedItem(CsItem.Knife);
        }
        
        Server.NextFrame(() =>
        {
            player.ExecuteClientCommand("use weapon_knife");
            
            if (!string.IsNullOrEmpty(hale.Model))
            {
                player.PlayerPawn.Value!.SetModel(hale.Model);
            }

            if (!string.IsNullOrEmpty(hale.Viewmodel))
            {
                var weapon = WeaponUtils.FindPlayerWeapon(player, "weapon_knife");
                if (weapon is not null)
                {
                    WeaponUtils.UpdatePlayerWeaponModel(player, weapon, hale.Viewmodel, true);
                }
            }
        });
    }

    /// <summary>
    /// 플레이어가 헤일인지 유무를 반환합니다.
    /// </summary>
    /// <returns>
    /// 헤일일 경우 true 아니면 flase 반환
    /// </returns>
    public bool IsHale()
    {
        return Hale is not null && Flags is HaleFlags.Hale;
    }

    /// <summary>
    /// 플레이어를 헤일에서 제거합니다.
    /// </summary>
    /// <param name="gameStatus">게임 상태</param>
    /// <param name="slot">Player Slot</param>
    public void Remove(GameStatus gameStatus, int slot)
    {
        if (!IsHale()) return;
        
        Hale = null;
        Flags = HaleFlags.None;
        
        var player = Utilities.GetPlayerFromSlot(slot);
        if (player is not null && player.IsValid)
        {
            player.CommitSuicide(false, true);
            if (gameStatus is GameStatus.Start && 
                player.Team is CsTeam.CounterTerrorist && 
                CommonUtils.GetTeamAlivePlayers(player.Team) <= 0)
            {
                CommonUtils.GetGameRules()
                    .TerminateRound(ConVarUtils.GetRoundRestartDelay(), RoundEndReason.TerroristsWin);
            }
        }
    }
}