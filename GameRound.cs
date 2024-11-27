using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Models;
using FreakStrike2.Utils;

namespace FreakStrike2;

public partial class FreakStrike2
{
    private void CreateRoundTimerOnRoundStart()
    {
        KillInGameRoundTimer();
        
        var gameRules = CommonUtils.GetGameRules();

        if (!gameRules.WarmupPeriod && InGameStatus != GameStatus.Warmup)
        {
            var roundTime = Config.RoundTime;
            var convarRoundTime = ConVarUtils.GetRoundTime();
            if (convarRoundTime > 0 && !convarRoundTime.Equals(roundTime))
                roundTime = convarRoundTime;

            var freezeTime = ConVarUtils.GetFreezeTime();
            if (freezeTime > 0)
                roundTime += freezeTime - 1f;

            Console.WriteLine($"ROUND TIME {roundTime}");
            
            InGameRoundTimer = AddTimer((roundTime - .5f) * 60f, () =>
            {
                var humanCount = PlayerUtils.GetTeamAlivePlayers(CsTeam.Terrorist);
                gameRules.TerminateRound(freezeTime, humanCount > 0 ? RoundEndReason.TerroristsWin : RoundEndReason.CTsWin);

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