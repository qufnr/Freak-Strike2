using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace FreakStrike2
{
    public partial class FreakStrike2
    {
        public Dictionary<int, bool> DebugPlayers { get; set; } = new Dictionary<int, bool>();

        /**
         * 접속 시 디버그 비활성화
         */
        private void DebugDisableOnClientPutInServer(int client)
        {
            DebugPlayers[client] = false;
        }
        
        /**
         * 디버그 표시자
         */
        private void DebugPrintGameCondition()
        {
            var players = Utilities.GetPlayers();
            foreach (var player in players)
            {
                if (player.IsValid && !player.IsBot && DebugPlayers[player.Slot])
                {
                    player.PrintToCenterHtml($"Game Status: {_gameStatus}<br/>Game Timer: {_gameTimer?.Handle}", 1);
                }
            }
        }
    }
}
