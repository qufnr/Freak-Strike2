using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
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
        
        KillText();
        
        Entity = Utilities.CreateEntityByName<CPointWorldText>("point_worldtext");
        if (Entity == null)
            throw new GameNotSupportedException();
            
        Entity.Enabled = true;
        
        SetFullbright();
        SetDepthOffset();
        SetSize();
        SetColor(Color.White);
        SetScale();
        SetJustifyHorizontal();
        SetJustifyVertical();
        SetReorientMode();
        
        Entity.DispatchSpawn();
        
        UpdatePosition();
    }

    public void SetText(string message, float duration = 0f)
    {
        if (!Target.IsValid || Entity == null || !Entity.IsValid)
            return;
        
        Entity.MessageText = message;
        Utilities.SetStateChanged(Entity, "CPointWorldText", "m_messageText");
        
        if (duration > 0.0)
            Entity.AddEntityIOEvent("Kill", Entity, null, "", duration);
    }

    public void SetSize(float size = 23)
    {
        if (Entity != null && Entity.IsValid)
        {
            Entity.FontSize = size;
            Utilities.SetStateChanged(Entity, "CPointWorldText", "m_flFontSize");
        }
    }

    public void SetColor(Color color)
    {
        if (Entity != null && Entity.IsValid)
        {
            Entity.Color = color;
            Utilities.SetStateChanged(Entity, "CPointWorldText", "m_Color");
        }
    }

    public void SetScale(float scale = 0.12f)
    {
        if (Entity != null && Entity.IsValid)
        {
            Entity.WorldUnitsPerPx = scale;
            Utilities.SetStateChanged(Entity, "CPointWorldText", "m_flWorldUnitsPerPx");
        }
    }

    public void SetJustifyHorizontal(PointWorldTextJustifyHorizontal_t justifyHorizontal = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT)
    {
        if (Entity != null && Entity.IsValid)
        {
            Entity.JustifyHorizontal = justifyHorizontal;
            Utilities.SetStateChanged(Entity, "CPointWorldText", "m_nJustifyHorizontal");
        }
    }

    public void SetJustifyVertical(PointWorldTextJustifyVertical_t justifyVertical = PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_TOP)
    {
        if (Entity != null && Entity.IsValid)
        {
            Entity.JustifyVertical = justifyVertical;
            Utilities.SetStateChanged(Entity, "CPointWorldText", "m_nJustifyVertical");
        }
    }

    public void SetReorientMode(PointWorldTextReorientMode_t reorientMode = PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE)
    {
        if (Entity != null && Entity.IsValid)
        {
            Entity.ReorientMode = reorientMode;
            Utilities.SetStateChanged(Entity, "CPointWorldText", "m_nReorientMode");
        }
    }

    private void SetDepthOffset()
    {
        if (Entity != null && Entity.IsValid)
        {
            Entity.DepthOffset = .0f;
            Utilities.SetStateChanged(Entity, "CPointWorldText", "m_flDepthOffset");
        }
    }

    private void SetFullbright()
    {
        if (Entity != null && Entity.IsValid)
        {
            Entity.Fullbright = true;
            Utilities.SetStateChanged(Entity, "CPointWorldText", "m_bFullbright");
        }
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
        eyePosition += right * 25;

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