using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using FreakStrike2.Classes;
using FreakStrike2.Utils.Helpers.Entity;
using Microsoft.Extensions.Logging;

namespace FreakStrike2;

public partial class FreakStrike2
{
    /// <summary>
    /// 무기 JSON 파일 읽기
    /// </summary>
    /// <param name="hotReload">핫 리로드 유무</param>
    private void ReadWeaponJsonOnLoad(bool hotReload)
    {
        var directory = Path.Combine(Server.GameDirectory, PluginConfigDirectory);
        if (!Directory.Exists(directory))
        {
            Logger.LogError($"Couldn't find Plugin Configuration directory. [Directory Path: {directory}]");
            return;
        }

        var jsonFile = Path.Combine(directory, WeaponFilename);
        if (!File.Exists(jsonFile))
        {
            Logger.LogError($"Couldn't find Weapon Configuration file. [Path: {jsonFile}]");
            return;
        }

        if (hotReload)
            Weapons.Clear();

        Weapons = BaseWeapon.GetWeaponsFromJson(File.ReadAllText(jsonFile));
    }

    /// <summary>
    /// 엔티티(무기) 스폰 시 Clip 과 Ammo 를 설정한다.
    /// </summary>
    /// <remarks>참고: https://github.com/schwarper/cs2-advanced-weapon-system/blob/main/src/event/event.cs</remarks>
    /// <param name="entity">스폰 엔티티 객체</param>
    private void WeaponClipAndAmmoUpdateOnEntitySpawned(CEntityInstance entity)
    {
        if (Weapons.TryGetValue(entity.DesignerName, out var baseWeapon))
            return;

        if (entity.As<CCSWeaponBase>().VData is not CCSWeaponBaseVData weaponVData)
            return;

        if (baseWeapon?.Clip != null)
            weaponVData.MaxClip1 = baseWeapon.Clip;

        if (baseWeapon?.Ammo != null)
            weaponVData.PrimaryReserveAmmoMax = baseWeapon.Ammo;
    }

    /// <summary>
    /// 무기 공격 속도 설정
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    private void ModifyWeaponFireRateOnWeaponFire(CCSPlayerController? player)
    {
        if (player == null || !player.IsValid || !player.PawnIsAlive)
            return;
        
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn == null || !playerPawn.IsValid)
            return;

        var activeWeapon = playerPawn.WeaponServices!.ActiveWeapon.Value;
        if (activeWeapon == null)
            return;

        if (!Weapons.TryGetValue(WeaponUtils.GetDesignerNameEx(activeWeapon), out var baseWeapon))
            return;
        
        var weaponNextAttackTick = activeWeapon.NextPrimaryAttackTick / baseWeapon.FireRate;
        activeWeapon.SetWeaponNextPrimaryAttackTick((int) weaponNextAttackTick);
    }
}