using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using FreakStrike2.Classes;
using FreakStrike2.Models;

using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace FreakStrike2;
public partial class FreakStrike2 : BasePlugin, IPluginConfig<GameConfig>
{
    public override string ModuleName => "FreakStrike2";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "byeoruk";
    public override string ModuleDescription => "Freak Fortress 2 in Counter-Strike 2.";

    public required GameConfig Config { get; set; }              //  플러그인 콘피그
    
    public List<BaseHale> Hales = new();            //  서버에서 설정한 헤일 List 객체
    
    public GameStatus InGameStatus = GameStatus.None;   //  게임 상태
    public Timer? InGameTimer = null;                   //  게임 타이머
    public int FindInterval = 0;                        //  헤일을 찾는 시간

    public required Dictionary<int, BaseGamePlayer> BaseGamePlayers;    //  서버 내 플레이어 정보
    public required Dictionary<int, BaseHalePlayer> BaseHalePlayers;    //  서버 내 플레이어 헤일 정보
    public required Dictionary<int, BaseQueuePoint> PlayerQueuePoints;  //  서버 내 플레이어 큐포인트 정보
    
    public override void Load(bool hotReload)
    {
        Console.WriteLine($"[FreakStrike2] Freak-Strike 2 ({ModuleVersion}) loaded!");
        
        if (hotReload)
        {
            Unload(hotReload);
        }
        
        GameEventRegister();
        HookVirtualFunctions();
        GetHaleJsonOnLoad(hotReload);
        // ServerCommandInitialize();
    }

    public override void Unload(bool hotReload)
    {
        IgnoreRoundWinConditions();
        GameResetOnHotReload();
        GameEventDeregister();
        UnhookVirtualFunctions();
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        ServerCommandInitialize();
    }
}