using CounterStrikeSharp.API.Core;
using FreakStrike2.Models;

namespace FreakStrike2;
public partial class FreakStrike2 : BasePlugin, IPluginConfig<Config>
{
    internal static FreakStrike2 Instance { get; private set; } = new();
    
    public override string ModuleName => "FreakStrike2";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "byeoruk";
    public override string ModuleDescription => "Freak Fortress 2 in Counter-Strike 2.";

    public static string PluginConfigDirectory = "csgo\\addons\\counterstrikesharp\\configs\\plugins\\FreakStrike2\\";
    public static string HaleConfigFilename = "playable_hales.json";
    public static string HumanConfigFilename = "playable_humans.json";
    public static string WeaponFilename = "weapons.json";

    public Config Config { get; set; } = new(); //  플러그인 콘피그
    
    public override void Load(bool hotReload)
    {
        Instance = this;
    }

    public override void Unload(bool hotReload)
    {
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
    }

    public void OnConfigParsed(Config config)
    {
        if (config.ReadyInterval < 5) config.ReadyInterval = 5;
        if (config.DamageRankRows > 5) config.DamageRankRows = 5;
        if (config.QueuePointRankRows > 10) config.QueuePointRankRows = 10;

        Config = config;
    }
}