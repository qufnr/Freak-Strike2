using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using FreakStrike2.Exceptions;
using FreakStrike2.Models.Player;
using FreakStrike2.Utils.Helpers;

namespace FreakStrike2.Models.SaxtonHale;

/// <summary>
/// 플레이어에 대한 색스턴 헤일 객체
/// </summary>
public class FSPlayerSaxtonHale
{
    private static FreakStrike2 _instance = FreakStrike2.Instance;
    
    private int _client;
    private FSPlayer _parent;
    
    private CCSPlayerController? Player => Utilities.GetPlayerFromSlot(_client);

    public FSSaxtonHale? Hale { get; private set; } = null;

    public FSPlayerSaxtonHale(int client, FSPlayer parent)
    {
        _client = client;
        _parent = parent;
    }

    /// <summary>
    /// Sets player hale index.
    /// </summary>
    /// <param name="hale">Saxton Hale Object</param>
    /// <exception cref="PlayerInvalidException">Player is Invalid!</exception>
    public void SetHale(FSSaxtonHale? hale = null)
    {
        if (Player == null || !Player.IsValid || !Player.PawnIsAlive)
            throw new PlayerInvalidException();
        
        Hale = hale == null ? CommonUtils.GetRandomInList(_instance.SaxtonHales) : hale;

        if (!Hale.SetPlayerStatus(Player))
            Hale = null;
    }
}