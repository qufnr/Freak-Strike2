using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;

namespace FreakStrike2.Utils;

public static class WeaponUtils
{
    /// <summary>
    /// 조금 더 정확한 무기의 Designer 명을 반환합니다.
    /// </summary>
    /// <param name="weapon">무기 객체</param>
    /// <returns>Designer Name</returns>
    public static string GetDesignerNameEx(this CBasePlayerWeapon weapon)
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
    public static CBaseViewModel? GetViewmodel(this CCSPlayerController player)
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
    public static string GetViewmodelName(this CCSPlayerController player)
    {
        return player.GetViewmodel()?.VMName ?? string.Empty;
    }
    
    /// <summary>
    /// 플레이어의 Viewmodel 설정
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="model">모델 경로</param>
    public static void SetViewmodel(this CCSPlayerController player, string model)
    {
        player.GetViewmodel()?.SetModel(model);
    }

    /// <summary>
    /// 플레이어의 무기 모델을 업데이트합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="weapon">플레이어 무기 객체</param>
    /// <param name="model">모델 </param>
    /// <param name="updateViewmodel">Viewmodel 업데이트 여부</param>
    public static void UpdateWeaponModel(this CCSPlayerController player, CBasePlayerWeapon weapon, string model, bool updateViewmodel)
    {
        weapon.Globalname = $"{player.GetViewmodelName()}:{model}";
        weapon.SetModel(model);
        if(updateViewmodel)
            player.SetViewmodel(model);
    }

    /// <summary>
    /// 플레이어 무기의 모델을 삭제합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="weapon">무기 객체</param>
    /// <param name="updateViewmodel">Viewmodel 업데이트 여부</param>
    public static void RemoveWeaponModel(this CCSPlayerController player, CBasePlayerWeapon weapon, bool updateViewmodel)
    {
        if (string.IsNullOrEmpty(weapon.Globalname))
            return;
        var weaponModel = weapon.Globalname.Split(':')[0];
        weapon.Globalname = string.Empty;
        weapon.SetModel(weaponModel);
        if(updateViewmodel)
            player.SetViewmodel(weaponModel);
    }

    /// <summary>
    /// 무기를 모두 삭제합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="drop">무기 떨어뜨리기 여부 (삭제되는 무기를 땅에 떨어뜨립니다.)</param>
    /// <param name="ignoreKnife">삭제 무기 중 근접무기 제외 여부</param>
    public static void ForceRemoveWeapons(this CCSPlayerController player, bool drop = false, bool ignoreKnife = false)
    {
        //  TODO :: 크래시남 고치기
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn is null || !playerPawn.IsValid)
            return;
        var weaponServices = playerPawn.WeaponServices;
        if (weaponServices is null)
            return;
        
        var weapons = weaponServices.MyWeapons;
        foreach (var weapon in weapons)
        {
            if (weapon.Value is not null && weapon.IsValid)
            {
                var weaponVData = weapon.Value.As<CCSWeaponBase>().GetVData<CCSWeaponBaseVData>();
                if (ignoreKnife && weaponVData!.GearSlot is gear_slot_t.GEAR_SLOT_KNIFE)
                    continue;
                if (drop)
                    player.ForceDropWeaponByDesignerName(weapon.Value.DesignerName);
                else
                    weapon.Value.Remove();
            }
        }
    }

    /// <summary>
    /// 무기 보조 공격 다음 틱을 설정합니다.
    /// </summary>
    /// <param name="weapon">무기 객체</param>
    /// <param name="nextTick">틱</param>
    public static void SetWeaponNextSecondaryAttackTick(this CBasePlayerWeapon weapon, int nextTick)
    {
        weapon.NextSecondaryAttackTick = nextTick;
        Utilities.SetStateChanged(weapon, "CBasePlayerWeapon", "m_nNextSecondaryAttackTick");
    }
    
    /// <summary>
    /// 플레이어가 특정 무기를 소지하고 있는지 여부를 반환합니다.
    /// </summary>
    /// <param name="player">플레이어</param>
    /// <param name="designerName">무기 Designer 명</param>
    /// <param name="contains">찾을 때 일부 일치 여부</param>
    /// <returns>무기를 소지하고 있으면 true, 아니면 false 반환</returns>
    public static bool HasWeaponByDesignerName(this CCSPlayerController player, string designerName, bool contains = false)
    {
        return player.PlayerPawn.Value!.WeaponServices!.MyWeapons
            .Where(w => 
                contains ? 
                    w.Value!.GetDesignerNameEx().Contains(designerName) : 
                    w.Value!.GetDesignerNameEx() == designerName)
            .Count() > 0;
    }
    
    /// <summary>
    /// 플레이어로 부터 weaponName 에 해당되는 무기를 찾습니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="weaponName">무기 이름 (Designer Name)</param>
    /// <returns>무기 객체</returns>
    public static CBasePlayerWeapon? FindWeapon(this CCSPlayerController player, string weaponName)
    {
        var weaponServices = player.PlayerPawn.Value!.WeaponServices;
        if (weaponServices is null)
            return null;

        var activeWeapon = weaponServices.ActiveWeapon.Value;
        if (activeWeapon is not null && activeWeapon.GetDesignerNameEx() == weaponName)
            return activeWeapon;

        return weaponServices.MyWeapons
            .SingleOrDefault(mw => mw.Value != null && mw.Value!.GetDesignerNameEx() == weaponName)
            ?.Value;
    }

    /// <summary>
    /// Designer 명으로 된 플레이어가 소지중인 무기를 떨어뜨립니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="weaponName">무기 Designer 명</param>
    public static void ForceDropWeaponByDesignerName(this CCSPlayerController player, string weaponName)
    {
        var weapon = player.PlayerPawn.Value!.WeaponServices!.MyWeapons
            .Where(w => w.Value!.GetDesignerNameEx() == weaponName)
            .FirstOrDefault();
        if (weapon is not null && weapon.IsValid)
        {
            player.PlayerPawn.Value.WeaponServices.ActiveWeapon.Raw = weapon.Raw;
            player.DropActiveWeapon();
        }
    }
}