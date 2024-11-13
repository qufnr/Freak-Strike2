using System.Drawing;
using System.Numerics;
using CounterStrikeSharp.API.Core;

namespace FreakStrike2.Utils.Classes;

public class HudTextAttribute
{
    public float FontSize { get; set; } = 20.0f;
    public Color Color { get; set; } = Color.White;
    public float Scale { get; set; } = 1.0f;
    public PointWorldTextJustifyHorizontal_t JustifyHorizontal { get; set; } = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT;
    public PointWorldTextJustifyVertical_t JustifyVertical { get; set; } = PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_TOP;
    public PointWorldTextReorientMode_t PeorientMode { get; set; } = PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE;
}