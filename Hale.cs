using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Classes;
using FreakStrike2.Utils;
using Microsoft.Extensions.Logging;

namespace FreakStrike2;
public partial class FreakStrike2
{
    public static string PluginConfigDirectory = "csgo\\addons\\counterstrikesharp\\configs\\plugins\\FreakStrike2\\";
    public static string HaleConfigFilename = "playable_hales.json";
    
    private List<BaseHale> _hales = new List<BaseHale>();
    private BaseHalePlayer _halePlayer = new BaseHalePlayer();
    
    /// <summary>
    /// 헤일 설정 파일을 읽어옵니다.
    /// </summary>
    /// <param name="hotReload">핫리로드 유무</param>
    private void GetHaleJsonOnLoad(bool hotReload)
    {
        var directory = Path.Combine(Server.GameDirectory, PluginConfigDirectory);
        if (!Directory.Exists(directory))
        {
            Logger.LogError($"Couldn't find Plugin Configuration directory. [Directory Path: {directory}]");
            return;
        }
        
        var jsonFile = Path.Combine(directory, HaleConfigFilename);
        if (!File.Exists(jsonFile))
        {
            Logger.LogError($"Couldn't find Hale Configuration file. [Path: {jsonFile}]");
            return;
        }

        if (hotReload)
            _hales.Clear();

        _hales = BaseHale.GetHalesFromJson(File.ReadAllText(jsonFile));
    }

    /// <summary>
    /// 헤일 모델 프리캐싱
    /// </summary>
    /// <param name="manifest">리소스 매니페스트</param>
    private void PrecacheHaleModels(ResourceManifest manifest)
    {
        foreach (var hale in _hales)
        {
            if (!string.IsNullOrEmpty(hale.Model) || !string.IsNullOrEmpty(hale.ArmsModel) || !string.IsNullOrEmpty(hale.Viewmodel))
            {
                if (!string.IsNullOrEmpty(hale.Model)) manifest.AddResource(hale.Model);
                if (!string.IsNullOrEmpty(hale.ArmsModel)) manifest.AddResource(hale.ArmsModel);
                if (!string.IsNullOrEmpty(hale.Viewmodel)) manifest.AddResource(hale.Viewmodel);
                Logger.LogInformation($"[FreakStrike2] Precaching {hale.DesignerName} models...");
            }
        }
    }

    /// <summary>
    /// 타이머가 종료되는 시점에서 무작위(또는 Queuepoint 가 높은 플레이어)로 헤일 선택
    /// </summary>
    private void SetHalePlayerOnTimerEnd()
    {
        var player = _queuepoint.GetPlayerWithMostQueuepoints() ?? PlayerUtils.GetRandomPlayer();

        if (player is null)
        {
            Logger.LogError("[FreakStrike2] No player has been selected.");
            return;
        }
        
        var hale = _hales[CommonUtils.GetRandomInt(0, _hales.Count - 1)];
        _halePlayer.SetPlayerHale(player, hale, Config.HaleTeleportToSpawn);
        _queuepoint.SetPlayerQueuepoint(player.Slot, 0);
        
        ServerUtils.PrintToCenterAlertAll($"[FS2] {player.PlayerName} 이(가) {hale.Name} 헤일로 선택 되었습니다!");
        Logger.LogInformation($"[FreakStrike2] {player.PlayerName}({player.AuthorizedSteamID!.SteamId64}) has been chosen as the Hale for {hale.Name}!");
    }
}
