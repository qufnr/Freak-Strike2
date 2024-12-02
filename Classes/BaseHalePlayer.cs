using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Exceptions;
using FreakStrike2.Models;
using FreakStrike2.Utils.Helpers.Entity;
using FreakStrike2.Utils.Helpers.Server;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace FreakStrike2.Classes;

public class BaseHalePlayer
{
    private int _client;

    private CCSPlayerController? Player => Utilities.GetPlayerFromSlot(_client);
    
    public bool WeightDownReady { get; set; } = true; //  내려찍기 준비
    public float WeightDownCooldown { get; private set; } = 0f;         //  내려찍기 쿨다운
    public Timer? WeightDownCooldownTimer { get; private set; } = null; //  내려찍기 쿨다운 타이머
    
    public bool SuperJumpReady { get; set; } = true;            //  높이 점프 준비
    public bool DoSuperJumpHold { get; set; } = false;          //  높이 점프 홀드
    public float SuperJumpHoldTicks { get; set; } = 0f;         //  높이 점프 홀드 틱
    public float SuperJumpHoldStartTicks { get; set; } = 0f;    //  높이 점프 홀드 시작 틱
    public float SuperJumpCooldown { get; private set; } = 0f;
    public Timer? SuperJumpCooldownTimer { get; private set; } = null;
    
    public float Rage { get; set; } = 0f;                       //  분노 게이지
    public Timer? RageChargeTimer { get; private set; } = null; //  분노 게이지 충전 타이머

    public bool IsHale => MyHale != null && Type != HaleType.None;

    public BaseHale? MyHale { get; private set; } = null;
    public HaleType Type { get; private set; } = HaleType.None;

    /// <summary>
    /// 맴버 변수(딕셔너리) 생성
    /// </summary>
    public BaseHalePlayer(int client)
    {
        _client = client;
        Reset();
    }

    public void SetHaleState(BaseHale hale)
    {
        if (Player == null || !Player.IsValid)
            return;
        
        var playerPawn = Player.PlayerPawn.Value;
        
        if (playerPawn == null || !playerPawn.IsValid)
            throw new PlayerNotFoundException();

        Reset();
        MyHale = hale;
        Type = HaleType.Hale;
        
        //  스폰으로 텔레포트
        Player.TeleportToSpawnPoint((CsTeam) Fs2Team.Hale);

        FreakStrike2.Instance.AddTimer(0.1f, () =>
        {
            if (Player.Team != (CsTeam) Fs2Team.Hale)
                Player.SwitchTeam((CsTeam) Fs2Team.Hale);
            
            if (!Player.PawnIsAlive)
                Player.Respawn();

            if (MyHale.CanUseRage)
                RageChargeTimer = FreakStrike2.Instance.AddTimer(MyHale.RageChargeInterval, () =>
                {
                    if (Player.PawnIsAlive)
                    {
                        var range = MyHale.GetRageRangeByCharge();
                        Rage = Rage + range > BaseHale.MaxRage ? BaseHale.MaxRage : Rage + range;
                    }
                }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);

        }, TimerFlags.STOP_ON_MAPCHANGE);

        FreakStrike2.Instance.AddTimer(0.15f, () => MyHale.SetPlayer(Player), TimerFlags.STOP_ON_MAPCHANGE);
    }

    /// <summary>
    /// 슈퍼 점프 쿨타임 생성
    /// </summary>
    public void CreateWeightDownCooldown()
    {
        if (Player == null || !Player.IsValid || !IsHale)
            return;
        
        var playerPawn = Player.PlayerPawn.Value;
        if (playerPawn == null)
            return;
        
        var instance = FreakStrike2.Instance;
        var originGravityScale = playerPawn.GravityScale;
        
        WeightDownCooldown = MyHale!.WeightDownCooldown;
        WeightDownCooldownTimer = instance.AddTimer(0.1f, () =>
        {
            if (Player == null || !Player.IsValid || !IsHale)
            {
                Reset(true);
                return;
            }

            //  땅에 닿을 때 원래 중력 크기로 되돌리기
            if (playerPawn != null && playerPawn.IsValid && Player.PawnIsAlive && (playerPawn.Flags & (1 << 0)) != 0)
                playerPawn.GravityScale = originGravityScale;

            if (instance.BaseGamePlayers[_client].DebugModeType == DebugType.HalePlayer)
                Player.PrintToCenterAlert($"Weight Down Cooldown: {WeightDownCooldown:F1}");

            WeightDownCooldown -= 0.1f;

            if (WeightDownCooldown <= 0.0f)
            {
                if (instance.BaseGamePlayers[_client].DebugModeType == DebugType.HalePlayer)
                    Player.PrintToChat("[FS2 Debugger] Weight Down is Ready!");

                WeightDownCooldown = 0.0f;
                WeightDownReady = true;
                WeightDownCooldownTimer!.Kill();
            }
        }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
    }

    /// <summary>
    /// 슈퍼 점프 쿨타임 생성
    /// </summary>
    public void CreateSuperJumpCooldown()
    {
        var instance = FreakStrike2.Instance;
        
        SuperJumpCooldown = MyHale!.SuperJumpCooldown;
        SuperJumpCooldownTimer = instance.AddTimer(0.1f, () =>
        {
            if (Player == null || !Player.IsValid || !IsHale)
            {
                Reset(true);
                return;
            }
    
            if (instance.BaseGamePlayers[_client].DebugModeType == DebugType.HalePlayer)
                Player.PrintToCenterAlert($"Super Jump Cooldown: {SuperJumpCooldown:F1}");
    
            SuperJumpCooldown -= 0.1f;

            if (SuperJumpCooldown <= 0.0f)
            {
                if (instance.BaseGamePlayers[_client].DebugModeType == DebugType.HalePlayer)
                    Player.PrintToChat("[FS2 Debugger] Super Jump is Ready!");
        
                SuperJumpCooldown = 0.0f;
                SuperJumpReady = true;
                SuperJumpCooldownTimer!.Kill();
            }
        }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
    }

    /// <summary>
    /// 멤버 변수 초기화
    /// </summary>
    /// <returns>초기화 전에 헤일이었다면 true, 아니면 false 반환</returns>
    public void Reset(bool destoryClient = false)
    {
        MyHale = null;
        Type = HaleType.None;
        
        WeightDownReady = true;
        WeightDownCooldown = 0f;
        if (WeightDownCooldownTimer != null) WeightDownCooldownTimer.Kill();
        WeightDownCooldownTimer = null;
        
        DoSuperJumpHold = true;
        SuperJumpHoldTicks = 0f;
        SuperJumpHoldStartTicks = 0f;
        SuperJumpReady = true;
        SuperJumpCooldown = 0f;
        if (SuperJumpCooldownTimer != null) SuperJumpCooldownTimer.Kill();
        SuperJumpCooldownTimer = null;

        Rage = 0;
        if (RageChargeTimer != null) RageChargeTimer.Kill();
        RageChargeTimer = null;

        if (destoryClient)
        {
            if (Player != null)
            {
                if (Player.PawnIsAlive)
                    Player.CommitSuicide(false, true);
                
                if (FreakStrike2.Instance.InGameStatus == GameStatus.Start && 
                    Player.Team == (CsTeam) Fs2Team.Hale && 
                    PlayerUtils.GetTeamPlayers(Player.Team) <= 0)
                    ServerUtils.GameRules.TerminateRound(ConVarUtils.GetRoundRestartDelay(), RoundEndReason.TerroristsWin);
                
                Player.SwitchTeam((CsTeam) Fs2Team.Human);
            }
        }
    }
}