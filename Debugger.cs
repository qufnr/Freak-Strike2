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
                if (player.IsValid && !player.IsBot && _gamePlayer.PlayerIsDebugMode(player))
                    player.PrintToCenterHtml($"Game Status: {_gameStatus}<br/>Game Timer: {_gameTimer?.Handle}", 1);
        }
    }
}
