using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace FreakStrike2;

public partial class FreakStrike2
{
    private void HookVirtualFunctions()
    {
        VirtualFunctions.CCSPlayer_ItemServices_CanAcquireFunc.Hook(OnWeaponCanAcquire, HookMode.Pre);
    }

    private void UnhookVirtualFunctions()
    {
        VirtualFunctions.CCSPlayer_ItemServices_CanAcquireFunc.Unhook(OnWeaponCanAcquire, HookMode.Pre);
    }

    private HookResult OnWeaponCanAcquire(DynamicHook hook)
    {
        var player = hook.GetParam<CCSPlayer_ItemServices>(0).Pawn.Value.Controller.Value!.As<CCSPlayerController>();
        if (!player.IsValid || !player.PawnIsAlive)
            return HookResult.Continue;

        if (BaseHalePlayers[player.Slot].IsHale())
            return HookResult.Stop;
        
        return HookResult.Continue;
    }
}