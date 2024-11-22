using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Classes;
using FreakStrike2.Models;
using FreakStrike2.Utils;
using Microsoft.Extensions.Logging;

namespace FreakStrike2;
public partial class FreakStrike2
{
    public static string PluginConfigDirectory = "csgo\\addons\\counterstrikesharp\\configs\\plugins\\FreakStrike2\\";
    public static string HaleConfigFilename = "playable_hales.json";
    
    /// <summary>
    /// 헤일 설정 파일을 읽어옵니다.
    /// </summary>
    /// <param name="hotReload">핫리로드 유무</param>
    private void GetHaleJsonOnLoad(bool hotReload)
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

    private void RemoveAllHalePlayers() =>
        Utilities.GetPlayers().Where(player => player.IsValid)
            .ToList()
            .ForEach(player => BaseHalePlayers[player.Slot].Remove());

    /// <summary>
    /// 타이머가 종료되는 시점에서 무작위(또는 Queuepoint 가 높은 플레이어)로 헤일 선택
    /// </summary>
    private void SetHalePlayerOnTimerEnd()
    {
        InGameStatus = GameStatus.Start;
        
        var player = BaseQueuePoint.GetPlayerWithMostQueuepoints(PlayerQueuePoints) ?? PlayerUtils.GetRandomAlivePlayer();

        if (player is null)
        {
            Logger.LogError("[FreakStrike2] No player has been selected.");
            return;
        }

        var hale = CommonUtils.GetRandomInList(Hales);
        BaseHalePlayers[player.Slot] = new BaseHalePlayer(player, hale, Config.HaleTeleportToSpawn);
        PlayerQueuePoints[player.Slot].Points = 0;
        
        ServerUtils.PrintToCenterAlertAll($"{player.PlayerName} 이(가) {hale.Name} 헤일로 선택 되었습니다!");
        Logger.LogInformation($"[FreakStrike2] {player.PlayerName}({player.AuthorizedSteamID!.SteamId64}) has been chosen as the Hale for {hale.Name}!");
    }

    /// <summary>
    /// 헤일 플레이어가 높이 점프 하기 전에 행동과 상태를 체크합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    private void DynamicJumpOnPostThinkPost(CCSPlayerController player)
    {
        var slot = player.Slot;
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn is not null &&
            player.PawnIsAlive &&
            BaseHalePlayers[slot].IsHale &&
            BaseHalePlayers[slot].MyHale!.CanUseDynamicJump &&
            BaseHalePlayers[slot].DynamicJumpReady)
        {
            var angles = playerPawn.EyeAngles;

            //  위를 올려다 보고 앉기를 누르고 있거나, 우클릭(2번째 공격)을 하고 있을 때 (헤일 높이 점프 대기)
            if (((PlayerButtons.Duck & player.Buttons) != 0 && angles.X < BaseHale.DynamicJumpAngleXRange) || 
                (PlayerButtons.Attack2 & player.Buttons) != 0)
            {
                //  스턴 상태일 경우 무시
                if (BaseHalePlayers[slot].IsStun)
                    return;

                if (!BaseHalePlayers[slot].DoDynamicJumpHold)
                {
                    BaseHalePlayers[slot].DoDynamicJumpHold = true;

                    BaseHalePlayers[slot].DynamicJumpHoldTicks = Server.CurrentTime;
                    BaseHalePlayers[slot].DynamicJumpHoldStartTicks = Server.CurrentTime;
                    
                    //  프로그래스바 생성
                    playerPawn.ProgressBarStartTime = (int) BaseHale.DynamicJumpMaximumHoldTime;
                }

                if (BaseHalePlayers[slot].DynamicJumpHoldTicks - BaseHalePlayers[slot].DynamicJumpHoldStartTicks < BaseHale.DynamicJumpMaximumHoldTime)
                    BaseHalePlayers[slot].DynamicJumpHoldTicks = Server.CurrentTime;
                
                if (BaseGamePlayers[slot].DebugMode)
                    player.PrintToCenterAlert($"Dynamic Jump Hold Time: {(BaseHalePlayers[slot].DynamicJumpHoldTicks - BaseHalePlayers[slot].DynamicJumpHoldStartTicks):F1} Tick(s)");
            }
            //  헤일이 높이 점프 대기상태가 아니고, DoDynamicJumpHold(점프 홀드 여부)가 true 일 때
            else if (BaseHalePlayers[slot].DoDynamicJumpHold)
            {
                BaseHalePlayers[slot].DoDynamicJumpHold = false;
                
                //  프로그래스바 삭제
                playerPawn.ProgressBarStartTime = 0;

                var holdTime = BaseHalePlayers[slot].DynamicJumpHoldTicks - BaseHalePlayers[slot].DynamicJumpHoldStartTicks;
                
                if (angles.X < BaseHale.DynamicJumpAngleXRange && 
                    holdTime > BaseHale.DynamicJumpMinimumHoldTime)
                {
                    BaseHalePlayers[slot].DynamicJumpReady = false;
                    OnHalePlayerDynamicJump(player, holdTime, BaseHalePlayers[slot].MyHale!.DynamicJumpVectorScale);
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
    private void OnHalePlayerDynamicJump(CCSPlayerController player, float holdTime, float vectorScale)
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
        
        if (BaseGamePlayers[slot].DebugMode)
            player.PrintToChat($"[FS2 Debugger] Dynamic Jump Velocity: {velocity}");
        
        player.Teleport(null, null, velocity);

        BaseHalePlayers[slot].MyHale!.EmitJumpSound();
        
        //  쿨타임 생성
        BaseHalePlayers[slot].DynamicJumpCooldown = hale.DynamicJumpCooldown;
        BaseHalePlayers[slot].DynamicJumpCooldownTimer = AddTimer(
            0.1f, 
            BaseHalePlayers[slot].DynamicJumpCooldownCallback(player, InGameStatus, BaseGamePlayers[slot].DebugMode), 
            TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
    }
}
