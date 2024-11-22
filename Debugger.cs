using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace FreakStrike2
{
    public partial class FreakStrike2
    {
        /// <summary>
        /// 게임 상태 디버그 표시자
        /// </summary>
        private void DebugPrintGameCondition() => FindDebugingPlayers()
            .ForEach(player =>
                player.PrintToCenter($"Game Status: {InGameStatus}\nGame Timer: {InGameTimer?.Handle}"));

        /// <summary>
        /// 카운트 다운 표시자
        /// </summary>
        private void DebugPrintGameConditionCountdown() => FindDebugingPlayers()
            .ForEach(player => player.PrintToChat($"[FS2 Debugger] Countdown: {FindInterval}"));
        
        /// <summary>
        /// 서버에서 디버그 모드를 사용중인 플레이어를 찾습니다.
        /// </summary>
        /// <returns>디버그 모드가 활성화된 플레이어 객체 목록</returns>
        private List<CCSPlayerController> FindDebugingPlayers() => Utilities.GetPlayers().Where(player => player.IsValid && !player.IsBot && BaseGamePlayers[player.Slot].DebugMode).ToList();
    }
}
