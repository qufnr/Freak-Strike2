using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Models;

namespace FreakStrike2
{
    public partial class FreakStrike2
    {
        private void TeamChangeOnClientPutInServer(int client)
        {
            var player = Utilities.GetPlayerFromSlot(client);
            if (player is not null && player.IsValid && !player.IsHLTV)
            {
                switch (_gameStatus)
                {
                    case GameStatus.PlayerWaiting:
                    case GameStatus.PlayerFinding:
                        player.ChangeTeam(CsTeam.Terrorist);
                        Server.NextFrame(() =>
                        {
                            if (!player.PawnIsAlive)
                            {
                                player.Respawn();
                            }
                        });
                        break;
                    default:
                        player.ChangeTeam(CsTeam.Terrorist);
                        break;
                }
            }
        }
    }
}
