using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using FreakStrike2.Models;
using FreakStrike2.Utils;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace FreakStrike2.Classes;

public class BaseGamePlayer
{
    public int Damages { get; set; } = 0;                   //  플레이어가 입힌 피해량
    public float StunTime { get; private set; } = 0f;       //  스턴 지속 시간
    public Timer? StunTimer { get; private set; } = null;           //  스턴 타이머
    public bool DebugMode { get; set; } = false;            //  디버그 모드 활성화 여부

    public BaseGamePlayer()
    {
        Damages = 0;
        StunTime = 0;
        DebugMode = false;
        KillStunTimer();
    }

    /// <summary>
    /// 플레이어 스턴 여부
    /// </summary>
    /// <returns>스턴일 경우 true, 아니면 false 반환</returns>
    public bool IsStunned() => Server.CurrentTime <= StunTime;
    
    /// <summary>
    /// 스턴 상태 활성화
    /// </summary>
    /// <param name="player">플레이어 Pawn 객체</param>
    /// <param name="stunTimer">스턴 타이머 (0.1 Repeat)</param>
    /// <param name="stunTime">스턴 시간</param>
    public void ActivateStun(CCSPlayerPawn player, Timer stunTimer, float stunTime = 10f)
    {
        StunTime = StunTime != 0 ? (Server.CurrentTime - StunTime) + stunTime : Server.CurrentTime + stunTime;
        
        player.SetMoveType(MoveType_t.MOVETYPE_NONE);

        KillStunTimer();
        StunTimer = stunTimer;
    }

    /// <summary>
    /// 스턴 상태 비활성화
    /// </summary>
    /// <param name="player">플레이어 Pawn 객체</param>
    public void DeactivateStun(CCSPlayerPawn? player)
    {
        if (StunTime > 0)
        {
            StunTime = 0;
            KillStunTimer();
            if(player != null && player.IsValid && player.MoveType == MoveType_t.MOVETYPE_NONE)
                player.SetMoveType(MoveType_t.MOVETYPE_WALK);
        }
    }

    /// <summary>
    /// 스턴 타이머 콜백
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="gameStatus">현재 게임 상태</param>
    /// <param name="debug">디버그 여부</param>
    /// <returns>타이머 콜백</returns>
    public Action StunTimerCallback(CCSPlayerController player, GameStatus gameStatus, bool debug = false) => () =>
    {
        var playerPawn = player.PlayerPawn.Value;
        
        //  스턴 비활성화 조건
        if (gameStatus != GameStatus.Start || 
            !player.PawnIsAlive || 
            Server.CurrentTime > StunTime)
        {
            DeactivateStun(playerPawn);
            return;
        }

        if (playerPawn != null && playerPawn.IsValid)
        {
            //  땅을 밟고 있고, MoveType 이 NONE 이 아닐 경우 MoveType 을 NONE 으로 만든다.
            if ((playerPawn.Flags & (1 << 0)) != 0 && playerPawn.MoveType != MoveType_t.MOVETYPE_NONE)
                playerPawn.SetMoveType(MoveType_t.MOVETYPE_NONE);
        }
        
        if (debug && player.IsValid)
            player.PrintToCenterAlert($"Stun Time: {StunTime - Server.CurrentTime:F2}");
    };

    public void Reset(CCSPlayerController player)
    {
        Damages = 0;
        
        if (player.IsValid)
        {
            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn != null && playerPawn.IsValid)
            {
                DeactivateStun(playerPawn);
            }
        }
    }

    public void Reset(int slot)
    {
        var player = Utilities.GetPlayerFromSlot(slot);
        if(player != null)
            Reset(player);
    }

    /// <summary>
    /// 플레이어의 디버그 모드를 토글합니다.
    /// </summary>
    /// <returns>디버그 모드 여부</returns>
    public bool ToggleDebugMode()
    {
        DebugMode = !DebugMode;
        return DebugMode;
    }

    /// <summary>
    /// 스턴 타이머 죽이기
    /// </summary>
    private void KillStunTimer()
    {
        if (StunTimer != null)
        {
            StunTimer.Kill();
            StunTimer = null;
        }
    }
}