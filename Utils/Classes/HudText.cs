using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Exceptions;
using FreakStrike2.Utils.Helpers.Entity;

namespace FreakStrike2.Utils.Classes;

public class HudText
{
    public CPointWorldText? Entity { get; private set; }
    public CCSPlayerController Target { get; private set; }

    public HudText(CCSPlayerController player)
    {
        Target = player;
        if (Entity != null && Entity.IsValid)
            Entity.Remove();
        
        Entity = CounterStrikeSharp.API.Utilities.CreateEntityByName<CPointWorldText>("point_worldtext");
        if (Entity == null)
            throw new GameNotSupportedException();
            
        Entity.Enabled = true;
            
        Entity.DispatchSpawn();
        
        UpdatePosition();
    }

    public void SetMessage(string message, HudTextAttribute attribute)
    {
        if (!Target.IsValid || Entity == null || !Entity.IsValid)
            return;
        
        var playerPawn = Target.PlayerPawn.Value;
        var playerCameraService = playerPawn?.CameraServices;
        if (!Target.IsValid || playerPawn == null || !playerPawn.IsValid || playerCameraService == null)
            throw new PlayerNotFoundException();

        if (string.IsNullOrEmpty(message))
            return;

        var vmHandle = playerPawn.ViewModelServices!.Handle;
        if (vmHandle == IntPtr.Zero)
            throw new Exception("ViewModelServices.Handle is null.");
        
        Entity.MessageText = message;
        Utilities.SetStateChanged(Entity, "CPointWorldText", "m_messageText");
        
        Entity.Fullbright = true;
        Entity.DepthOffset = .0f;
        Entity.FontSize = attribute.FontSize;
        Entity.Color = attribute.Color;
        Entity.WorldUnitsPerPx = attribute.Scale;
        Entity.JustifyHorizontal = attribute.JustifyHorizontal;
        Entity.JustifyVertical = attribute.JustifyVertical;
        Entity.ReorientMode = attribute.PeorientMode;
        if (attribute.Duration > 0.0)
            Entity.AddEntityIOEvent("Kill", Entity, null, "", attribute.Duration);
        
        Utilities.SetStateChanged(Entity, "CPointWorldText", "m_bFullbright");
        Utilities.SetStateChanged(Entity, "CPointWorldText", "m_flDepthOffset");
        Utilities.SetStateChanged(Entity, "CPointWorldText", "m_flFontSize");
        Utilities.SetStateChanged(Entity, "CPointWorldText", "m_Color");
        Utilities.SetStateChanged(Entity, "CPointWorldText", "m_flWorldUnitsPerPx");
    }

    public void UpdatePosition()
    {
        var playerPawn = Target.PlayerPawn.Value;
        if (Entity == null || !Target.IsValid || playerPawn == null || !playerPawn.IsValid)
            return;
        
        Vector forward = new(), right = new(), up = new();
        NativeAPI.AngleVectors(playerPawn.EyeAngles.Handle, forward.Handle, right.Handle, up.Handle);
        
        Vector eyePosition = new();
        eyePosition += forward * 50;
        eyePosition += right * -10;

        QAngle angles = new()
        {
            X = 0,
            Y = playerPawn.EyeAngles.Y + 270,
            Z = playerPawn.EyeAngles.Z + 90
        };
        
        var viewmodel = Target.GetViewmodel();
        Entity.AcceptInput("FollowEntity", viewmodel, Entity, "!activator");
        
        Entity.Teleport(playerPawn.AbsOrigin! + eyePosition, angles, new Vector(0, 0, 0));
    }

    /// <summary>
    /// point_worldtext 삭제
    /// </summary>
    /// <returns>point_worldtext를 삭제 했다면 true, 아니면 false 반환</returns>
    public void KillText()
    {
        if (Entity != null && Entity.IsValid)
            Entity.Remove();
        Entity = null;
    }

    public bool IsValid => Entity != null && Target.IsValid;
}