using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using FreakStrike2.Exceptions;
using FreakStrike2.Models.Human;
using FreakStrike2.Utils.Helpers.Entity;

namespace FreakStrike2.Managers;

public class SaxtonHaleManager
{
    private static FreakStrike2 _instance = FreakStrike2.Instance;

    public static void OnFindSaxtonHalePlayer()
    {
        if (PlayerUtils.GetTeamAlivePlayers(FSHuman.Team) <= 0)
            return;
        
        var winner = _instance.FSPlayers
            .Select(pair => (Client: pair.Key, QueuePoint: pair.Value.QueuePoint))
            .OrderByDescending(pair => pair.QueuePoint)
            .FirstOrDefault();
        if (Utilities.GetPlayerFromSlot(winner.Client) is not CCSPlayerController player)
            player = PlayerUtils.GetRandomAlivePlayer() ?? throw new PlayerInvalidException();

        _instance.FSPlayers[player.Slot].HaleData.SetHale();
    }
}