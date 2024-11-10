using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;

namespace FreakStrike2.Utils;

public class CommonUtils
{
    public static CCSGameRules GetGameRules()
    {
        return CounterStrikeSharp.API.Utilities
            .FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
            .First()
            .GameRules!;
    }

    public static float GetRoundRestartDelay()
    {
        var cvarRoundRestartDelay = ConVar.Find("mp_round_restart_delay");
        return cvarRoundRestartDelay != null ? cvarRoundRestartDelay.GetPrimitiveValue<float>() : 7.0f;
    }
}