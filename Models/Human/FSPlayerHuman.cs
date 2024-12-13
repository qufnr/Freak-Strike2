using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Timers;
using FreakStrike2.Models.Player;
using FreakStrike2.Utils.Helpers;

namespace FreakStrike2.Models.Human;

public class FSPlayerHuman
{
    private static FreakStrike2 _instance = FreakStrike2.Instance;
    
    private int _client;
    private FSPlayer _parent;

    public FSHuman? Human { get; private set; } = null;
    
    public FSPlayerHuman(int client, FSPlayer parent)
    {
        _client = client;
        _parent = parent;
        Human = CommonUtils.GetRandomInList(_instance.Humans);
    }

    public void Spawn()
    {
        _instance.AddTimer(0.1f, () =>
        {
            if (_parent.HaleData.Hale != null)
                return;
            
            
        }, TimerFlags.STOP_ON_MAPCHANGE);
    }
}