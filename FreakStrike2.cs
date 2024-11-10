using CounterStrikeSharp.API.Core;

namespace FreakStrike2;
public partial class FreakStrike2 : BasePlugin
{
    public override string ModuleName => "FreakStrike2";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "byeoruk";
    public override string ModuleDescription => "FF2 in CS2.";

    public override void Load(bool hotReload)
    {
        Console.WriteLine("[FreakStrike2] Freak-Strike 2 loaded!");

        GameEventInitialize();
        // ServerCommandInitialize();

        if (hotReload)
        {
            ConfigurationInitialize();
            IgnoreRoundWinConditions();
        }
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        ServerCommandInitialize();
    }
}