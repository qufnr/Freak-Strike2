using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using FreakStrike2.Exceptions;

namespace FreakStrike2.Utils.Classes;

public class GlowEntity
{
    private CBaseEntity? Entity { get; set; } = null;
    private CDynamicProp? Glow { get; set; } = null;

    public Color Colour { get; private set; } = Color.Transparent;
    public int Range { get; private set; } = 5000;
    public int RangeMin { get; private set; } = 0;
    public int Team { get; private set; } = 2;
    public int Type { get; private set; } = 3;

    /// <summary>
    /// Glow 생성자
    /// </summary>
    /// <param name="entity">주인 객체</param>
    /// <exception cref="GameNotSupportedException">게임 미지원</exception>
    public GlowEntity(CBaseEntity entity)
    {
        Glow = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");

        if (Glow == null || !Glow.IsValid)
            throw new GameNotSupportedException();
        
        Entity = entity;

        Glow.Spawnflags = 256;
        Glow.Render = Color.Transparent;
        Glow.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags = (uint)(Glow.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags & ~ (1 << 2));
        Glow.SetModel(Entity.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName);
        Glow.DispatchSpawn();

        SetColour(Colour);
        SetRange(Range);
        SetRangeMin(RangeMin);
        SetTeam(Team);
        SetType(Type);
        
        Glow.Teleport(Entity.AbsOrigin, Entity.AbsRotation, Entity.AbsVelocity);
        Glow.AcceptInput("SetParent", Entity, Glow, "!activator");
    }

    public void SetColour(Color colour)
    {
        if (Glow == null || !Glow.IsValid)
            return;
        
        Colour = colour;
        Glow.Glow.GlowColorOverride = Colour;
        Utilities.SetStateChanged(Glow, "CGlowProperty", "m_glowColorOverride");
    }

    public void SetRange(int range)
    {
        if (Glow == null || !Glow.IsValid)
            return;

        Range = range;
        Glow.Glow.GlowRange = Range;
        Utilities.SetStateChanged(Glow, "CGlowProperty", "m_nGlowRange");
    }

    public void SetRangeMin(int rangeMin)
    {
        if (Glow == null || !Glow.IsValid)
            return;

        RangeMin = rangeMin;
        Glow.Glow.GlowRangeMin = RangeMin;
        Utilities.SetStateChanged(Glow, "CGlowProperty", "m_nGlowRangeMin");
    }

    public void SetTeam(int teamnum)
    {
        if (Glow == null || !Glow.IsValid)
            return;

        Team = teamnum;
        Glow.Glow.GlowTeam = Team;
        Utilities.SetStateChanged(Glow, "CGlowProperty", "m_iGlowTeam");
    }

    public void SetType(int type)
    {
        if (Glow == null || !Glow.IsValid)
            return;

        Type = type;
        Glow.Glow.GlowType = Type;
        Utilities.SetStateChanged(Glow, "CGlowProperty", "m_iGlowType");
    }
    
    /// <summary>
    /// Glow 비활성화
    /// </summary>
    public void Deactivate() => SetAttribute(Color.Transparent, 0);

    /// <summary>
    /// Glow 활성화
    /// </summary>
    public void Activate() => SetAttribute(Colour, Range, RangeMin, Team, Type);

    /// <summary>
    /// Glow 삭제
    /// </summary>
    public void Kill()
    {
        if (Glow == null || !Glow.IsValid)
            return;

        Glow.Remove();
        Glow = null;
        Entity = null;
    }

    /// <summary>
    /// Glow 의 주인이 유효한지 여부 판단
    /// </summary>
    public bool IsValidOwner => Entity != null && Entity.IsValid;
    
    /// <summary>
    /// 속성 설정
    /// </summary>
    /// <param name="colour">색상</param>
    /// <param name="range">범위</param>
    /// <param name="rangeMin">최소 범위</param>
    /// <param name="team">팀</param>
    /// <param name="type">유형</param>
    private void SetAttribute(Color colour, int range = 5000, int rangeMin = 0, int team = 2, int type = 3)
    {
        if (Glow == null)
            return;
        
        SetColour(colour);
        SetRange(range);
        SetRangeMin(rangeMin);
        SetTeam(team);
        SetType(type);
    }
}