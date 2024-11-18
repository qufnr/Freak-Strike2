using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using FreakStrike2.Models;

namespace FreakStrike2.Classes;

public class BaseGamePlayer
{
    private Dictionary<int, int> _damages;
    private Dictionary<int, bool> _debugMode;

    public BaseGamePlayer()
    {
        _damages = new Dictionary<int, int>();
        _debugMode = new Dictionary<int, bool>();
    }

    public void CreateByPlayerSlot(int clientSlot)
    {
        _damages[clientSlot] = 0;
        _debugMode[clientSlot] = false;
    }

    public bool PlayerIsDebugMode(CCSPlayerController player) => _debugMode[player.Slot];
    
    public void SetPlayerDebugMode(CCSPlayerController player, bool value) => _debugMode[player.Slot] = value;
    
    public int GetPlayerDamage(CCSPlayerController player) => _damages[player.Slot];

    public void AddPlayerDamage(CCSPlayerController? victim, CCSPlayerController? attacker, BaseHalePlayer baseHalePlayer, GameStatus gameStatus, int damage)
    {
        if (victim is not null && attacker is not null &&
            gameStatus is GameStatus.Start && 
            baseHalePlayer.PlayerIsHale(victim) && 
            baseHalePlayer.PlayerIsHuman(attacker))
            _damages[attacker.Slot] += damage;
    }

    public void Clear(CCSPlayerController player)
    {
        _damages[player.Slot] = 0;
        _debugMode[player.Slot] = false;
    }

    public void Clear() => Utilities.GetPlayers().ForEach(player => Clear(player));
}