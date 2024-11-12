using CounterStrikeSharp.API;
using FreakStrike2.Classes;
using FreakStrike2.Exceptions;
using Microsoft.Extensions.Logging;

namespace FreakStrike2;
public partial class FreakStrike2
{
    public static string HALE_CONFIG_FILE = "playable_hales.json";
    
    private List<CBaseHale> _hales = new List<CBaseHale>();
    private Dictionary<int, CBaseHalePlayer> _halePlayers = new Dictionary<int, CBaseHalePlayer>();
    
    /// <summary>
    /// 헤일 설정 파일을 읽어옵니다.
    /// </summary>
    /// <param name="hotReload">핫리로드 유무</param>
    private void GetHaleJsonOnLoad(bool hotReload)
    {
        var path = Path.Combine(Server.GameDirectory, $"csgo\\addons\\counterstrikesharp\\configs\\plugins\\FreakStrike2\\{HALE_CONFIG_FILE}");
        if (!Directory.Exists(path))
        {
            Logger.LogError($"Couldn't find Hale Configuration file. Path: {path}");
            return;
        }

        if (hotReload)
        {
            _hales.Clear();
        }

        _hales = CBaseHale.GetHalesFromJson(File.ReadAllText(path));
    }

    private void CleanUpHalePlayerOnClientPutInServer(int client)
    {
        _halePlayers[client] = new CBaseHalePlayer();
    }

    private void ChooseHaleOnGameTimerEnd()
    {
        //  TODO : 랜덤으로 헤일 뽑기
    }
}
