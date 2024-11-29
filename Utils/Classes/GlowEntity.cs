using System.Drawing;
using CounterStrikeSharp.API.Core;
using FreakStrike2.Exceptions;

namespace FreakStrike2.Utils.Classes;

public class GlowEntity
{
    private CBaseEntity? Entity { get; set; } = null;
    private CDynamicProp? Glow { get; set; } = null;

    public Color Colour { get; set; } = Color.Transparent;
    public int Range { get; set; } = 5000;
    public int RangeMin { get; set; } = 0;
    public int Team { get; set; } = 2;
    public int Type { get; set; } = 3;

    /// <summary>
    /// Glow 생성자
    /// </summary>
    /// <param name="entity">주인 객체</param>
    /// <exception cref="GameNotSupportedException">게임 미지원</exception>
    public GlowEntity(CBaseEntity entity)
    {
        Glow = CounterStrikeSharp.API.Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");

        if (Glow == null || !Glow.IsValid)
            throw new GameNotSupportedException();
        
        Entity = entity;

        Glow.Spawnflags = 256;
        Glow.Render = Color.Transparent;
        Glow.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags = (uint)(Glow.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags & ~ (1 << 2));
        Glow.SetModel(Entity.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName);
        Glow.DispatchSpawn();

        Glow.Glow.GlowColorOverride = Colour;
        Glow.Glow.GlowRange = Range;
        Glow.Glow.GlowRangeMin = RangeMin;
        Glow.Glow.GlowTeam = Team;
        Glow.Glow.GlowType = Team;
        
        Glow.Teleport(Entity.AbsOrigin, Entity.AbsRotation, Entity.AbsVelocity);
        Glow.AcceptInput("SetParent", Entity, Glow, "!activator");
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

        Glow.Glow.GlowColorOverride = colour;
        Glow.Glow.GlowRange = range;
        Glow.Glow.GlowRangeMin = rangeMin;
        Glow.Glow.GlowTeam = team;
        Glow.Glow.GlowType = type;
    }
}