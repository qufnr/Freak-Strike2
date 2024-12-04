using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using FreakStrike2.Utils.Classes;
using FreakStrike2.Utils.Helpers.Entity;

namespace FreakStrike2;

public partial class FreakStrike2
{
    private void CreatePlayerHudTextOnRoundStart(CCSPlayerController player)
    {
        AddTimer(0.2f, () =>
        {
            if (!player.IsBot && !player.IsHLTV)
                HudTexts[player.Slot] = new HudText(player);
        }, TimerFlags.STOP_ON_MAPCHANGE);
    }

    private void KillHudTextAllOnRoundEnd()
    {
        foreach (var player in PlayerUtils.FindValidPlayers())
            if (HudTexts.ContainsKey(player.Slot) && !player.IsBot && !player.IsHLTV)
                HudTexts[player.Slot].KillText();
    }

    private void PrintHudTextStatusOnGlobalTimerTick(CCSPlayerController player)
    {
        if (!player.IsBot && !player.IsHLTV && HudTexts.ContainsKey(player.Slot))
        {
            if (BaseHalePlayers[player.Slot].IsHale)
            {
                var jumpStatus = BaseHalePlayers[player.Slot].SuperJumpCooldown > 0
                    ? $"{BaseHalePlayers[player.Slot].SuperJumpCooldown:F1}초"
                    : "준비 완료!";
                var weightDownStatus = BaseHalePlayers[player.Slot].WeightDownCooldown > 0
                    ? $"{BaseHalePlayers[player.Slot].WeightDownCooldown:F1}초"
                    : "준비 완료!";
                HudTexts[player.Slot].SetColor(Color.DarkOrange);
                HudTexts[player.Slot].SetText($"분노: 0%\n점프: {jumpStatus}\n내려 찍기: {weightDownStatus}");
            }
            else
            {
                HudTexts[player.Slot].SetColor(Color.CornflowerBlue);
                HudTexts[player.Slot].SetText($"현재 피해량: {BaseGamePlayers[player.Slot].Damages} DMG");
            }
        }
    }
}