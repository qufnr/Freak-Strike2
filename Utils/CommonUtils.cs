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
    /// <returns>mp_round_restart_delay Convar 값</returns>
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

    /// <summary>
    /// 무기를 모두 삭제합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="drop">무기 떨어뜨리기 여부 (삭제되는 무기를 땅에 떨어뜨립니다.)</param>
    /// <param name="ignoreKnife">삭제 무기 중 근접무기 제외 여부</param>
    public static void ForceRemoveWeapons(CCSPlayerController player, bool drop = false, bool ignoreKnife = false)
    {
        var weapons = player.PlayerPawn.Value!.WeaponServices!.MyWeapons;
        for (var i = weapons.Count - 1; i >= 0; i--)
        {
            if (weapons[i].IsValid)
            {
                var weaponVData = weapons[i].Value!.As<CCSWeaponBase>().GetVData<CCSWeaponBaseVData>();
                if (ignoreKnife && weaponVData!.GearSlot is gear_slot_t.GEAR_SLOT_KNIFE)
                {
                    continue;
                }

                if (drop)
                {
                    ForceDropPlayerWeaponByDesignerName(player, weapons[i].Value!.DesignerName);
                }
                else
                {
                    weapons[i].Value!.Remove();
                }
            }
        }
    }

    /// <summary>
    /// Designer 명으로 된 플레이어가 소지중인 무기를 떨어뜨립니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="weaponDesignerName">무기 Designer 명</param>
    public static void ForceDropPlayerWeaponByDesignerName(CCSPlayerController player, string weaponDesignerName)
    {
        var weapon = player.PlayerPawn.Value!.WeaponServices!.MyWeapons
            .Where(w => w.Value!.DesignerName == weaponDesignerName)
            .FirstOrDefault();
        if (weapon is not null && weapon.IsValid)
        {
            player.PlayerPawn.Value.WeaponServices.ActiveWeapon.Raw = weapon.Raw;
            player.DropActiveWeapon();
        }
    }

    /// <summary>
    /// 플레이어가 특정 무기를 소지하고 있는지 여부를 반환합니다.
    /// </summary>
    /// <param name="player">플레이어</param>
    /// <param name="designerName">무기 Designer 명</param>
    /// <param name="contains">찾을 때 일부 일치 여부</param>
    /// <returns>무기를 소지하고 있으면 true, 아니면 false 반환</returns>
    public static bool HasWeaponByDesignerName(CCSPlayerController player, string designerName, bool contains = false)
    {
        return player.PlayerPawn.Value!.WeaponServices!.MyWeapons
            .Where(w => 
                contains ? 
                    w.Value!.DesignerName.Contains(designerName) : 
                    w.Value!.DesignerName == designerName)
            .Count() > 0;
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