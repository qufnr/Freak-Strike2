using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Memory;
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

    /// <summary>
    /// min 부터 max 까지 난수 생성
    /// </summary>
    /// <param name="min">최소값</param>
    /// <param name="max">최대값</param>
    /// <returns>무작위 숫자 (int)</returns>
    public static int GetRandomInt(int min, int max)
    {
        return new Random().Next(min, max + 1);
    }
}