using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Classes;
using FreakStrike2.Models;
using FreakStrike2.Utils.Helpers;
using FreakStrike2.Utils.Helpers.Entity;
using FreakStrike2.Utils.Helpers.Server;
using Microsoft.Extensions.Logging;

namespace FreakStrike2;
public partial class FreakStrike2
{
    /// <summary>
    /// 헤일 설정 파일을 읽어옵니다.
    /// </summary>
    /// <param name="hotReload">핫리로드 유무</param>
    private void ReadHaleJsonOnLoad(bool hotReload)
    {
        var directory = Path.Combine(Server.GameDirectory, PluginConfigDirectory);
        if (!Directory.Exists(directory))
        {
            Logger.LogError($"Couldn't find Plugin Configuration directory. [Directory Path: {directory}]");
            return;
        }
        
        var jsonFile = Path.Combine(directory, HaleConfigFilename);
        if (!File.Exists(jsonFile))
        {
            Logger.LogError($"Couldn't find Hale Configuration file. [Path: {jsonFile}]");
            return;
        }

        if (hotReload)
            Hales.Clear();

        Hales = BaseHale.GetHalesFromJson(File.ReadAllText(jsonFile));
    }

    /// <summary>
    /// 헤일 모델 프리캐싱
    /// </summary>
    /// <param name="manifest">리소스 매니페스트</param>
    private void PrecacheHaleModels(ResourceManifest manifest)
    {
        foreach (var hale in Hales)
        {
            if (!string.IsNullOrEmpty(hale.Model) || !string.IsNullOrEmpty(hale.ArmsModel) || !string.IsNullOrEmpty(hale.Viewmodel))
            {
                if (!string.IsNullOrEmpty(hale.Model)) manifest.AddResource(hale.Model);
                if (!string.IsNullOrEmpty(hale.ArmsModel)) manifest.AddResource(hale.ArmsModel);
                if (!string.IsNullOrEmpty(hale.Viewmodel)) manifest.AddResource(hale.Viewmodel);
                Logger.LogInformation($"[FreakStrike2] Precaching {hale.DesignerName} models...");
            }
        }
    }

    /// <summary>
    /// 라운드가 시작되는 시점에서 무작위 플레이어(또는 큐포인트가 높은 플레이어)를 헤일로 선택
    /// </summary>
    private void CreateHalePlayerOnRoundStart()
    {
        if (ServerUtils.GameRules.WarmupPeriod || Utilities.GetPlayers().Count <= 1)
            return;
        
        var player = BaseQueuePoint.GetPlayerWithMostQueuePoints() ?? PlayerUtils.GetRandomAlivePlayer();
        if (player == null)
        {
            Logger.LogError("[FreakStrike2] No player has been selected.");
            return;
        }

        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn == null)
        {
            Logger.LogError("[FreakStrike2] Player \"CCSPlayerPawn\" is null!");
            return;
        }

        var hale = CommonUtils.GetRandomInList(Hales);
        BaseHalePlayers[player.Slot].SetHaleState(hale);
        player.SetMoveType(MoveType_t.MOVETYPE_NONE);
        playerPawn.AbsVelocity.X = 0;
        playerPawn.AbsVelocity.Y = 0;
        playerPawn.AbsVelocity.Z = 0;
        PlayerQueuePoints[player.Slot].Points = 0;
        
        player.PrintToCenterAlert($"귀하가 이번 라운드의 헤일 {BaseHalePlayers[player.Slot].MyHale!.Name} (으)로 선정되었습니다!");
        Logger.LogInformation($"[FreakStrike2] {player.PlayerName}({player.AuthorizedSteamID!.SteamId64}) has been chosen as the Hale for {hale.Name}!");
        
        //  헤일 외 다른 플레이어는 인간 진영으로 이동
        foreach (var other in Utilities.GetPlayers().Where(p => p.IsValid && p.PawnIsAlive && !BaseHalePlayers[p.Slot].IsHale))
        {
            other.SwitchTeam((CsTeam) Fs2Team.Human);                   //  팀 변경
            other.TeleportToSpawnPoint((CsTeam) Fs2Team.Human);         //  스폰으로 텔레포트
            BaseHumanPlayers[other.Slot].SetHumanClassState();          //  플레이어 클래스 설정
        }
    }

    /// <summary>
    /// 타이머가 종료되면 헤일의 활동 상태를 변경합니다.
    /// </summary>
    private void HalePlayerAllActiveOnTimerEnd()
    {

        if (FindHalePlayers().Count > 0)
        {
            InGameStatus = GameStatus.Start;
            
            var players = Utilities.GetPlayers();
            foreach (var player in players)
            {
                if(player.IsValid)
                {
                    if (BaseHalePlayers[player.Slot].IsHale)
                    {
                        var playerPawn = player.PlayerPawn.Value;
                        if (playerPawn != null && playerPawn.IsValid)
                            player.SetMoveType(MoveType_t.MOVETYPE_WALK);
                        player.PrintToCenter("라운드가 시작했습니다. 모든 인간 진영을 처치하세요!");
                    }
                    else
                        player.PrintToCenter("헤일이 활동하기 시작했습니다. 헤일을 처치하거나 라운드 시간 동안 생존하세요!");
                }
            }
        }

    }

    /// <summary>
    /// 헤일 플레이어가 높이 점프 하기 전에 행동과 상태를 체크합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    private void SuperJumpOnPostThinkPost(CCSPlayerController player)
    {
        var slot = player.Slot;
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn is not null &&
            player.PawnIsAlive &&
            (InGameStatus == GameStatus.Start || InGameStatus == GameStatus.End) &&
            !BaseGamePlayers[slot].IsStunned() &&
            BaseHalePlayers[slot].IsHale &&
            BaseHalePlayers[slot].MyHale!.CanUseSuperJump &&
            BaseHalePlayers[slot].SuperJumpReady)
        {
            var angles = playerPawn.EyeAngles;

            //  위를 올려다 보고 앉기를 누르고 있거나, 우클릭(2번째 공격)을 하고 있을 때 (헤일 높이 점프 대기)
            if (((PlayerButtons.Duck & player.Buttons) != 0 && angles.X < BaseHale.SuperJumpAngleXRange) || (PlayerButtons.Attack2 & player.Buttons) != 0)
            {
                if (!BaseHalePlayers[slot].DoSuperJumpHold)
                {
                    BaseHalePlayers[slot].DoSuperJumpHold = true;

                    BaseHalePlayers[slot].SuperJumpHoldTicks = Server.CurrentTime;
                    BaseHalePlayers[slot].SuperJumpHoldStartTicks = Server.CurrentTime;
                    
                    //  프로그래스바 생성
                    player.SetProgressBar((int) BaseHale.SuperJumpMaximumHoldTime);
                }

                if (BaseHalePlayers[slot].SuperJumpHoldTicks - BaseHalePlayers[slot].SuperJumpHoldStartTicks < BaseHale.SuperJumpMaximumHoldTime)
                    BaseHalePlayers[slot].SuperJumpHoldTicks = Server.CurrentTime;
                
                if (BaseGamePlayers[slot].DebugModeType == DebugType.HalePlayer)
                    player.PrintToCenterAlert($"Dynamic Jump Hold Time: {(BaseHalePlayers[slot].SuperJumpHoldTicks - BaseHalePlayers[slot].SuperJumpHoldStartTicks):F2} Tick(s)");
            }
            //  헤일이 높이 점프 대기상태가 아니고, DoDynamicJumpHold(점프 홀드 여부)가 true 일 때
            else if (BaseHalePlayers[slot].DoSuperJumpHold)
            {
                BaseHalePlayers[slot].DoSuperJumpHold = false;
                
                //  프로그래스바 삭제
                player.SetProgressBar();

                var holdTime = BaseHalePlayers[slot].SuperJumpHoldTicks - BaseHalePlayers[slot].SuperJumpHoldStartTicks;
                
                if (angles.X < BaseHale.SuperJumpAngleXRange && 
                    holdTime > BaseHale.SuperJumpMinimumHoldTime)
                {
                    BaseHalePlayers[slot].SuperJumpReady = false;
                    OnHalePlayerSuperJump(player, holdTime, BaseHalePlayers[slot].MyHale!.SuperJumpVectorScale);
                }
            }
        }
    }

    /// <summary>
    /// 높이 점프
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="holdTime">점프 홀드 시간</param>
    /// <param name="vectorScale">점프 벡터 크기</param>
    private void OnHalePlayerSuperJump(CCSPlayerController player, float holdTime, float vectorScale)
    {
        var playerPawn = player.PlayerPawn.Value;
        var slot = player.Slot;
        var hale = BaseHalePlayers[slot].MyHale;
        if (playerPawn is null || hale is null || !BaseHalePlayers[slot].IsHale)
            return;

        var charge = holdTime * 100f;
        if (charge > 100f)
            charge = 100f;

        var angles = playerPawn.EyeAngles;
        var velocity = new Vector(playerPawn.Velocity.Handle);

        velocity.X += MathF.Cos(float.DegreesToRadians(angles.X)) * MathF.Cos(float.DegreesToRadians(angles.Y)) * 500 * vectorScale;
        velocity.Y += MathF.Cos(float.DegreesToRadians(angles.X)) * MathF.Sin(float.DegreesToRadians(angles.Y)) * 500 * vectorScale;
        velocity.Z = (750f + 175f + charge / 70f) * vectorScale;
        
        if (BaseGamePlayers[slot].DebugModeType == DebugType.HalePlayer)
            player.PrintToChat($"[FS2 Debugger] Dynamic Jump Velocity: {velocity}");
        
        playerPawn.Teleport(null, null, velocity);

        BaseHalePlayers[slot].MyHale!.EmitJumpSound();
        
        //  쿨타임 생성
        BaseHalePlayers[slot].CreateSuperJumpCooldown();
    }

    /// <summary>
    /// 헤일 내려찍기
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    private void WeightDownOnPostThinkPost(CCSPlayerController player)
    {
        var slot = player.Slot;
        if (player.IsValid && 
            player.PawnIsAlive && 
            !player.IsBot && 
            (InGameStatus == GameStatus.Start || InGameStatus == GameStatus.End) &&
            BaseHalePlayers[slot].IsHale &&
            BaseHalePlayers[slot].MyHale!.CanUseWeightDown &&
            !BaseGamePlayers[slot].IsStunned())
        {
            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn == null || !playerPawn.IsValid)
                return;
            
            if ((playerPawn.Flags & (1 << 0)) == 0)
            {
                var eyeAngles = playerPawn.EyeAngles;

                if ((PlayerButtons.Duck & player.Buttons) != 0 && eyeAngles.X > BaseHale.WeightDownAngleXRange)
                {
                    if (BaseHalePlayers[slot].WeightDownReady)
                    {
                        BaseHalePlayers[slot].WeightDownReady = false;

                        BaseHalePlayers[slot].CreateWeightDownCooldown();
                        
                        var velocity = playerPawn.AbsVelocity;
                        velocity.Z = BaseHale.WeightDownZVelocity;
                        playerPawn.Teleport(null, null, velocity);

                        playerPawn.GravityScale = BaseHale.WeightDownGravityScale;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 헤일 플레이어 보조 공격 차단
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    private void HalePlayerSecondaryAttackBlockOnPostThinkPost(CCSPlayerController player)
    {
        if (player.PawnIsAlive && BaseHalePlayers[player.Slot].IsHale)
        {
            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn != null && playerPawn.IsValid)
            {
                if (playerPawn.WeaponServices != null)
                {
                    var activeWeapon = playerPawn.WeaponServices.ActiveWeapon.Value;
                    if (activeWeapon != null && activeWeapon.GetDesignerNameEx().Contains("knife"))
                        activeWeapon.SetWeaponNextSecondaryAttackTick(Server.TickCount + 99999);
                }
            }
        }
    }

    private void HalePlayerRageChargeOnPlayerHurt(CCSPlayerController victim, CCSPlayerController attacker, int damage)
    {
        if (BaseHalePlayers[victim.Slot].IsHale && !BaseHalePlayers[attacker.Slot].IsHale && BaseHalePlayers[victim.Slot].MyHale!.CanUseRage && damage > 0)
            BaseHalePlayers[victim.Slot].Rage += BaseHalePlayers[victim.Slot].MyHale!.GetRageRangeByTakeDamage(damage);
    }

    /// <summary>
    /// 헤일 이름으로 헤일을 찾습니다.
    /// </summary>
    /// <param name="name">헤일 이름</param>
    /// <returns>헤일</returns>
    private BaseHale? FindHaleByDesignerName(string name) =>
        Hales.Where(hale => hale.DesignerName == name)
            .FirstOrDefault();

    /// <summary>
    /// 헤일 플레이어를 찾습니다.
    /// </summary>
    /// <returns>헤일 플레이어 (없으면 빈 List 반환)</returns>
    private List<CCSPlayerController> FindHalePlayers()
    {
        var players = new List<CCSPlayerController>();
        foreach (var baseHalePlayer in BaseHalePlayers)
        {
            var player = Utilities.GetPlayerFromSlot(baseHalePlayer.Key);
            if(player != null && player.IsValid && player.PawnIsAlive && baseHalePlayer.Value.IsHale)
                players.Add(player);
        }

        return players;
    }
}
