using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using Microsoft.Extensions.Logging;

namespace FreakStrike2
{
    public partial class FreakStrike2
    {
        private FakeConVar<int> cvarFindInterval = new("fs2_find_interval", "헤일을 찾는 시간입니다.", 20, ConVarFlags.FCVAR_NONE, new RangeValidator<int>(5, 100));
        private FakeConVar<bool> cvarFoundHaleTeleport = new("fs2_found_hale_teleport", "헤일로 선택된 플레이어를 스폰지역으로 텔레포트합니다.", true);
        
        public void ConfigurationOnLoad()
        {
            RegisterFakeConVars(typeof(ConVar));
        }

        private bool ConfigurationInitialize()
        {
            var configDir = Path.Combine(Server.GameDirectory, "csgo/cfg/fs2/");
            if (!Directory.Exists(configDir))
            {
                Logger.LogError($"[FreakStrike2] Couldn't find fs2 config directory: {configDir}");
                return false;
            }

            var configPath = Path.Combine(configDir, "fs2.cfg");
            if (!Directory.Exists(configPath))
            {
                CreateAutoExecConfig(configPath);
                Logger.LogInformation($"[FreakStrike2] Creating {configPath}!");
            }
            
            Server.ExecuteCommand("exec fs2/fs2");

            return true;
        }

        private static void CreateAutoExecConfig(string path)
        {
            var execConfig = File.CreateText(path);
            execConfig.WriteLine("fs2_find_interval \"20\"");
            execConfig.WriteLine("fs2_found_hale_teleport \"1\"");
            execConfig.Close();
        }
    }
}
