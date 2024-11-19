using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Timers;
using FreakStrike2.Models;
using FreakStrike2.Utils;
using Microsoft.Extensions.Logging;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace FreakStrike2;
public partial class FreakStrike2
{
    private GameStatus _gameStatus = GameStatus.None;
    
    private Timer? _gameTimer = null; //  게임 타이머

    private int _findInterval = 0;   //  헤일을 찾는 시간

    private void GameResetOnHotReload()
    {
        KillGameTimer();
        
        Server.ExecuteCommand("mp_restartgame 1");
    }

    /// <summary>
    /// 클라이언트가 서버에 접속했을 때 게임 시작 체크
    /// </summary>
    private void GameStartOnClientPutInServer()
    {
        var playerCount = Utilities.GetPlayers().Count;
        if (_gameStatus == GameStatus.PlayerWaiting && playerCount > 1)
            CommonUtils.GetGameRules()
                .TerminateRound(ConVarUtils.GetRoundRestartDelay(), RoundEndReason.RoundDraw);
    }
    
    /// <summary>
    /// 게임 타이머를 죽입니다. (OnMapStart, OnRoundEnd)
    /// </summary>
    private void KillGameTimer()
    {
        if (_gameTimer is not null)
        {
            _gameTimer.Kill();
            _gameTimer = null;
        }
    }

    /// <summary>
    /// 게임 타이머를 생성합니다. (OnRoundFreezeEnd, OnRoundStart)
    /// </summary>
    private void CreateGameTimer(bool isFreezeEnd = true)
    {
        if (!isFreezeEnd && ConVarUtils.GetFreezeTime() > 0)
            return;
        
        _findInterval = Config.FindInterval;

        if (CommonUtils.GetGameRules().WarmupPeriod) _gameStatus = GameStatus.Warmup;
        else if (Utilities.GetPlayers().Count <= 1) _gameStatus = GameStatus.PlayerWaiting;
        else _gameStatus = GameStatus.PlayerFinding;
        
        if (_gameTimer is null)
            _gameTimer = AddTimer(1.0f, OnGameTimerInterval, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
    }

    /// <summary>
    /// 게임 전역 타이머
    /// </summary>
    private void OnGameTimerInterval()
    {
        if (_gameStatus == GameStatus.End)
        {
            KillGameTimer();
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
            _gameStatus = GameStatus.PlayerWaiting;

        if (_findInterval < 0)
        {
            SetHalePlayerOnTimerEnd();
            KillGameTimer();
            return;
        }

        var message = string.Empty;
        switch (_gameStatus)
        {
            case GameStatus.PlayerWaiting:
                message = "다른 플레이어를 기다리고 있습니다.";
                break;
            case GameStatus.Warmup:
                message = "준비 시간이 종료되면 게임이 시작됩니다.";
                break;
            case GameStatus.PlayerFinding:
                message = $"{_findInterval}초 후 헤일 플레이어가 선택됩니다!";
                Logger.LogInformation($"Countdown: {_findInterval} seconds");
                _findInterval--;
                break;
        }
        
        if (!string.IsNullOrEmpty(message))
            ServerUtils.PrintToCenterAll(message);
    }

    /// <summary>
    /// 게임 시작이 아니고, 플레이어(victim)가 얻어 맞았을 때 피해 무효 처리
    /// </summary>
    /// <param name="victim">피해자 플레이어</param>
    /// <param name="attacker">가해자 플레이어</param>
    /// <returns>기능 설명에 대한 조건이 참인지 거짓인지 반환. (true 반환 시 EventPlayerHurt 에서 Hook 을 Handled 처리합니다.)</returns>
    private bool GameNotStartDamageIgnoreOnPlayerHurt(CCSPlayerController? victim, CCSPlayerController? attacker) => 
        _gameStatus is not GameStatus.Start && 
        victim is not null && victim.IsValid && 
        attacker is not null && attacker.IsValid;
}

