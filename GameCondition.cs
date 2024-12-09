using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Timers;
using FreakStrike2.Models;
using FreakStrike2.Utils.Helpers.Entity;
using FreakStrike2.Utils.Helpers.Server;

namespace FreakStrike2;
public partial class FreakStrike2
{
    /// <summary>
    /// 핫리로드 시 게임 재설정
    /// </summary>
    private void GameResetOnHotReload()
    {
        KillInGameTimer();
        KillInGameGlobalTimer();
        
        Server.ExecuteCommand("mp_restartgame 1");
    }

    /// <summary>
    /// 클라이언트가 서버에 접속했을 때 게임 시작 체크
    /// </summary>
    private void GameStartOnClientPutInServer()
    {
        var playerCount = Utilities.GetPlayers().Count;
        if (InGameStatus == GameStatus.PlayerWaiting && playerCount > 1)
            ServerUtils.GameRules
                .TerminateRound(ConVarUtils.GetRoundRestartDelay(), RoundEndReason.RoundDraw);
    }
    
    /// <summary>
    /// 게임 타이머를 죽입니다. (OnMapStart, OnRoundEnd)
    /// </summary>
    private void KillInGameTimer()
    {
        if (InGameTimer is not null)
            InGameTimer.Kill();
        InGameTimer = null;
    }

    /// <summary>
    /// 전역 타이머를 죽입니다. (OnMapEnd, Unload, HotReload)
    /// </summary>
    private void KillInGameGlobalTimer()
    {
        if (InGameGlobalTimer != null)
            InGameGlobalTimer.Kill();
        InGameGlobalTimer = null;
    }

    /// <summary>
    /// 전역 타이머를 생성합니다.
    /// </summary>
    private void CreateInGameGlobalTimer()
    {
        if (InGameGlobalTimer == null)
            InGameGlobalTimer = AddTimer(0.1f, OnTickGlobalGameTimer, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
    }

    /// <summary>
    /// 게임 타이머를 생성합니다. (OnRoundStart, OnRoundFreezeEnd)
    /// </summary>
    private void CreateInGameTimer()
    {
        var gameRule = ServerUtils.GameRules;
        
        ReadyInterval = Config.ReadyInterval;

        if (gameRule.WarmupPeriod) InGameStatus = GameStatus.Warmup;
        else if (Utilities.GetPlayers().Count <= 1) InGameStatus = GameStatus.PlayerWaiting;
        else InGameStatus = GameStatus.Ready;

        if (InGameTimer == null && !gameRule.FreezePeriod)
            InGameTimer = AddTimer(1.0f, OnGameTimerInterval, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
    }

    /// <summary>
    /// 게임 전역 타이머
    /// </summary>
    private void OnGameTimerInterval()
    {
        if (InGameStatus == GameStatus.End)
        {
            KillInGameTimer();
            return;
        }

        if (Hales.Count == 0)
        {
            Server.PrintToChatAll("[FS] 서버에 플레이 가능한 헤일이 없습니다. 'configs/plugins/FreakStrike2/playable_hales.json' 파일에서 헤일 정보를 추가해주세요.");
            return;
        }
        
        var players = Utilities.GetPlayers();
        
        //  도중에 플레이어가 나갔을 때, 서버 인원이 1명 이하일 경우
        if (players.Count <= 1)
            InGameStatus = GameStatus.PlayerWaiting;

        if (ReadyInterval <= 0)
        {
            HalePlayerAllActiveOnTimerEnd();    //  헤일 활동 시작!!!
            KillInGameTimer();
            return;
        }

        switch (InGameStatus)
        {
            case GameStatus.PlayerWaiting:
                ServerUtils.PrintToCenterAll("다른 플레이어를 기다리고 있습니다.");
                break;
            case GameStatus.Warmup:
                ServerUtils.PrintToCenterAll("준비 시간이 종료되면 게임이 시작됩니다.");
                break;
            case GameStatus.Ready:
                foreach (var player in PlayerUtils.FindPlayersWithoutFakeClient())
                {
                    if (BaseGamePlayers[player.Slot].DebugModeType != DebugType.None)
                        player.PrintToChat($"[FS2 Debugger] Countdown: {ReadyInterval}");
                    if (BaseHalePlayers[player.Slot].IsHale)
                        player.PrintToCenter($"{ReadyInterval}초 후 {BaseHalePlayers[player.Slot].MyHale!.Name} 헤일로 플레이하게 될 것입니다!");
                    else
                        player.PrintToCenter($"헤일이 활동할 때 까지 {ReadyInterval}초 남았습니다.");
                }
                ReadyInterval--;
                break;
        }
    }
    
    /// <summary>
    /// 전역 타이머 콜백
    /// </summary>
    private void OnTickGlobalGameTimer()
    {
        DebugPrintOnGlobalGameTimerTick();
        
        //  플레이어에게 호출
        foreach (var player in PlayerUtils.FindValidPlayers())
        {
            PrintHudTextStatusOnGlobalTimerTick(player);
        }
    }
}

