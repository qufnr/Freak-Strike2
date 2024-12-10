using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Models;
using FreakStrike2.Utils.Helpers.Entity;

namespace FreakStrike2;

public partial class FreakStrike2
{
    private void HookVirtualFunctions()
    {
        VirtualFunctions.CCSPlayer_ItemServices_CanAcquireFunc.Hook(OnWeaponCanAcquire, HookMode.Pre);
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnEntityTakeDamage, HookMode.Pre);
        VirtualFunctions.CCSPlayerPawnBase_PostThinkFunc.Hook(OnPostThinkPost, HookMode.Post);
        VirtualFunctions.CBaseTrigger_StartTouchFunc.Hook(OnEntityStartTouch, HookMode.Post);
    }

    private void UnhookVirtualFunctions()
    {
        VirtualFunctions.CCSPlayer_ItemServices_CanAcquireFunc.Unhook(OnWeaponCanAcquire, HookMode.Pre);
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnEntityTakeDamage, HookMode.Pre);
        VirtualFunctions.CCSPlayerPawnBase_PostThinkFunc.Unhook(OnPostThinkPost, HookMode.Post);
        VirtualFunctions.CBaseTrigger_StartTouchFunc.Unhook(OnEntityStartTouch, HookMode.Post);
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

        var weaponName = weaponVData.Name;
        //  칼 스킨 있는 사람은 칼 이름이 달라서 weapon_knife 로 통일
        if (weaponName.Contains("knife_"))
        {
            var explode = weaponName.Split('_');
            weaponName = $"{explode[0]}_{explode[1]}";
        }
        
        if (BaseHalePlayers[player.Slot].IsHale)
        {
            //  헤일 무기 못 줍게 하기
            if (!BaseHalePlayers[player.Slot].MyHale!.Weapons.Contains(weaponName))
                return HookResult.Stop;
        }
        else
        {
            //  인간 클래스 전용 무기 외 못 줍게 하기
            var humanClass = BaseHumanPlayers[player.Slot].MyClass;
            if (humanClass != null && !humanClass.ExclusiveWeapons.Contains(weaponName))
                return HookResult.Stop;    
        }
        
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
        var weapon = info.Ability.Value;
        
        //  피해자가 플레이어일 경우
        if (victimEntity.DesignerName == "player")
        {
            var victimPawn = victimEntity.As<CCSPlayerPawn>();
            var victim = victimPawn.OriginalController.Get();
            
            #region 플레이어가 무언가로 부터 피해를 입었을 때
            if (victim != null && victim.IsValid)
            {
                //  헤일 준비 상태에서 피해 무효화
                if (ReadyInterval > 0 && BaseHalePlayers[victim.Slot].IsHale)
                {
                    info.Damage = 0;
                    
                    return HookResult.Stop;
                }
                
                //  낙하 피해 확인
                if (info.BitsDamageType == DamageTypes_t.DMG_FALL)
                {
                    //  플레이어가 헤일일 경우 낙하 피해 무시
                    if (BaseHalePlayers[victim.Slot].IsHale)
                    {
                        info.Damage = 0;

                        return HookResult.Stop;
                    }
                    //  플레이어가 인간 진영일 경우 낙하 피해 처리
                    else
                    {
                        if (info.Damage > 3)
                            victim.SetAimPunchAngle(new Vector() { X = info.Damage * .25f, Y = info.Damage * .5f, Z = info.Damage * .25f });
                        
                        if (Config.UnrealityFallDamage)
                        {
                            if (info.Damage <= 5)
                            {
                                info.Damage = 0;
                                
                                return HookResult.Stop;
                            }
                            else
                            {
                                info.Damage = 10;
                                
                                return HookResult.Changed;
                            }
                        }
                    }
                }
            }
            #endregion
            
            #region 플레이어와 플레이어간 피해를 가했을 때
            //  플레이어(피해자)와 플레이어(가해자)간의 피해
            if (attackerEntity != null && attackerEntity.DesignerName == "player")
            {
                var attackerPawn = new CCSPlayerPawn(attackerEntity.Handle);
                var attacker = attackerPawn.OriginalController.Get();

                if (victim != null && victim.IsValid && 
                    attacker != null && attacker.IsValid &&
                    victimPawn.TeamNum != attackerPawn.TeamNum)
                {
                    //  라운드 종료 시 플레이어간의 피해 무효 처리
                    if (InGameStatus != GameStatus.Start)
                    {
                        info.Damage = 0;
                    
                        return HookResult.Stop;
                    }
                    
                    //  무기 피해량 수정 / 넉백 만들기
                    if (weapon != null && !BaseHalePlayers[attacker.Slot].IsHale)
                    {
                        var changed = false;
                        
                        if (Weapons.TryGetValue(weapon.DesignerName, out var baseWeapon))
                        {
                            info.Damage = baseWeapon.Damage.Equals(-1.0f) ? 0 : info.Damage * baseWeapon.Damage;
                            changed = true;
                        }

                        PlayerKnockbackOnTakeDamage(victimPawn, attackerPawn, baseWeapon, info.Damage);

                        return changed ? HookResult.Changed : HookResult.Continue;
                    }
                }
            }
            #endregion
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
            WeightDownOnPostThinkPost(player);
            HalePlayerSecondaryAttackBlockOnPostThinkPost(player);
        }
        
        return HookResult.Continue;
    }

    /// <summary>
    /// 엔티티 터치
    /// </summary>
    /// <param name="hook">동적 훅</param>
    /// <returns>훅 결과</returns>
    private HookResult OnEntityStartTouch(DynamicHook hook)
    {
        var trigger = hook.GetParam<CBaseTrigger>(0);
        var entity = hook.GetParam<CBaseEntity>(1);
        
        Console.WriteLine($"[FreakStrike2] trigger {trigger} entity {entity}");

        // if (trigger.DesignerName == "player" && entity.DesignerName == "player")
        // {
        //     var targetPawn = trigger.As<CCSPlayerPawn>();
        //     var playerPawn = trigger.As<CCSPlayerPawn>();
        //
        //     if (targetPawn.IsValid && playerPawn.IsValid)
        //     {
        //         //  TODO :: 터치 테스트
        //         Logger.LogInformation($"[FreakStrike2] Touch :: target {targetPawn} | player {playerPawn}");
        //     }
        // }

        return HookResult.Continue;
    }
}