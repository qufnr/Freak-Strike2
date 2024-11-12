using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;

namespace FreakStrike2.Utils;

public class CommonUtils
{
    /// <summary>
    /// GameRules 반환
    /// </summary>
    /// <returns>CCSGameRules</returns>
    public static CCSGameRules GetGameRules()
    {
        return Utilities
            .FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
            .First()
            .GameRules!;
    }

    /// <summary>
    /// Convar "mp_round_restart_delay" 값을 float 형태로 반환합니다.
    /// </summary>
    /// <returns>mp_round_restart_delay</returns>
    public static float GetRoundRestartDelay()
    {
        var cvarRoundRestartDelay = ConVar.Find("mp_round_restart_delay");
        return cvarRoundRestartDelay != null ? cvarRoundRestartDelay.GetPrimitiveValue<float>() : 7.0f;
    }

    /// <summary>
    /// 특정한 팀에서 살아있는 플레이어 수를 반환합니다.
    /// </summary>
    /// <param name="team">팀</param>
    /// <returns>팀에 살아있는 플레이어 수</returns>
    public static int GetTeamAlivePlayers(CsTeam team)
    {
        return Utilities.GetPlayers()
            .Where(player => player.IsValid && player.PawnIsAlive && player.Team == team)
            .Count();
    }
}