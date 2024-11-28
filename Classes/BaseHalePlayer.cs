using System.Reflection;
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
    public bool WeightDownReady { get; set; } = true; //  내려찍기 준비
    public float WeightDownCooldown { get; set; } = 0f;         //  내려찍기 쿨다운
    public Timer? WeightDownCooldownTimer { get; set; } = null; //  내려찍기 쿨다운 타이머
    
    public bool SuperJumpReady { get; set; } = true;            //  높이 점프 준비
    public bool DoSuperJumpHold { get; set; } = false;          //  높이 점프 홀드
    public float SuperJumpHoldTicks { get; set; } = 0f;         //  높이 점프 홀드 틱
    public float SuperJumpHoldStartTicks { get; set; } = 0f;    //  높이 점프 홀드 시작 틱
    public float SuperJumpCooldown { get; set; } = 5f;
    public Timer? SuperJumpCooldownTimer { get; set; } = null;

    public bool IsHale => MyHale != null;

    public BaseHale? MyHale { get; private set; } = null;
    public HaleType Type { get; private set; } = HaleType.None;

    /// <summary>
    /// 맴버 변수(딕셔너리) 생성
    /// </summary>
    public BaseHalePlayer() => Remove();
    
    /// <summary>
    /// 헤일 플레이어 생성
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="hale">헤일</param>
    /// <exception cref="PlayerNotFoundException">플레이어가 유효하지 않으면 던집니다.</exception>
    public BaseHalePlayer(CCSPlayerController player, BaseHale hale)
    {
        var playerPawn = player.PlayerPawn.Value;
        
        if (!player.IsValid || playerPawn == null || !playerPawn.IsValid)
            throw new PlayerNotFoundException();
        
        if (!player.PawnIsAlive)
            player.Respawn();

        if (player.Team != CsTeam.CounterTerrorist)
            player.SwitchTeam(CsTeam.CounterTerrorist);

        MyHale = hale;
        Type = HaleType.Hale;

        SuperJumpCooldown = hale.SuperJumpCooldown;
        WeightDownCooldown = hale.WeightDownCooldown;
        
        //  스폰으로 텔레포트
        player.TeleportToSpawnPoint(CsTeam.CounterTerrorist);
        
        MyHale.SetPlayer(player);
    }

    /// <summary>
    /// 높이 점프 쿨타임 타이머 콜백
    /// </summary>
    /// <remarks>
    /// 이 타이머 콜백은 무조건 0.1초 동안 반복 타이머로 호출해야합니다.
    /// </remarks>
    /// <param name="player">플레이어 객체</param>
    /// <param name="gameStatus">게임 상태</param>
    /// <param name="debug">디버깅 여부</param>
    /// <returns>타이머 콜백</returns>
    public Action SuperJumpCooldownCallback(CCSPlayerController player, GameStatus gameStatus = GameStatus.None, bool debug = false) => () =>
    {
        var slot = player.Slot;
        
        if (!player.IsValid || !IsHale)
        {
            Remove(slot, gameStatus);
            return;
        }
        
        if (debug)
            player.PrintToCenterAlert($"Super Jump Cooldown: {SuperJumpCooldown:F1}");
        
        SuperJumpCooldown -= 0.1f;

        if (SuperJumpCooldown <= 0.0f)
        {
            if (debug)
                player.PrintToChat("[FS2 Debugger] Super Jump is Ready!");
            
            SuperJumpCooldown = 0.0f;
            SuperJumpReady = true;
            SuperJumpCooldownTimer!.Kill();
        }
    };

    /// <summary>
    /// 내려찍기 쿨타임 타이머 콜백
    /// </summary>
    /// <remarks>
    /// 이 타이머 콜백은 무조건 0.1초 동안 반복 타이머로 호출해야합니다.
    /// </remarks>
    /// <param name="player">플레이어 객체</param>
    /// <param name="originGravityScale">이전 플레이어 중력 크기</param>
    /// <param name="gameStatus">게임 상태</param>
    /// <param name="debug">디버그 여부</param>
    /// <returns>타이머 콜백</returns>
    public Action WeightDownCooldownCallback(CCSPlayerController player, float originGravityScale, GameStatus gameStatus = GameStatus.None, bool debug = false) => () =>
    {
        var slot = player.Slot;

        if (!player.IsValid || !IsHale)
        {
            Remove(slot, gameStatus);
            return;
        }
        
        var playerPawn = player.PlayerPawn.Value;
        //  땅에 닿을 때 원래 중력 크기로 되돌리기
        if (playerPawn != null && playerPawn.IsValid && player.PawnIsAlive && (playerPawn.Flags & (1 << 0)) != 0)
            playerPawn.GravityScale = originGravityScale;

        if (debug)
            player.PrintToCenterAlert($"Weight Down Cooldown: {WeightDownCooldown:F1}");

        WeightDownCooldown -= 0.1f;

        if (WeightDownCooldown <= 0.0f)
        {
            if (debug)
                player.PrintToChat("[FS2 Debugger] Weight Down is Ready!");
            
            WeightDownCooldown = 0.0f;
            WeightDownReady = true;
            WeightDownCooldownTimer!.Kill();
        }
    };

    /// <summary>
    /// 멤버 변수 초기화
    /// </summary>
    /// <returns>초기화 전에 헤일이었다면 true, 아니면 false 반환</returns>
    public bool Remove()
    {
        var isHale = IsHale;
        
        MyHale = null;
        Type = HaleType.None;
        
        WeightDownReady = true;
        WeightDownCooldown = 5f;
        if (WeightDownCooldownTimer != null) WeightDownCooldownTimer.Kill();
        WeightDownCooldownTimer = null;
        
        DoSuperJumpHold = true;
        SuperJumpHoldTicks = 0f;
        SuperJumpHoldStartTicks = 0f;
        SuperJumpReady = true;
        SuperJumpCooldown = 5f;
        if (SuperJumpCooldownTimer != null) SuperJumpCooldownTimer.Kill();
        SuperJumpCooldownTimer = null;
        
        return isHale;
    }
    
    /// <summary>
    /// 플레이어를 헤일에서 제거합니다.
    /// </summary>
    /// <param name="clientSlot">플레이어 슬롯</param>
    /// <param name="gameStatus">게임 상태 (매개변수로 넘기지 않을 시 GameStatus.None 으로 판별)</param>
    public void Remove(int clientSlot, GameStatus? gameStatus = GameStatus.None)
    { 
        var player = Utilities.GetPlayerFromSlot(clientSlot);

        //  제거 대상의 플레이어가 헤일이었고 진행중인 게임이 있을 경우, 라운드를 강제로 종료시킨다.
        if (Remove() && player is not null && player.IsValid)
        {
            if (player.PawnIsAlive)
                player.CommitSuicide(false, true);
            
            if (gameStatus == GameStatus.Start && 
                player.Team == CsTeam.CounterTerrorist && 
                PlayerUtils.GetTeamAlivePlayers(player.Team) <= 0)
            {
                ServerUtils.GameRules
                    .TerminateRound(ConVarUtils.GetRoundRestartDelay(), RoundEndReason.TerroristsWin);
            }
        }
    }
}