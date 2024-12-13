using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using FreakStrike2.Managers;
using FreakStrike2.Models;
using FreakStrike2.Models.Human;
using FreakStrike2.Models.Player;
using FreakStrike2.Models.SaxtonHale;

namespace FreakStrike2;
public class FreakStrike2 : BasePlugin, IPluginConfig<Config>
{
    internal static FreakStrike2 Instance { get; private set; } = new();
    
    public override string ModuleName => "FreakStrike2";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "byeoruk";
    public override string ModuleDescription => "Freak Fortress 2 in Counter-Strike 2.";

    public static string PluginConfigDirectory = "csgo\\addons\\counterstrikesharp\\configs\\plugins\\FreakStrike2\\";

    public Config Config { get; set; } = new(); //  플러그인 콘피그

    public List<FSSaxtonHale> SaxtonHales = new(72);        //  서버 내 정의된 색스턴 헤일
    public List<FSHuman> Humans = new(72);                  //  서버 내 정의된 인간 진영 클래스

    public CCSGameRules? GameRules;
    
    public Dictionary<int, FSPlayer> FSPlayers = new(0);    //  플레이어 데이터 풀
    
    public override void Load(bool hotReload)
    {
        Instance = this;

        FSPlayers = new(Server.MaxPlayers);
    }

    public override void Unload(bool hotReload)
    {
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        ServerConVarManager.Execute();
    }

    public void OnConfigParsed(Config config)
    {
        if (config.ReadyInterval < 5) config.ReadyInterval = 5;
        if (config.DamageRankRows > 5) config.DamageRankRows = 5;
        if (config.QueuePointRankRows > 10) config.QueuePointRankRows = 10;

        Config = config;
    }
}