using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Classes;
using FreakStrike2.Models;
using FreakStrike2.Utils.Classes;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace FreakStrike2;
public partial class FreakStrike2 : BasePlugin, IPluginConfig<GameConfig>
{
    internal static FreakStrike2 Instance { get; private set; } = new();
    
    public override string ModuleName => "FreakStrike2";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "byeoruk";
    public override string ModuleDescription => "Freak Fortress 2 in Counter-Strike 2.";

    public static string PluginConfigDirectory = "csgo\\addons\\counterstrikesharp\\configs\\plugins\\FreakStrike2\\";
    public static string HaleConfigFilename = "playable_hales.json";
    public static string HumanConfigFilename = "playable_humans.json";

    public static string MessagePrefix = "[FS2] ";
    public static string LogMessagePrefix = "[FreakStrike2] ";

    public GameConfig Config { get; set; } = new(); //  플러그인 콘피그
    
    public List<BaseHale> Hales = new();            //  서버에서 설정한 헤일 List 객체
    public List<BaseHuman> Humans = new();          //  서버에서 설정한 인간 List 객체
        
    public GameStatus InGameStatus = GameStatus.None;   //  게임 상태
    public Timer? InGameTimer = null;                   //  게임 타이머
    public Timer? InGameGlobalTimer = null;             //  전역 타이머 (.1초 마다 계속 실행)
    public Timer? InGameRoundTimer = null;              //  라운드 타이머
    public int FindInterval = 0;                        //  헤일을 찾는 시간
    public Timer? DamageRankingTimer = null;            //  피해량 순위 표시 타이머

    public Dictionary<int, BaseGamePlayer> BaseGamePlayers = new(64);   //  서버 내 플레이어 정보
    public Dictionary<int, BaseHalePlayer> BaseHalePlayers = new(64);   //  서버 내 플레이어 헤일 정보
    public Dictionary<int, BaseHumanPlayer> BaseHumanPlayers = new(64); //  서버 내 플레이어 인간 클래스 정보
    public Dictionary<int, BaseQueuePoint> PlayerQueuePoints = new(64); //  서버 내 플레이어 큐포인트 정보

    public Dictionary<int, HudText> HudTexts = new(64);                 //  허드 텍스트
    
    public override void Load(bool hotReload)
    {
        Instance = this;
        
        Console.WriteLine($"[FreakStrike2] Freak-Strike 2 ({ModuleVersion}) loaded!");

        if (hotReload)
        {
            Unload(hotReload);
            CreateInGameGlobalTimer();
        }
        
        GameEventRegister();
        HookVirtualFunctions();
        RegisterCommands();
        
        ReadHaleJsonOnLoad(hotReload);
        ReadHumanJsonOnLoad(hotReload);
        // ServerCommandInitialize();
    }

    public override void Unload(bool hotReload)
    {
        IgnoreRoundWinConditions();
        GameResetOnHotReload();
        GameEventDeregister();
        RemoveCommands();
        UnhookVirtualFunctions();
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        ServerCommandInitialize();
    }
}