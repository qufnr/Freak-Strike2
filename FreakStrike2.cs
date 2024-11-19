using CounterStrikeSharp.API.Core;
using FreakStrike2.Classes;

namespace FreakStrike2;
public partial class FreakStrike2 : BasePlugin, IPluginConfig<GameConfig>
{
    public override string ModuleName => "FreakStrike2";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "byeoruk";
    public override string ModuleDescription => "Freak Fortress 2 in Counter-Strike 2.";

    public List<BaseHale> Hales = new();            //  서버에서 설정한 헤일 List 객체
    public required BaseGamePlayer GamePlayer;      //  서버 내 플레이어의 정보
    public required BaseHalePlayer HalePlayer;      //  서버 내 플레이어의 헤일 정보
    public required Queuepoint PlayerQueuePoint;    //  서버 내 플레이어의 QueuePoint 정보
    
    public override void Load(bool hotReload)
    {
        Console.WriteLine($"[FreakStrike2] Freak-Strike 2 ({ModuleVersion}) loaded!");
        
        if (hotReload)
        {
            Unload(hotReload);
        }
        
        GameEventInitialize();
        GetHaleJsonOnLoad(hotReload);
        // ServerCommandInitialize();
    }

    public override void Unload(bool hotReload)
    {
        IgnoreRoundWinConditions();
        GameResetOnHotReload();
        GameEventDeregister();
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        ServerCommandInitialize();
    }
}