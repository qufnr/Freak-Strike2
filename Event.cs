using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using FreakStrike2.Models;
using Microsoft.Extensions.Logging;

namespace FreakStrike2
{
    public partial class FreakStrike2
    {
        private void GameEventInitialize()
        {
            RegisterEventHandler<EventRoundStart>(OnRoundStart);
            RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
            
            RegisterListener<Listeners.OnMapStart>(OnMapStart);
            RegisterListener<Listeners.OnClientPutInServer>(OnClientPutInServer);
            RegisterListener<Listeners.OnClientDisconnect>(OnClientDisconnect);
            RegisterListener<Listeners.OnTick>(OnTick);
        }

        private void OnMapStart(string mapName)
        {
            KillGameTimer();
        }

        private void OnClientPutInServer(int client)
        {
            GameStartOnClientPutInServer();
            DebugDisableOnClientPutInServer(client);
            TeamChangeOnClientPutInServer(client);
            // var player = Utilities.GetPlayerFromSlot(client);
            // if (player is not null && player.IsValid && !player.IsBot && !player.IsHLTV)
            // {
            //     Logger.LogInformation($"[FreakStrike2] Player {player.PlayerName} ({player.AuthorizedSteamID?.SteamId64}) is put in server with {player.Slot}.");
            //
            // }
        }

        private void OnClientDisconnect(int client)
        {
            
        }

        private void OnTick()
        {
            DebugPrintGameCondition();
        }

        private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo eventInfo)
        {
            RemoveEntities();
            CreateGameTimer();
            
            return HookResult.Continue;
        }

        private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo eventInfo)
        {
            if (_gameStatus is GameStatus.PlayerWaiting)
                return HookResult.Stop;
            
            _gameStatus = GameStatus.End;
            
            return HookResult.Continue;
        }
    }
}
