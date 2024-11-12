using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Models;
using FreakStrike2.Utils;

namespace FreakStrike2.Classes;

public class CBaseHalePlayer
{
    private CBaseHale? Hale { get; set; } = null;
    private HaleFlags Flags { get; set; } = HaleFlags.None;

    public CBaseHalePlayer()
    {
        Hale = null;
        Flags = HaleFlags.None;
    }
    
    public CBaseHalePlayer(CCSPlayerController player, CBaseHale hale, bool respawn = true)
    {
        if (!player.PawnIsAlive)
        {
            return;
        }
        
        Hale = hale;
        Flags = HaleFlags.Hale;

        player.MaxHealth = Convert.ToInt32(Math.Round(hale.MaxHealth * hale.HealthMultiplier));
        player.Health = player.MaxHealth;
        player.PawnArmor = hale.Armor;
        player.PawnHasHelmet = true;
        player.Speed = hale.Laggedmovement;
        player.GravityScale = hale.Gravity;
        
        player.RemoveWeapons();
        player.GiveNamedItem(CsItem.Knife);
        
        
        
        Server.NextFrame(() =>
        {
            player.ExecuteClientCommand("use weapon_knife");
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
                    .TerminateRound(CommonUtils.GetRoundRestartDelay(), RoundEndReason.TerroristsWin);
            }
        }
    }
}