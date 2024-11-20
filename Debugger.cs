using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace FreakStrike2
{
    public partial class FreakStrike2
    {
        /// <summary>
        /// 디버그 표시자
        /// </summary>
        private void DebugPrintGameCondition()
        {
            var players = Utilities.GetPlayers();
            foreach (var player in players)
                if (player.IsValid && !player.IsBot && BaseGamePlayers[player.Slot].DebugMode)
                    player.PrintToCenterHtml($"Game Status: {InGameStatus}<br/>Game Timer: {InGameTimer?.Handle}", 1);
        }
    }
}
