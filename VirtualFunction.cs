using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using FreakStrike2.Classes;

namespace FreakStrike2;

public partial class FreakStrike2
{
    private void HookVirtualFunctions()
    {
        VirtualFunctions.CCSPlayer_ItemServices_CanAcquireFunc.Hook(OnWeaponCanAcquire, HookMode.Pre);
        VirtualFunctions.CCSPlayerPawnBase_PostThinkFunc.Hook(OnPostThinkPost, HookMode.Post);
    }

    private void UnhookVirtualFunctions()
    {
        VirtualFunctions.CCSPlayer_ItemServices_CanAcquireFunc.Unhook(OnWeaponCanAcquire, HookMode.Pre);
        VirtualFunctions.CCSPlayerPawnBase_PostThinkFunc.Unhook(OnPostThinkPost, HookMode.Post);
    }

    /// <summary>
    /// 무기 장착
    /// </summary>
    /// <param name="hook">정적 훅</param>
    /// <returns>훅 결과</returns>
    private HookResult OnWeaponCanAcquire(DynamicHook hook)
    {
        //  0 player, 1 item
        
        var player = hook.GetParam<CCSPlayer_ItemServices>(0).Pawn.Value.Controller.Value!.As<CCSPlayerController>();
        var weaponVData = VirtualFunctions.GetCSWeaponDataFromKeyFunc
                              .Invoke(-1, hook.GetParam<CEconItemView>(1).ItemDefinitionIndex.ToString())
                          ?? throw new Exception("Failed to get \"CCSWeaponBaseVData\"!");
        
        if (!player.IsValid || !player.PawnIsAlive)
            return HookResult.Continue;

        //  헤일 무기 못 줍게 하기
        if (BaseHalePlayers[player.Slot].IsHale &&
            !BaseHalePlayers[player.Slot].MyHale!.Weapons.Contains(weaponVData.Name))
            return HookResult.Stop;
        
        return HookResult.Continue;
    }

    /// <summary>
    /// 포스트 띵크 포스트
    /// </summary>
    /// <param name="hook">정적 훅</param>
    /// <returns>훅 결과</returns>
    private HookResult OnPostThinkPost(DynamicHook hook)
    {
        //  TODO :: PostThinkFunc 에서 플레이어 찾는 방법 없을까?
        foreach (var player in Utilities.GetPlayers().Where(player => player.IsValid))
        {
            DynamicJumpOnPostThinkPost(player);
        }
        
        return HookResult.Continue;
    }
}