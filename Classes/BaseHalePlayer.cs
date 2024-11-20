using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Exceptions;
using FreakStrike2.Models;
using FreakStrike2.Utils;

using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace FreakStrike2.Classes;

public class BaseHalePlayer
{
    private BaseHale? _baseHale;
    private HaleFlags _haleFlags;
    private Timer? _weightDownCooldown;
    private Timer? _highestJumpCooldown;
    
    /// <summary>
    /// 맴버 변수(딕셔너리) 생성
    /// </summary>
    public BaseHalePlayer()
    {
        _baseHale = null;
        _haleFlags = HaleFlags.None;
        _weightDownCooldown = null;
        _highestJumpCooldown = null;
    }
    
    public BaseHalePlayer(CCSPlayerController player, BaseHale hale, bool spawnTeleport = true)
    {
        var playerPawn = player.PlayerPawn.Value;
        
        if (!player.IsValid || playerPawn is null || !playerPawn.IsValid)
            throw new PlayerNotFoundException();
        
        if (!player.PawnIsAlive)
            player.Respawn();

        if (player.Team is not CsTeam.CounterTerrorist)
            player.SwitchTeam(CsTeam.CounterTerrorist);

        if (spawnTeleport)
        {
            var infoPlayerCounterterrorists = Utilities.FindAllEntitiesByDesignerName<CInfoPlayerCounterterrorist>("info_player_counterterrorist")
                .ToList();
            
            var entities = new List<CInfoPlayerCounterterrorist>();
            foreach (var infoPlayerCounterterrorist in infoPlayerCounterterrorists)
                if(infoPlayerCounterterrorist.IsValid)
                    entities.Add(infoPlayerCounterterrorist);
            
            if (entities.Count() > 0)
            {
                var candidate = CommonUtils.GetRandomInList(entities);
                if (candidate.IsValid && candidate.AbsOrigin is not null)
                {
                    var spawnOrigin = new Vector()
                    {
                        X = candidate.AbsOrigin.X,
                        Y = candidate.AbsOrigin.Y + 1.0f,
                        Z = candidate.AbsOrigin.Z
                    };
                    player.Teleport(spawnOrigin);
                }
            }
        }

        _baseHale = hale;
        _haleFlags = HaleFlags.Hale;

        var playerCount = Utilities.GetPlayers().Count - 1;
        
        playerPawn.MaxHealth = Convert.ToInt32(Math.Round(hale.MaxHealth * (hale.HealthMultiplier * playerCount)));
        playerPawn.Health = playerPawn.MaxHealth;
        playerPawn.ArmorValue = Convert.ToInt32(Math.Round(hale.Armor * (hale.ArmorMultiplier * playerCount)));
        player.PawnHasHelmet = true;
        playerPawn.VelocityModifier *= hale.Laggedmovement; //  태깅 걸리거나 뭐 어떤짓 하면 원래 속도로 바뀜... 
        playerPawn.GravityScale = hale.Gravity; //  사다리 타면 초기화 됨 (소스1 때랑 동일한 현상)
        
        player.RemoveWeapons();
        player.GiveNamedItem(CsItem.Knife);
        // WeaponUtils.ForceRemoveWeapons(player, false, true);
        // if (!WeaponUtils.HasWeaponByDesignerName(player, "weapon_knife"))
        //     player.GiveNamedItem(CsItem.Knife);
        
        Server.NextFrame(() =>
        {
            if (!string.IsNullOrEmpty(hale.Model))
                playerPawn.SetModel(hale.Model);
        
            if (!string.IsNullOrEmpty(hale.Viewmodel))
            {
                var weapon = WeaponUtils.FindPlayerWeapon(player, "weapon_knife");
                if (weapon is not null)
                    WeaponUtils.UpdatePlayerWeaponModel(player, weapon, hale.Viewmodel, true);
            }
        });
    }

    /// <summary>
    /// 플레이어가 헤일인지 유무를 반환합니다.
    /// </summary>
    /// <returns>
    /// 헤일일 경우 true 아니면 flase 반환
    /// </returns>
    public bool IsHale() => _haleFlags is HaleFlags.Hale && _baseHale is not null;

    /// <summary>
    /// 플레이어가 인간인지 유무를 반환합니다.
    /// </summary>
    /// <returns>인간일 경우 true 아니면 false 반환</returns>
    public bool IsHuman() => !IsHale();
    
    /// <summary>
    /// 플레이어를 헤일에서 제거합니다.
    /// </summary>
    /// <param name="clientSlot">플레이어 슬롯</param>
    /// <param name="gameStatus">게임 상태 (매개변수로 넘기지 않을 시 GameStatus.None 으로 판별)</param>
    public void Clear(int clientSlot, GameStatus? gameStatus = GameStatus.None)
    {
        if (IsHuman())
            return;

        var player = Utilities.GetPlayerFromSlot(clientSlot);
        
        _baseHale = null;
        _haleFlags = HaleFlags.None;
        
        if (player is not null && player.IsValid)
        {
            player.CommitSuicide(false, true);
            if (gameStatus is GameStatus.Start && 
                player.Team is CsTeam.CounterTerrorist && 
                PlayerUtils.GetTeamAlivePlayers(player.Team) <= 0)
            {
                CommonUtils.GetGameRules()
                    .TerminateRound(ConVarUtils.GetRoundRestartDelay(), RoundEndReason.TerroristsWin);
            }
        }
    }
}