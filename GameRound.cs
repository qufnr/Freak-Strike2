using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Models;
using FreakStrike2.Utils.Helpers.Entity;
using FreakStrike2.Utils.Helpers.Server;

namespace FreakStrike2;

public partial class FreakStrike2
{
    private void CreateRoundTimerOnRoundStart()
    {
        KillInGameRoundTimer();
        
        var gameRules = ServerUtils.GameRules;

        if (!gameRules.WarmupPeriod && InGameStatus != GameStatus.Warmup)
        {
            var roundTime = ServerUtils.GetRaminingRoundTime() - 0.5f;

            Console.WriteLine($"ROUND TIME {roundTime}");
            
            InGameRoundTimer = AddTimer(roundTime, () =>
            {
                var humanCount = PlayerUtils.GetTeamAlivePlayers((CsTeam) Fs2Team.Human);
                gameRules.TerminateRound(ConVarUtils.GetRoundRestartDelay(), humanCount > 0 ? RoundEndReason.TerroristsWin : RoundEndReason.CTsWin);

                KillInGameTimer();
            });
        }
    }

    /// <summary>
    /// 라운드 타이머 죽이기
    /// </summary>
    private void KillInGameRoundTimer()
    {
        if (InGameRoundTimer != null)
            InGameRoundTimer.Kill();
        
        InGameRoundTimer = null;
    }
}