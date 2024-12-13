using CounterStrikeSharp.API.Core;
using FreakStrike2.Models.Player;
using FreakStrike2.Utils.Helpers.Server;

namespace FreakStrike2.Managers;

public class EventManager
{
    private static FreakStrike2 _instance = FreakStrike2.Instance;
    
    public static void Register()
    {
        _instance.RegisterListener<Listeners.OnMapStart>(OnMapStart);
        _instance.RegisterListener<Listeners.OnClientPutInServer>(OnClientPutInServer);
        
        _instance.RegisterEventHandler<EventRoundStart>(OnRoundStart);
    }

    public static void Remove()
    {
        _instance.RemoveListener<Listeners.OnMapStart>(OnMapStart);
        _instance.RemoveListener<Listeners.OnClientPutInServer>(OnClientPutInServer);
        
        _instance.DeregisterEventHandler<EventRoundStart>(OnRoundStart);
    }

    private static void OnMapStart(string mapName)
    {
        _instance.GameRules = ServerUtils.GameRules;
        ServerConVarManager.Execute();
    }

    private static void OnClientPutInServer(int client)
    {
        _instance.FSPlayers[client] = new FSPlayer(client);
    }

    private static HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        return HookResult.Continue;
    }
}