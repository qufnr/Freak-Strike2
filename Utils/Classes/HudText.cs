using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Utils.Exceptions;

namespace FreakStrike2.Utils.Classes;

public class HudText
{
    public CPointWorldText? Entity { get; private set; }
    public CCSPlayerController? Target { get; private set; }

    private HudText(CCSPlayerController? player, string message, HudTextAttribute attribute)
    {
        var playerPawn = player?.PlayerPawn.Value;
        var playerCameraService = playerPawn?.CameraServices;
        if (player is null || playerPawn is null || playerCameraService is null)
            throw new Exception("플레이어 객체가 잘못 되었습니다.");

        Target = player;
        Entity = Utilities.CreateEntityByName<CPointWorldText>("point_worldtext");
        if (Entity is null)
            throw new GameNotSupportedException();
        
        var vmHandle = playerPawn.ViewModelServices!.Handle;
        if (vmHandle == IntPtr.Zero)
            throw new Exception("ViewModelServices.Handle is null.");

        SetText(message);
        Entity.Enabled = true;
        SetAttribute(attribute);

        Vector forward = new(), right = new(), up = new();
        NativeAPI.AngleVectors(playerPawn.EyeAngles.Handle, forward.Handle, right.Handle, up.Handle);

        var viewOffsetZ = playerCameraService.OldPlayerViewOffsetZ;

        Vector eyePosition = new();
        eyePosition += forward * 50;
        eyePosition += right * -40;
        eyePosition += up * (10 + viewOffsetZ);

        QAngle angles = new();
        angles.X = 0;
        angles.Y = playerPawn.EyeAngles.Y + 270;
        angles.Z = playerPawn.EyeAngles.Z + 90;
        
        Entity.Teleport(playerPawn.AbsOrigin! + eyePosition, angles, new Vector(0, 0, 0));
        Entity.DispatchSpawn();
        
        CCSPlayer_ViewModelServices vmServices = new(vmHandle);
        var vmOffset = vmServices.Handle + Schema.GetSchemaOffset("CCSPlayer_ViewModelServices", "m_hViewModel");
        var viewmodels = MemoryMarshal.CreateSpan(ref vmOffset, 3);
        CHandle<CBaseViewModel> viewmodel = new(viewmodels[0]);
        
        Entity.AcceptInput("SetParent", playerPawn, null, "!activator");
        Entity.AcceptInput("FollowEntity", viewmodel.Value, Entity, "!activator");
        // Entity.AcceptInput("SetParentAttachmentMaintainOffset", playerPawn, null, "axis_of_intent");
    }

    /// <summary>
    /// 플레이어에게 텍스트를 출력합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <param name="text">텍스트 내용</param>
    /// <param name="attribute">텍스트 속성</param>
    /// <returns>HudText 객체</returns>
    public static HudText Print(CCSPlayerController player, string text, HudTextAttribute? attribute)
    {
        if (attribute is null)
        {
            attribute = new HudTextAttribute();
        }

        return new HudText(player, text, attribute);
    }

    /// <summary>
    /// 모든 플레이어에게 텍스트를 출력합니다.
    /// </summary>
    /// <remarks>
    /// 봇과 SourceTV 를 제외한 모든 플레이어에게 출력됩니다.  
    /// 모든 플레이어에게 텍스트 객체(HudText)를 생성하므로, 플레이어 마다의 HudText 객체는 반환받지 않습니다.  
    /// HudTextAttribute.Duration 을 무한(0.0f)으로 설정할 수 없습니다. 이는 단지 "일정 시간 동안 모든 플레이어에게" 표시하기 위해 사용되는 메소드입니다.
    /// </remarks>
    /// <param name="text">텍스트 내용</param>
    /// <param name="attribute">텍스트 속성</param>
    public static void PrintToAll(string text, HudTextAttribute? attribute)
    {
        var players = Utilities.GetPlayers();
        foreach (var player in players)
        {
            if (player.IsValid && !player.IsBot && !player.IsHLTV)
            {
                if (attribute!.Duration <= .0f)
                {
                    attribute.Duration = 5.0f;
                }
                Print(player, text, attribute);
            }
        }
    }

    public void SetText(string text)
    {
        if (Entity is not null && Entity.IsValid)
        {
            Entity.MessageText = text;
        }
    }

    public void SetAttribute(HudTextAttribute attribute)
    {
        if (Entity is not null && Entity.IsValid)
        {
            Entity.Fullbright = true;
            Entity.DepthOffset = .0f;
            Entity.FontSize = attribute.FontSize;
            Entity.Color = attribute.Color;
            Entity.WorldUnitsPerPx = attribute.Scale;
            Entity.JustifyHorizontal = attribute.JustifyHorizontal;
            Entity.JustifyVertical = attribute.JustifyVertical;
            Entity.ReorientMode = attribute.PeorientMode;
            if (attribute.Duration > 0.0)
            {
                //  TODO :: HOW?????? `SetVariantString("OnUser1 !self:kill::1.0:1");` in CSSharp?!
                Entity.AcceptInput("AddOutput", Entity, null, $"OnUser1 !self:kill::{attribute.Duration}:1");   //  ??
                Entity.AcceptInput("FireUser1");
            }
        }
    }

    /// <summary>
    /// 엔티티 삭제
    /// </summary>
    /// <returns>엔티티를 삭제 했다면 true, 아니면 false 반환</returns>
    public bool Remove()
    {
        if (Entity is not null && Entity.IsValid)
        {
            Entity.Remove();
            return true;
        }

        return false;
    }
}