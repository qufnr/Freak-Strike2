using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Classes;
using FreakStrike2.Models;

namespace FreakStrike2
{
    public partial class FreakStrike2
    {
        private CQueuepoint _queuepoint = new CQueuepoint();

        /// <summary>
        /// 큐포인트 객체 생성 (OnMapStart)
        /// </summary>
        private void CreatePlayerQueuepoint()
        {
            _queuepoint = new CQueuepoint();
        }
        
        /// <summary>
        /// 클라이언트 접속 시 팀 변경
        /// </summary>
        /// <param name="client">클라이언트</param>
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
