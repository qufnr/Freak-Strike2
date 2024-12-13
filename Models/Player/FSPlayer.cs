using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using FreakStrike2.Models.Human;
using FreakStrike2.Models.SaxtonHale;

namespace FreakStrike2.Models.Player;

/// <summary>
/// Freak-Strike 2 내 플레이어 객체
/// </summary>
public class FSPlayer
{
    private int _client;
    private CCSPlayerController? Player => Utilities.GetPlayerFromSlot(_client);
    
    public FSPlayerSaxtonHale HaleData { get; private set; }
    public FSPlayerHuman HumanData { get; private set; }

    public int QueuePoint { get; private set; } = 0;

    public FSPlayer(int client)
    {
        _client = client;
        
        HaleData = new FSPlayerSaxtonHale(_client, this);
        HumanData = new FSPlayerHuman(_client, this);
    }
}