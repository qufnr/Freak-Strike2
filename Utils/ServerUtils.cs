using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace FreakStrike2.Utils;

public class ServerUtils
{
    /// <summary>
    /// cs_player_manager 찾기
    /// </summary>
    /// <returns>CCSPlayerResource</returns>
    public static CCSPlayerResource GetPlayerResource() =>
        Utilities.FindAllEntitiesByDesignerName<CCSPlayerResource>("cs_player_manager").First();

    /// <summary>
    /// 팀 점수를 설정합니다.
    /// </summary>
    /// <param name="team">팀 번호</param>
    /// <param name="score">설정할 점수</param>
    public static void SetTeamScore(int team, int score)
    {
        var teamManagers = Utilities.FindAllEntitiesByDesignerName<CCSTeam>("cs_team_manager");
        foreach (var teamManager in teamManagers)
        {
            if (teamManager.TeamNum == team)
            {
                teamManager.Score = score;
                Utilities.SetStateChanged(teamManager, "CTeam", "m_iScore");
            }
        }
    }

    /// <summary>
    /// 팀 점수를 반환합니다.
    /// </summary>
    /// <param name="team">팀 번호</param>
    /// <returns>팀 점수</returns>
    public static int GetTeamScore(int team) =>
        Utilities.FindAllEntitiesByDesignerName<CCSTeam>("cs_team_manager")
            .Where(teamManager => teamManager.TeamNum == team)
            .First()
            .Score;
    
    /// <summary>
    /// 모두에게 PrintToCenter
    /// </summary>
    /// <param name="message">메시지</param>
    public static void PrintToCenterAll(string message) =>
        Utilities.GetPlayers().Where(player => player.IsValid && !player.IsBot)
            .ToList()
            .ForEach(player => player.PrintToCenter(message));

    /// <summary>
    /// 모두에게 PrintToCenterHtml
    /// </summary>
    /// <param name="message">메시지</param>
    /// <param name="duration">유지 시간</param>
    public static void PrintToCenterHtmlAll(string message, int duration = 5) =>
        Utilities.GetPlayers().Where(player => player.IsValid && !player.IsBot)
            .ToList()
            .ForEach(player => player.PrintToCenterHtml(message, duration));
    
    /// <summary>
    /// 모두에게 PrintToCenterAlert
    /// </summary>
    /// <param name="message">메시지</param>
    public static void PrintToCenterAlertAll(string message) =>
        Utilities.GetPlayers().Where(player => player.IsValid && !player.IsBot)
            .ToList()
            .ForEach(player => player.PrintToCenterAlert(message));
}