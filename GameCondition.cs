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

        private void GameResetOnHotReload()
        {
            KillGameTimer();
            _queuepoint.Clear();
            
            Server.ExecuteCommand("mp_restartgame 1");
            
            var players = Utilities.GetPlayers();
            foreach (var player in players)
            {
                if (player.IsValid)
                {
                    if (_halePlayers[player.Slot].IsHale())
                        _halePlayers[player.Slot].Remove(_gameStatus, player.Slot);
                    
                    if (player.PawnIsAlive)
                        player.CommitSuicide(false, true);
                }
            }

            _halePlayers.Clear();
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

            if (_hales.Count == 0)
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
                KillGameTimer();
                return;
            }
            
            foreach (var player in players)
            {
                if (player.IsValid && !player.IsBot && !player.IsHLTV)
                {
                    switch (_gameStatus)
                    {
                        case GameStatus.PlayerWaiting:
                            player.PrintToCenter("다른 플레이어를 기다리고 있습니다.");
                            break;
                        case GameStatus.Warmup:
                            player.PrintToCenter("준비 시간이 종료되면 게임이 시작됩니다.");
                            break;
                        case GameStatus.PlayerFinding:
                            player.PrintToCenter($"{_findInterval}초 후 헤일이 등장합니다.");
                            _findInterval--;
                            break;
                    }
                }
            }
        }
    }
}

