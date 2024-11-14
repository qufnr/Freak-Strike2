using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;

namespace FreakStrike2.Utils;

public class WeaponUtils
{
    /// <summary>
    /// 조금 더 정확한 무기의 Designer 명을 반환합니다.
    /// </summary>
    /// <param name="weapon">무기 객체</param>
    /// <returns>Designer Name</returns>
    public static string GetDesignerName(CBasePlayerWeapon weapon)
    {
        var designerName = weapon.DesignerName;
        var index = weapon.AttributeManager.Item.ItemDefinitionIndex;
        return (designerName, index) switch
        {
            var (name, _) when name.Contains("bayonet") => "weapon_knife",
            ("weapon_m4a1", 60) => "weapon_m4a1_silencer",
            ("weapon_hkp2000", 61) => "weapon_usp_silencer",
            _ => designerName
        };
    }

    /// <summary>
    /// 플레이어의 Viewmodel 을 반환합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <returns>플레이어의 Viewmodel</returns>
    public static CBaseViewModel? GetViewmodel(CCSPlayerController player)
    {
        var handle = player.PlayerPawn.Value!.ViewModelServices!.Handle;
        if (handle == IntPtr.Zero)
            return null;
        CCSPlayer_ViewModelServices viewmodelServices = new(handle);
        var viewmodelOffset = viewmodelServices.Handle + Schema.GetSchemaOffset("CCSPlayer_ViewModelServices", "m_hViewModel");
        var viewmodels = MemoryMarshal.CreateSpan(ref viewmodelOffset, 3);
        CHandle<CBaseViewModel> viewmodel = new(viewmodels[0]);
        return viewmodel.Value;
    }

    /// <summary>
    /// 플레이어의 Viewmodel 명을 반환합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <returns>Viewmodel 명 </returns>
    public static string GetViewmodelName(CCSPlayerController player)
    {
        return GetViewmodel(player)?.VMName ?? string.Empty;
    }
    
    /// <summary>
    /// 플레이어의 Viewmodel 설정
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="model">모델 경로</param>
    public static void SetViewmodel(CCSPlayerController player, string model)
    {
        GetViewmodel(player)?.SetModel(model);
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
                    GetDesignerName(w.Value!).Contains(designerName) : 
                    GetDesignerName(w.Value!) == designerName)
            .Count() > 0;
    }
    
    /// <summary>
    /// 플레이어로 부터 weaponName 에 해당되는 무기를 찾습니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="weaponName">무기 이름 (Designer Name)</param>
    /// <returns>무기 객체</returns>
    public static CBasePlayerWeapon? FindPlayerWeapon(CCSPlayerController player, string weaponName)
    {
        var weaponServices = player.PlayerPawn.Value!.WeaponServices;
        if (weaponServices is null)
            return null;

        var activeWeapon = weaponServices.ActiveWeapon.Value;
        if (activeWeapon is not null && GetDesignerName(activeWeapon) == weaponName)
            return activeWeapon;

        return weaponServices.MyWeapons
            .SingleOrDefault(mw => mw.Value != null && GetDesignerName(mw.Value) == weaponName)
            ?.Value;
    }

    /// <summary>
    /// Designer 명으로 된 플레이어가 소지중인 무기를 떨어뜨립니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="weaponName">무기 Designer 명</param>
    public static void ForceDropPlayerWeaponByDesignerName(CCSPlayerController player, string weaponName)
    {
        var weapon = player.PlayerPawn.Value!.WeaponServices!.MyWeapons
            .Where(w => GetDesignerName(w.Value!) == weaponName)
            .FirstOrDefault();
        if (weapon is not null && weapon.IsValid)
        {
            player.PlayerPawn.Value.WeaponServices.ActiveWeapon.Raw = weapon.Raw;
            player.DropActiveWeapon();
        }
    }
}