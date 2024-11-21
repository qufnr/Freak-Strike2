using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace FreakStrike2
{
    public partial class FreakStrike2
    {
        /// <summary>
        /// 디버그 표시자
        /// </summary>
        private void DebugPrintGameCondition() =>
            Utilities.GetPlayers()
                .Where(player => player.IsValid && !player.IsBot && BaseGamePlayers[player.Slot].DebugMode)
                .ToList()
                .ForEach(player =>
                    player.PrintToCenter($"Game Status: {InGameStatus}\nGame Timer: {InGameTimer?.Handle}"));
    }
}
