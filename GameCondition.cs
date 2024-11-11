using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Timers;
using FreakStrike2.Models;
using FreakStrike2.Utils;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace FreakStrike2
{
    public partial class FreakStrike2
    {
        private GameStatus _gameStatus = GameStatus.None;
        
        private Timer? _gameTimer = null; //  게임 타이머

        private int _findInterval = 0;   //  헤일을 찾는 시간

        /**
         * 클라이언트가 서버에 접속했을 때 게임 시작 체크
         */
        private void GameStartOnClientPutInServer()
        {
            var playerCount = Utilities.GetPlayers().Count;
            if (_gameStatus == GameStatus.PlayerWaiting && playerCount > 1)
            {
                CommonUtils
                    .GetGameRules()
                    .TerminateRound(CommonUtils.GetRoundRestartDelay(), RoundEndReason.RoundDraw);
            }
        }
        
        /**
         * 게임 타이머를 죽입니다. (OnMapStart, OnRoundEnd)
         */
        private void KillGameTimer()
        {
            if (_gameTimer is not null)
            {
                _gameTimer.Kill();
                _gameTimer = null;
            }
        }

        /**
         * 게임 타이머를 생성합니다. (OnMapStart, OnRoundStart)
         */
        private void CreateGameTimer()
        {
            _findInterval = Config.ConVarFindInterval;

            if (Utilities.GetPlayers().Count <= 1)
            {
                _gameStatus = GameStatus.PlayerWaiting;
            }
            else
            {
                _gameStatus = GameStatus.PlayerFinding;
            }
            
            if (_gameTimer is null)
            {
                _gameTimer = AddTimer(1.0f, OnGameTimerInterval, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
            }
        }

        /**
         * 게임 전역 타이머
         */
        private void OnGameTimerInterval()
        {
            if (_gameStatus == GameStatus.End)
            {
                KillGameTimer();
                return;
            }
            
            var players = Utilities.GetPlayers();
            
            //  도중에 플레이어가 나갔을 때, 서버 인원이 1명 이하일 경우
            if (players.Count <= 1)
            {
                _gameStatus = GameStatus.PlayerWaiting;
            }

            if (_findInterval < 0)
            {
                KillGameTimer();
                return;
            }
            
            foreach (var player in players)
            {
                if (player.IsValid && !player.IsBot && !player.IsHLTV)
                {
                    if (_gameStatus == GameStatus.PlayerWaiting)
                    {
                        player.PrintToCenter("다른 플레이어를 기다리고 있습니다.");
                    }
                    else if (_gameStatus == GameStatus.PlayerFinding)
                    {
                        player.PrintToCenter($"{_findInterval}초 후 헤일이 등장합니다.");
                        _findInterval--;
                    }
                }
            }
        }
    }
}

