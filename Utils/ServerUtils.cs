using CounterStrikeSharp.API;

namespace FreakStrike2.Utils;

public class ServerUtils
{
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
    public static void PrintToCenterHtmlAll(string message) =>
        Utilities.GetPlayers().Where(player => player.IsValid && !player.IsBot)
            .ToList()
            .ForEach(player => player.PrintToCenterHtml(message));
    
    /// <summary>
    /// 모두에게 PrintToCenterAlert
    /// </summary>
    /// <param name="message">메시지</param>
    public static void PrintToCenterAlertAll(string message) =>
        Utilities.GetPlayers().Where(player => player.IsValid && !player.IsBot)
            .ToList()
            .ForEach(player => player.PrintToCenterAlert(message));
}