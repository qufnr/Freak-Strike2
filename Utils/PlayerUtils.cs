using System.Drawing;
using System.Text.RegularExpressions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.UserMessages;
using CounterStrikeSharp.API.Modules.Utils;

namespace FreakStrike2.Utils;

public class PlayerUtils
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
    public static bool PlayerIsConsole(CCSPlayerController? player) => player is null || player.Slot <= 0;

    /// <summary>
    /// 유효한 플레이어들을 반환합니다.
    /// </summary>
    /// <returns>유효한 플레이어들</returns>
    public static List<CCSPlayerController> FindValidPlayers() =>
        Utilities.GetPlayers().Where(p => p.IsValid).ToList();
    
    /// <summary>
    /// 살아있는 플레이어들을 반환합니다.
    /// </summary>
    /// <returns>살아있는 플레이어들</returns>
    public static List<CCSPlayerController> FindValidAndPawnAlivePlayers() => 
        Utilities.GetPlayers().Where(p => p.IsValid && p.PawnIsAlive).ToList();

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
        FindValidAndPawnAlivePlayers().ForEach(player =>
        {
            if(player.IsValid && player.PawnIsAlive)
                pawnAlivePlayers.Add(player);
        });
        
        return pawnAlivePlayers.Count > 0 ? CommonUtils.GetRandomInList(pawnAlivePlayers) : null;
    }
    
    /// <summary>
    /// 특정한 팀에서 살아있는 플레이어 수를 반환합니다.
    /// </summary>
    /// <param name="team">팀</param>
    /// <returns>팀에 살아있는 플레이어 수</returns>
    public static int GetTeamAlivePlayers(CsTeam team)
    {
        return Utilities.GetPlayers()
            .Where(player => player.IsValid && player.PawnIsAlive && player.Team == team)
            .Count();
    }

    /// <summary>
    /// 시점 위치 반환
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <returns>시점 위치 벡터</returns>
    public static Vector? GetPlayerEyePosition(CCSPlayerController player)
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
    public static CCSPlayerController? GetPlayerTarget(CCSPlayerController player)
    {
        var gameRules = CommonUtils.GetGameRules();
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
    public static void SetColorScreen(CCSPlayerController player, Color color, float hold = .1f, float fade = .2f, ScreenFadeFlags screenFadeFlags = ScreenFadeFlags.FadeIn, bool withPurge = true)
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
    public static void SetPlayerModelSize(CCSPlayerController player, float scale)
    {
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn is null)
            return;
        playerPawn.CBodyComponent!.SceneNode!.Scale = scale;
        Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_CBodyComponent");
    }

    /// <summary>
    /// 플레이어 움직임 유형을 설정합니다.
    /// </summary>
    /// <param name="player">플레이어 Pawn 객체</param>
    /// <param name="moveType">움직임 유형</param>
    public static void SetPlayerMoveType(CCSPlayerPawn player, MoveType_t moveType)
    {
        player.MoveType = moveType;
        player.ActualMoveType = moveType;
        Utilities.SetStateChanged(player, "CBaseEntity", "m_MoveType");
    }

    /// <summary>
    /// 플레이어에게 프로그래스바를 생성합니다. (대테러부대일 경우 해체 UI, 테러리스트일 경우 폭탄설치 UI로 표시됩니다.)
    /// </summary>
    /// <param name="player">플레이어 Pawn 객체</param>
    /// <param name="duration">시간 (0으로 설정 시 프로그래스바 삭제)</param>
    public static void SetPlayerProgressBar(CCSPlayerPawn player, int duration = 0)
    {
        player.ProgressBarDuration = duration > 0 ? duration : 0;
        player.ProgressBarStartTime = duration > 0 ? Server.CurrentTime : 0;
        Utilities.SetStateChanged(player, "CCSPlayerPawnBase", "m_iProgressBarDuration");
        Utilities.SetStateChanged(player, "CCSPlayerPawnBase", "m_flProgressBarStartTime");
    }

    /// <summary>
    /// 플레이어에게 헬멧을 장착합니다.
    /// </summary>
    /// <param name="player">플레이어 Pawn 객체</param>
    public static void SetPlayerHelmet(CCSPlayerPawn player)
    {
        var itemServices = player.ItemServices;
        if (itemServices != null)
        {
            CCSPlayer_ItemServices services = new(itemServices.Handle);
            services.HasHelmet = true;
            Utilities.SetStateChanged(player, "CBasePlayerPawn", "m_pItemServices");
        }
    }

    /// <summary>
    /// 플레이어의 인-게임 자금을 설정합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="amount">자금</param>
    public static void SetPlayerMoney(CCSPlayerController player, int amount)
    {
        var inGameMoneyServices = player.InGameMoneyServices;
        if (inGameMoneyServices != null)
        {
            inGameMoneyServices.Account = amount;
            Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");
        }
    }

    /// <summary>
    /// 플레이어를 스폰지점으로 텔레포트합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="team">팀</param>
    /// <exception cref="Exception">팀이 유효하지 않습니다.</exception>
    public static void TeleportToSpawnPoint(CCSPlayerController player, CsTeam team)
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
                playerPawn.Teleport(new Vector() { X = candidate.AbsOrigin.X, Y = candidate.AbsOrigin.Y + 1f, Z = candidate.AbsOrigin.Z });
        }
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
                return Utilities.GetPlayerFromUserid(userId);
        }

        return Utilities.GetPlayers().Where(player => containsName 
                ? player.PlayerName.Contains(val) 
                : player.PlayerName.Equals(val))
            .FirstOrDefault();
    }
}