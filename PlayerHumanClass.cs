using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Classes;
using Microsoft.Extensions.Logging;
using Serilog;

namespace FreakStrike2;

public partial class FreakStrike2
{
    /// <summary>
    /// 인간 클래스 JSON 읽기
    /// </summary>
    /// <param name="hotReload">핫 리로드 유무</param>
    private void ReadHumanJsonOnLoad(bool hotReload)
    {
        var directory = Path.Combine(Server.GameDirectory, PluginConfigDirectory);
        if (!Directory.Exists(directory))
        {
            Logger.LogError($"Couldn't find Plugin Configuration directory. [Directory Path: {directory}]");
            return;
        }
        
        var jsonFile = Path.Combine(directory, HumanConfigFilename);
        if (!File.Exists(jsonFile))
        {
            Logger.LogError($"Couldn't find Human Configuration file. [Path: {jsonFile}]");
            return;
        }

        if (hotReload)
            Humans.Clear();

        Humans = BaseHuman.GetHumansFromJson(File.ReadAllText(jsonFile));
    }

    /// <summary>
    /// 인간 클래스 모델 프리캐싱
    /// </summary>
    /// <param name="manifest">매니페스트</param>
    private void PrecacheHumanModels(ResourceManifest manifest)
    {
        foreach (var human in Humans)
        {
            if (!string.IsNullOrEmpty(human.Model) || !string.IsNullOrEmpty(human.ArmsModel))
            {
                if (!string.IsNullOrEmpty(human.Model))  manifest.AddResource(human.Model);
                if (!string.IsNullOrEmpty(human.ArmsModel)) manifest.AddResource(human.ArmsModel);
                Logger.LogInformation($"[FreakStrike2] Precaching {human.Name} models...");
            }

            foreach (var weaponModel in human.GetExclusiveWeaponModels())
            {
                manifest.AddResource(weaponModel);
                Log.Information($"[FreakStrike2] Precaching {human.Name} Weapon models...");
            }
        }
    }
}