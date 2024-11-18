using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace FreakStrike2.Utils;

public class PlayerUtils
{
    public static List<CCSPlayerController> GetValidPawnAlivePlayers() => Utilities.GetPlayers().Where(p => p.IsValid && p.PawnIsAlive).ToList();

    public static CCSPlayerController? GetRandomPlayer() =>
        GetValidPawnAlivePlayers().Where(p => p.Slot == CommonUtils.GetRandomInt(0, Server.MaxPlayers)).FirstOrDefault();
}