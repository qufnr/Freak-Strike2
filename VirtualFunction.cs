using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Classes;
using FreakStrike2.Models;
using FreakStrike2.Utils;

namespace FreakStrike2;

public partial class FreakStrike2
{
    private void HookVirtualFunctions()
    {
        VirtualFunctions.CCSPlayer_ItemServices_CanAcquireFunc.Hook(OnWeaponCanAcquire, HookMode.Pre);
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnEntityTakeDamage, HookMode.Pre);
        VirtualFunctions.CCSPlayerPawnBase_PostThinkFunc.Hook(OnPostThinkPost, HookMode.Post);
    }

    private void UnhookVirtualFunctions()
    {
        VirtualFunctions.CCSPlayer_ItemServices_CanAcquireFunc.Unhook(OnWeaponCanAcquire, HookMode.Pre);
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnEntityTakeDamage, HookMode.Pre);
        VirtualFunctions.CCSPlayerPawnBase_PostThinkFunc.Unhook(OnPostThinkPost, HookMode.Post);
    }

    /// <summary>
    /// 무기 장착
    /// </summary>
    /// <param name="hook">동적 훅</param>
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
    /// 엔티티가 피해를 입을 때
    /// </summary>
    /// <param name="hook">동적 훅</param>
    /// <returns>훅 결과</returns>
    private HookResult OnEntityTakeDamage(DynamicHook hook)
    {
        var info = hook.GetParam<CTakeDamageInfo>(1);
        var victimEntity = hook.GetParam<CEntityInstance>(0);
        var attackerEntity = info.Attacker.Value;
        
        //  피해자가 플레이어일 경우
        if (victimEntity.DesignerName == "player")
        {
            var victimPawn = victimEntity.As<CCSPlayerPawn>();
            var victim = victimPawn.OriginalController.Get();
            
            if (victim != null && victim.IsValid)
            {
                if (info.BitsDamageType == DamageTypes_t.DMG_FALL)
                {
                    //  플레이어가 헤일일 경우 낙하 피해 무시
                    if (BaseHalePlayers[victim.Slot].IsHale)
                    {
                        info.Damage = 0;
                        info.OriginalDamage = 0;

                        return HookResult.Stop;
                    }
                    //  플레이어가 인간 진영일 경우 낙하 피해 처리
                    else
                    {
                        if (info.OriginalDamage > 3)
                        {
                            victimPawn.AimPunchAngle.X = info.OriginalDamage * 0.25f;
                            victimPawn.AimPunchAngle.Y = info.OriginalDamage * 0.25f;
                            victimPawn.AimPunchAngle.Z = info.OriginalDamage * 0.25f;
                        }
                        
                        if (Config.UnrealityFallDamage)
                        {
                            if (info.OriginalDamage <= 5)
                            {
                                info.Damage = 0;
                                info.OriginalDamage = 0;
                                return HookResult.Stop;
                            }
                            else
                            {
                                info.Damage = 10;
                                info.OriginalDamage = info.Damage;
                                return HookResult.Changed;
                            }
                        }
                    }
                }
            }
            
            //  플레이어(피해자)와 플레이어(가해자)간의 피해
            if (attackerEntity != null && attackerEntity.DesignerName == "player")
            {
                var attackerPawn = new CCSPlayerPawn(attackerEntity.Handle);
                var attacker = attackerPawn.OriginalController.Get();

                if (victim != null && victim.IsValid && attacker != null && attacker.IsValid)
                {
                    //  라운드 종료 시 헤일에 대한 피해 무효 처리
                    if (InGameStatus != GameStatus.Start && BaseHalePlayers[attacker.Slot].IsHale)
                    {
                        info.Damage = 0;
                        info.OriginalDamage = 0;
                    
                        return HookResult.Stop;
                    }
                }
            }
        }

        return HookResult.Continue;
    }

    /// <summary>
    /// 포스트 띵크 포스트
    /// </summary>
    /// <param name="hook">동적 훅</param>
    /// <returns>훅 결과</returns>
    private HookResult OnPostThinkPost(DynamicHook hook)
    {
        //  TODO :: PostThinkFunc 에서 플레이어 찾는 방법 없을까?
        foreach (var player in Utilities.GetPlayers().Where(player => player.IsValid))
        {
            SuperJumpOnPostThinkPost(player);
        }
        
        return HookResult.Continue;
    }
}