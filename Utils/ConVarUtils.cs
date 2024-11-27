using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Cvars;

namespace FreakStrike2.Utils;

public class ConVarUtils
{
    /// <summary>
    /// Convar "mp_round_restart_delay" 값을 float 형태로 반환합니다.
    /// </summary>
    /// <returns>mp_round_restart_delay Convar 값</returns>
    public static float GetRoundRestartDelay()
    {
        var cvar = ConVar.Find("mp_round_restart_delay");
        return cvar != null ? cvar.GetPrimitiveValue<float>() : 7.0f;
    }

    /// <summary>
    /// Convar "mp_freezetime" 값을 float 형태로 반환합니다.
    /// </summary>
    /// <returns>mp_freezetime</returns>
    public static int GetFreezeTime()
    {
        var cvar = ConVar.Find("mp_freezetime");
        return cvar != null ? cvar.GetPrimitiveValue<int>() : 0;
    }

    /// <summary>
    /// Convar "mp_roundtime*" 값을 float 형태로 반환합니다.
    /// </summary>
    /// <returns>mp_roundtime, mp_roundtime_defuse, mp_roundtime_hostage</returns>
    public static float GetRoundTime()
    {
        var mapName = Server.MapName;
        ConVar? cvar;
        
        if (mapName.StartsWith("de_")) cvar = ConVar.Find("mp_roundtime_defuse");
        else if (mapName.StartsWith("cs_")) cvar = ConVar.Find("mp_roundtime_hostage");
        else cvar = ConVar.Find("mp_roundtime");

        return cvar != null ? cvar.GetPrimitiveValue<float>() : 0;
    }
}