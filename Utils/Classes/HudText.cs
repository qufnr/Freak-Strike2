using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Utils.Exceptions;

namespace FreakStrike2.Utils.Classes;

public class HudText
{
    public CPointWorldText? Entity { get; private set; }
    public CCSPlayerController? Target { get; private set; }

    public HudText(CCSPlayerController? player, string message, HudTextAttribute attribute)
    {
        var playerPawn = player?.PlayerPawn.Value;
        var playerCameraService = playerPawn?.CameraServices;
        if (player is null || playerPawn is null || playerCameraService is null)
        {
            throw new Exception("플레이어 객체가 잘못 되었습니다.");
        }

        Target = player;
        Entity = Utilities.CreateEntityByName<CPointWorldText>("point_worldtext");
        if (Entity is null)
        {
            throw new GameNotSupportedException();
        }

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
        
        Entity.AcceptInput("SetParent", playerPawn, null, "!activator");

        var viewmodelServices = (CCSPlayer_ViewModelServices) player.PlayerPawn.Value!.ViewModelServices!;
        if (viewmodelServices.GetType() == typeof(CCSPlayer_ViewModelServices))
        {
            var viewmodel = viewmodelServices.ViewModel[0].Value;
            Entity.AcceptInput("FollowEntity", viewmodel, Entity, "!activator");
        }
        // Entity.AcceptInput("SetParentAttachmentMaintainOffset", playerPawn, null, "axis_of_intent");
    }

    public void SetText(string text)
    {
        if (Entity is not null)
        {
            Entity.MessageText = text;
        }
    }

    public void SetAttribute(HudTextAttribute attribute)
    {
        if (Entity is not null)
        {
            Entity.Fullbright = true;
            Entity.DepthOffset = .0f;
            Entity.FontSize = attribute.FontSize;
            Entity.Color = attribute.Color;
            Entity.WorldUnitsPerPx = attribute.Scale;
            Entity.JustifyHorizontal = attribute.JustifyHorizontal;
            Entity.JustifyVertical = attribute.JustifyVertical;
            Entity.ReorientMode = attribute.PeorientMode;
        }
    }

    /// <summary>
    /// 엔티티 삭제
    /// </summary>
    /// <returns>엔티티를 삭제 했다면 true, 아니면 false 반환</returns>
    public bool Remove()
    {
        if (Entity is not null)
        {
            Entity.Remove();
            return true;
        }

        return false;
    }
}