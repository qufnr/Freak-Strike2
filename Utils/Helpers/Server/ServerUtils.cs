using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using FreakStrike2.Utils.Helpers.Entity;

namespace FreakStrike2.Utils.Helpers.Server;

public class ServerUtils
{
    /// <summary>
    /// cs_player_manager 찾기
    /// </summary>
    public static CCSPlayerResource PlayerResource =>
        CounterStrikeSharp.API.Utilities.FindAllEntitiesByDesignerName<CCSPlayerResource>("cs_player_manager").First();

    /// <summary>
    /// cs_gamerules 찾기
    /// </summary>
    public static CCSGameRules GameRules =>
        CounterStrikeSharp.API.Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;

    /// <summary>
    /// 현재 라운드 남은 시간을 반환합니다.
    /// </summary>
    /// <returns>현재 라운드의 남은 시간</returns>
    public static float GetRaminingRoundTime()
    {
        var gamerules = GameRules;
        return gamerules.RoundStartTime + gamerules.RoundTime - CounterStrikeSharp.API.Server.CurrentTime;
    }

    /// <summary>
    /// 팀 점수를 설정합니다.
    /// </summary>
    /// <param name="team">팀 번호</param>
    /// <param name="score">설정할 점수</param>
    public static void SetTeamScore(int team, int score)
    {
        var teamManagers = CounterStrikeSharp.API.Utilities.FindAllEntitiesByDesignerName<CCSTeam>("cs_team_manager");
        foreach (var teamManager in teamManagers)
        {
            if (teamManager.TeamNum == team)
            {
                teamManager.Score = score;
                CounterStrikeSharp.API.Utilities.SetStateChanged(teamManager, "CTeam", "m_iScore");
            }
        }
    }

    /// <summary>
    /// 팀 점수를 반환합니다.
    /// </summary>
    /// <param name="team">팀 번호</param>
    /// <returns>팀 점수</returns>
    public static int GetTeamScore(int team) =>
        CounterStrikeSharp.API.Utilities.FindAllEntitiesByDesignerName<CCSTeam>("cs_team_manager")
            .Where(teamManager => teamManager.TeamNum == team)
            .First()
            .Score;

    /// <summary>
    /// 모두에게 PrintToCenter
    /// </summary>
    /// <param name="message">메시지</param>
    public static void PrintToCenterAll(string message)
    {
        foreach (var player in PlayerUtils.FindPlayersWithoutFakeClient())
            player.PrintToCenter(message);
    }

    /// <summary>
    /// 모두에게 PrintToCenterHtml
    /// </summary>
    /// <param name="message">메시지</param>
    /// <param name="duration">유지 시간</param>
    public static void PrintToCenterHtmlAll(string message, int duration = 5)
    {
        foreach (var player in PlayerUtils.FindPlayersWithoutFakeClient())
            player.PrintToCenterHtml(message, duration);
    }

    /// <summary>
    /// 모두에게 PrintToCenterAlert
    /// </summary>
    /// <param name="message">메시지</param>
    public static void PrintToCenterAlertAll(string message)
    {
        foreach (var player in PlayerUtils.FindPlayersWithoutFakeClient())
            player.PrintToCenterAlert(message);
    }
}