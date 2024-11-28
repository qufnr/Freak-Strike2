using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using FreakStrike2.Models;

namespace FreakStrike2
{
    public partial class FreakStrike2
    {
        /// <summary>
        /// 상태 디버그 표시자
        /// </summary>
        private void DebugPrintOnGlobalGameTimerTick()
        {
            foreach (var player in Utilities.GetPlayers().Where(pl => pl.IsValid && !pl.IsBot && BaseGamePlayers[pl.Slot].DebugModeType != DebugType.None))
            {
                switch (BaseGamePlayers[player.Slot].DebugModeType)
                {
                    case DebugType.Game:
                        player.PrintToCenterAlert($"InGameStatus: {InGameStatus}\nInGameTimer: {InGameTimer?.Handle}\nInGameRoundTimer: {InGameRoundTimer?.Handle}");
                        break;
                    case DebugType.HalePlayer:
                        player.PrintToCenterAlert($".MyHale.Name: {BaseHalePlayers[player.Slot].MyHale?.Name}\n.IsHale: {BaseHalePlayers[player.Slot].IsHale}");
                        break;
                    case DebugType.HumanPlayer:
                        player.PrintToCenterAlert($".MyClass.Name: {BaseHumanPlayers[player.Slot].MyClass?.Name}\n.HasClass: {BaseHumanPlayers[player.Slot].HasClass}");
                        break;
                }
            }
        }
    }
}
