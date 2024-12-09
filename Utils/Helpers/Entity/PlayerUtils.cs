﻿using System.Drawing;
using System.Text.RegularExpressions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.UserMessages;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Utils.Helpers.Server;

namespace FreakStrike2.Utils.Helpers.Entity;

public static class PlayerUtils
{
    public enum ScreenFadeFlags
    {
        FadeIn,
        FadeOut,
        FadeStayout
    };
    
    /// <summary>
    /// 플레이어가 콘솔인지 유무를 반환합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <returns>콘솔이면 true, 아니면 false 반환</returns>
    public static bool IsConsole(this CCSPlayerController? player) => player is null || player.Slot <= 0;

    /// <summary>
    /// 유효한 플레이어들을 반환합니다.
    /// </summary>
    /// <returns>유효한 플레이어들</returns>
    public static List<CCSPlayerController> FindValidPlayers() => Utilities.GetPlayers().Where(p => p.IsValid).ToList();
    
    /// <summary>
    /// 살아있는 플레이어들을 반환합니다.
    /// </summary>
    /// <returns>살아있는 플레이어들</returns>
    public static List<CCSPlayerController> FindValidAndPawnAlivePlayers() => Utilities.GetPlayers().Where(p => p.IsValid && p.PawnIsAlive).ToList();

    /// <summary>
    /// Fake Client 를 제외한 유효 플레이어들을 반환합니다.
    /// </summary>
    /// <returns>유효한 플레이어들</returns>
    public static List<CCSPlayerController> FindPlayersWithoutFakeClient() => Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot && !p.IsHLTV).ToList();

    /// <summary>
    /// 살아있는 플레이어 중에서 무작위로 한 명을 뽑습니다.
    /// </summary>
    /// <remarks>
    /// 이는 null 을 반환할 수 있습니다.
    /// </remarks>
    /// <returns>무작위로 뽑힌 한 명의 플레이어</returns>
    public static CCSPlayerController? GetRandomAlivePlayer()
    {
        var pawnAlivePlayers = new List<CCSPlayerController>();
        foreach (var player in FindValidAndPawnAlivePlayers())
            pawnAlivePlayers.Add(player);
        
        return pawnAlivePlayers.Count > 0 ? CommonUtils.GetRandomInList(pawnAlivePlayers) : null;
    }
    
    /// <summary>
    /// 특정한 팀에서 살아있는 플레이어 수를 반환합니다.
    /// </summary>
    /// <param name="team">팀</param>
    /// <returns>팀에 살아있는 플레이어 수</returns>
    public static int GetTeamAlivePlayers(CsTeam team) => Utilities.GetPlayers()
            .Where(player => player.IsValid && player.PawnIsAlive && player.Team == team)
            .Count();

    /// <summary>
    /// 특정한 팀의 플레이어 수를 반환합니다.
    /// </summary>
    /// <param name="team">팀</param>
    /// <returns>팀의 플레이어 수</returns>
    public static int GetTeamPlayers(CsTeam team) => Utilities.GetPlayers()
        .Where(player => player.IsValid && player.Team == team)
        .Count();

    /// <summary>
    /// 시점 위치 반환
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <returns>시점 위치 벡터</returns>
    public static Vector? GetEyePosition(this CCSPlayerController player)
    {
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn is null)
            return null;
        
        var origin = playerPawn.AbsOrigin;
        var cameraServices = playerPawn.CameraServices;
        if (origin is null || cameraServices is null)
            return null;
        
        return new Vector(origin.X, origin.Y, origin.Z + cameraServices.OldPlayerViewOffsetZ);
    }

    /// <summary>
    /// 플레이어가 조준하고 있는 상대 플레이어 객체를 반환합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <returns>조준 대상 플레이어 객체</returns>
    /// <remarks>
    /// 참고 자료: https://discord.com/channels/1160907911501991946/1175947333880524962/1230542480903110716
    /// </remarks>
    public static CCSPlayerController? GetTarget(this CCSPlayerController player)
    {
        var gameRules = ServerUtils.GameRules;
        VirtualFunctionWithReturn<IntPtr, IntPtr, IntPtr> findPickerEntity = new(gameRules.Handle, 28);
        var target = new CBaseEntity(findPickerEntity.Invoke(gameRules.Handle, player.Handle));
        return target.DesignerName is "player" ? target.As<CCSPlayerController>().OriginalControllerOfCurrentPawn.Value : null;
    }

    /// <summary>
    /// 화면 색상 효과를 입힙니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="color">색상</param>
    /// <param name="hold">유지 시간</param>
    /// <param name="fade">페이딩 시간</param>
    /// <param name="screenFadeFlags">페이드 플레그</param>
    /// <param name="withPurge">제거 효과 유무</param>
    /// <remarks>
    /// 참고 자료: https://discord.com/channels/1160907911501991946/1175947333880524962/1277984855384260762
    /// </remarks>
    public static void SetColorScreen(this CCSPlayerController player, Color color, float hold = .1f, float fade = .2f, ScreenFadeFlags screenFadeFlags = ScreenFadeFlags.FadeIn, bool withPurge = true)
    {
        var userMessage = UserMessage.FromId(106);
        userMessage.SetInt("hold_time", Convert.ToInt32(hold * 512));
        userMessage.SetInt("duration", Convert.ToInt32(fade * 512));
        userMessage.SetInt("color", color.R | color.G << 8 | color.B << 16 | color.A << 24);
        var flags = screenFadeFlags switch
        {
            ScreenFadeFlags.FadeIn => 0x0001,
            ScreenFadeFlags.FadeOut => 0x0002,
            ScreenFadeFlags.FadeStayout => 0x0008,
            _ => 0x0001
        };
        userMessage.SetInt("flags", withPurge ? flags | 0x0010 : flags);
        userMessage.Send(player);
    }

    /// <summary>
    /// 플레이어 모델 크기를 변경합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="scale">크기</param>
    /// <remarks>
    /// 참고 자료: https://discord.com/channels/1160907911501991946/1175947333880524962/1237721748376518666
    /// </remarks>
    public static void SetModelSize(this CCSPlayerController player, float scale)
    {
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn is null)
            return;
        playerPawn.CBodyComponent!.SceneNode!.Scale = scale;
        CounterStrikeSharp.API.Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_CBodyComponent");
    }

    /// <summary>
    /// 플레이어 움직임 유형을 설정합니다.
    /// </summary>
    /// <param name="player">플레이어 Pawn 객체</param>
    /// <param name="moveType">움직임 유형</param>
    public static void SetMoveType(this CCSPlayerController player, MoveType_t moveType)
    {
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn == null || !playerPawn.IsValid)
            return;
        
        playerPawn.MoveType = moveType;
        playerPawn.ActualMoveType = moveType;
        CounterStrikeSharp.API.Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_MoveType");
    }

    /// <summary>
    /// 플레이어에게 프로그래스바를 생성합니다. (대테러부대일 경우 해체 UI, 테러리스트일 경우 폭탄설치 UI로 표시됩니다.)
    /// </summary>
    /// <param name="player">플레이어 Pawn 객체</param>
    /// <param name="duration">시간 (0으로 설정 시 프로그래스바 삭제)</param>
    public static void SetProgressBar(this CCSPlayerController player, int duration = 0)
    {
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn == null || !playerPawn.IsValid)
            return;
        
        playerPawn.ProgressBarDuration = duration > 0 ? duration : 0;
        playerPawn.ProgressBarStartTime = duration > 0 ? CounterStrikeSharp.API.Server.CurrentTime : 0;
        CounterStrikeSharp.API.Utilities.SetStateChanged(playerPawn, "CCSPlayerPawnBase", "m_iProgressBarDuration");
        CounterStrikeSharp.API.Utilities.SetStateChanged(playerPawn, "CCSPlayerPawnBase", "m_flProgressBarStartTime");
    }

    /// <summary>
    /// 플레이어에게 헬멧을 장착합니다.
    /// </summary>
    /// <param name="player">플레이어 Pawn 객체</param>
    /// <param name="helmet">헬멧</param>
    /// <param name="heavy">헤비 헬멧</param>
    public static void SetHelmet(this CCSPlayerController player, bool helmet = false, bool heavy = false)
    {
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn == null)
            return;
        
        var itemServices = playerPawn.ItemServices;
        if (itemServices != null)
        {
            CCSPlayer_ItemServices services = new(itemServices.Handle);
            services.HasHelmet = helmet;
            services.HasHeavyArmor = heavy;
            CounterStrikeSharp.API.Utilities.SetStateChanged(playerPawn, "CBasePlayerPawn", "m_pItemServices");
        }
    }

    /// <summary>
    /// 플레이어 방어 값을 설정합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="value">방어력</param>
    public static void SetArmorValue(this CCSPlayerController player, int value)
    {
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn == null)
            return;

        playerPawn.ArmorValue = value;
        CounterStrikeSharp.API.Utilities.SetStateChanged(playerPawn, "CCSPlayerPawn", "m_ArmorValue");
    }

    /// <summary>
    /// 체력을 설정합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="health">체력</param>
    /// <param name="withMaxHealth">최대 체력과 함께 변경</param>
    public static void SetHealth(this CCSPlayerController player, int health, bool withMaxHealth = false)
    {
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn == null)
            return;

        playerPawn.Health = health;

        if (withMaxHealth && playerPawn.Health > playerPawn.MaxHealth)
            playerPawn.MaxHealth = health;

        CounterStrikeSharp.API.Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
    }

    /// <summary>
    /// 플레이어의 움직임 속도를 설정합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="velocity">속도 수치</param>
    public static void SetMovementSpeed(this CCSPlayerController player, float velocity)
    {
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn == null)
            return;

        playerPawn.VelocityModifier = velocity;
        // Utilities.SetStateChanged(playerPawn, "CCSPlayerPawn", "m_flVelocityModifier");
    }

    /// <summary>
    /// 플레이어의 인-게임 자금을 설정합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="amount">자금</param>
    public static void SetMoney(this CCSPlayerController player, int amount)
    {
        var inGameMoneyServices = player.InGameMoneyServices;
        if (inGameMoneyServices != null)
        {
            inGameMoneyServices.Account = amount;
            CounterStrikeSharp.API.Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");
        }
    }

    /// <summary>
    /// 플레이어를 스폰지점으로 텔레포트합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="team">팀</param>
    /// <param name="killAngles">앵글 초기화</param>
    /// <exception cref="Exception">팀이 유효하지 않습니다.</exception>
    public static void TeleportToSpawnPoint(this CCSPlayerController player, CsTeam team, bool killAngles = true)
    {
        var playerPawn = player.PlayerPawn.Value;
        
        if (playerPawn == null)
            return;

        var entityName = team == CsTeam.CounterTerrorist
            ? "info_player_counterterrorist"
            : team == CsTeam.Terrorist 
                ? "info_player_terrorist" 
                : throw new Exception("Team is invalid!");
        
        var spawnpoints = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>(entityName);
        var spawnpointEntities = new List<SpawnPoint>();
        foreach (var spawnpoint in spawnpoints)
            if (spawnpoint.IsValid)
                spawnpointEntities.Add(spawnpoint);

        if (spawnpointEntities.Count > 0)
        {
            var candidate = CommonUtils.GetRandomInList(spawnpointEntities);
            if (candidate.IsValid && candidate.AbsOrigin != null)
                playerPawn.Teleport(new Vector() { X = candidate.AbsOrigin.X, Y = candidate.AbsOrigin.Y + 1f, Z = candidate.AbsOrigin.Z }, killAngles ? new QAngle(0, 0, 0) : null, new Vector(0, 0, 0));
        }
    }

    /// <summary>
    /// 다음 프레임에 팀을 변경합니다. Switches the team of the player, has the same effect as the "jointeam" console command.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="team">변경할 팀</param>
    /// <param name="task">팀 변경 후 일어날 일</param>
    public static void ChangeTeamOnNextFrame(this CCSPlayerController player, CsTeam team, Action? task = null)
    {
        if (player.TeamNum == (byte)team)
            return;
        
        CounterStrikeSharp.API.Server.NextFrame(() =>
        {
            player.ChangeTeam(team);

            if (task != null)
                task.Invoke();
        });
    }

    public static void SetRenderColour(this CCSPlayerController player, Color colour)
    {
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn == null)
            return;

        playerPawn.Render = colour;
        Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_clrRender");
    }
    
    /// <summary>
    /// 플레이어의 모습을 감춥니다.
    /// </summary>
    /// <remarks>모습을 감췄을 때 아이템을 장착하면 아이템만 보이게 됩니다. 해결 방법으로는 아이템을 주웠을 때 한 번 더 호출합니다. 예) EventItemPickup</remarks>
    /// <param name="player">플레이어 객체</param>
    /// <param name="visible">숨김 여부</param>
    public static void SetVisibility(this CCSPlayerController player, bool visible = true)
    {
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn == null)
            return;

        var colour = Color.FromArgb(visible ? 255 : 0, 255, 255, 255);
        
        player.SetRenderColour(colour);

        var weaponService = playerPawn.WeaponServices;

        if (weaponService != null)
        {
            var activeWeapon = weaponService.ActiveWeapon.Value;
            if (activeWeapon != null && activeWeapon.IsValid)
            {
                activeWeapon.ShadowStrength = 1f;
                activeWeapon.SetRenderColour(colour);
            }

            foreach (var weapon in weaponService.MyWeapons)
            {
                var myWeapon = weapon.Value;
                if (myWeapon != null)
                {
                    myWeapon.ShadowStrength = 1f;
                    myWeapon.SetRenderColour(colour);
                }
            }
        }
    }

    /// <summary>
    /// 플레이어 에임 펀치 수치 설정
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="vector">에임 펀치 수치</param>
    public static void SetAimPunchAngle(this CCSPlayerController player, Vector vector)
    {
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn == null)
            return;

        playerPawn.AimPunchAngle.X = vector.X;
        playerPawn.AimPunchAngle.Y = vector.Y;
        playerPawn.AimPunchAngle.Z = vector.Z;
        Utilities.SetStateChanged(playerPawn, "CCSPlayerPawn", "m_aimPunchAngle");
    }
    
    /// <summary>
    /// #UserId 혹은 플레이어 이름으로 플레이어 객체를 찾습니다.
    /// </summary>
    /// <param name="val">#UserId 또는 플레이어 이름</param>
    /// <param name="containsName">플레이어 이름으로 찾을 때 일부 일치로 할지 전부 일치로 할지 여부</param>
    /// <returns>플레이어 객체 또는 Null</returns>
    public static CCSPlayerController? FindPlayerByNameOrUserId(string val, bool containsName = true)
    {
        var matchUserId = Regex.Match(val, @"^#\d+$"); 
        if (matchUserId.Success)
        {
            int userId;
            if (int.TryParse(matchUserId.Groups[1].Value, out userId))
                return CounterStrikeSharp.API.Utilities.GetPlayerFromUserid(userId);
        }

        return CounterStrikeSharp.API.Utilities.GetPlayers().Where(player => containsName 
                ? player.PlayerName.Contains(val) 
                : player.PlayerName.Equals(val))
            .FirstOrDefault();
    }
}