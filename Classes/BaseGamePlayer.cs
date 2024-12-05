using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using FreakStrike2.Exceptions;
using FreakStrike2.Models;
using FreakStrike2.Utils.Helpers.Entity;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace FreakStrike2.Classes;

public class BaseGamePlayer
{
    private int _client;
    private CCSPlayerController Player => Utilities.GetPlayerFromSlot(_client) ?? throw new PlayerNotFoundException();
    
    public int Damages { get; set; } = 0;                           //  플레이어가 입힌 피해량

    public int ModifiedNextAttack { get; set; } = 0;                //  다음 공격 틱
    
    public float StunTime { get; private set; } = 0f;               //  스턴 지속 시간
    public Timer? StunTimer { get; private set; } = null;           //  스턴 타이머
    
    public DebugType DebugModeType { get; set; } = DebugType.None;  //  디버그 모드 활성화 여부

    public BaseGamePlayer(int client)
    {
        _client = client;
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
    /// <param name="stunTime">스턴 시간</param>
    public void ActivateStun(float stunTime = 10f)
    {
        var playerPawn = Player.PlayerPawn.Value;
        if (playerPawn == null || !playerPawn.IsValid)
            return;

        var instance = FreakStrike2.Instance;
        
        StunTime = StunTime != 0 ? (Server.CurrentTime - StunTime) + stunTime : Server.CurrentTime + stunTime;
        
        Player.SetMoveType(MoveType_t.MOVETYPE_NONE);

        KillStunTimer();
        StunTimer = FreakStrike2.Instance.AddTimer(0.1f, () =>
        {
            //  스턴 비활성화 조건
            if (instance.InGameStatus != GameStatus.Start || 
                !Player.PawnIsAlive || 
                Server.CurrentTime > StunTime)
            {
                DeactivateStun();
                return;
            }

            //  땅을 밟고 있고, MoveType 이 NONE 이 아닐 경우 MoveType 을 NONE 으로 만든다.
            if (playerPawn.IsValid && (playerPawn.Flags & (1 << 0)) != 0 && playerPawn.MoveType != MoveType_t.MOVETYPE_NONE)
                Player.SetMoveType(MoveType_t.MOVETYPE_NONE);
        
            if (instance.BaseGamePlayers[_client].DebugModeType == DebugType.HumanPlayer && Player.IsValid)
                Player.PrintToCenterAlert($"Stun Time: {StunTime - Server.CurrentTime:F2}");
        }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
    }

    /// <summary>
    /// 스턴 상태 비활성화
    /// </summary>
    public void DeactivateStun()
    {
        if (!IsStunned())
            return;
        
        StunTime = 0;
        KillStunTimer();
        if(Player.IsValid && Player.MoveType == MoveType_t.MOVETYPE_NONE)
            Player.SetMoveType(MoveType_t.MOVETYPE_WALK);
    }

    public void Reset()
    {
        Damages = 0;
        ModifiedNextAttack = 0;
        DeactivateStun();
    }

    /// <summary>
    /// 스턴 타이머 죽이기
    /// </summary>
    private void KillStunTimer()
    {
        if (StunTimer != null)
            StunTimer.Kill();
        
        StunTimer = null;
    }
}