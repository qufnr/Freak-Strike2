using CounterStrikeSharp.API.Core;
using FreakStrike2.Classes;

namespace FreakStrike2;
public partial class FreakStrike2 : BasePlugin, IPluginConfig<GameConfig>
{
    public override string ModuleName => "FreakStrike2";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "byeoruk";
    public override string ModuleDescription => "Freak Fortress 2 in Counter-Strike 2.";

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