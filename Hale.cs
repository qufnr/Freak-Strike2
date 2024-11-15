using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Classes;
using FreakStrike2.Exceptions;
using Microsoft.Extensions.Logging;

namespace FreakStrike2;
public partial class FreakStrike2
{
    public static string PLUGIN_CONFIG_DIRECTORY = "csgo\\addons\\counterstrikesharp\\configs\\plugins\\FreakStrike2\\";
    public static string HALE_CONFIG_FILE = "playable_hales.json";
    
    private List<BaseHale> _hales = new List<BaseHale>();
    private Dictionary<int, BaseHalePlayer> _halePlayers = new Dictionary<int, BaseHalePlayer>();
    
    /// <summary>
    /// 헤일 설정 파일을 읽어옵니다.
    /// </summary>
    /// <param name="hotReload">핫리로드 유무</param>
    private void GetHaleJsonOnLoad(bool hotReload)
    {
        var directory = Path.Combine(Server.GameDirectory, PLUGIN_CONFIG_DIRECTORY);
        if (!Directory.Exists(directory))
        {
            Logger.LogError($"Couldn't find Plugin Configuration directory. [Directory Path: {directory}]");
            return;
        }
        
        var jsonFile = Path.Combine(directory, HALE_CONFIG_FILE);
        if (!File.Exists(jsonFile))
        {
            Logger.LogError($"Couldn't find Hale Configuration file. [Path: {jsonFile}]");
            return;
        }

        if (hotReload)
        {
            _hales.Clear();
        }

        _hales = BaseHale.GetHalesFromJson(File.ReadAllText(jsonFile));
    }

    private void PrecacheHaleModels(ResourceManifest manifest)
    {
        foreach (var hale in _hales)
        {
            if(!string.IsNullOrEmpty(hale.Model)) manifest.AddResource(hale.Model);
            if(!string.IsNullOrEmpty(hale.ArmsModel)) manifest.AddResource(hale.ArmsModel);
            if(!string.IsNullOrEmpty(hale.Viewmodel)) manifest.AddResource(hale.Viewmodel);
        }
    }

    private void CleanUpHalePlayerOnClientPutInServer(int client)
    {
        _halePlayers[client] = new BaseHalePlayer();
    }

    private void SetHalePlayerOnTimerEnd()
    {
        //  TODO : 랜덤으로 헤일 뽑기
    }
}
