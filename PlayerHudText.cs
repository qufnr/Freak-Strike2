using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using FreakStrike2.Utils.Classes;

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

    private void KillPlayerHudTextOnRoundEnd(CCSPlayerController player)
    {
        if (!player.IsBot && !player.IsHLTV)
            HudTexts[player.Slot].KillText();
    }

    private void PrintHudTextStatusOnGlobalTimerTick(CCSPlayerController player)
    {
        if (!player.IsBot && !player.IsHLTV)
        {
            // HudTexts[player.Slot].UpdatePosition();
            if (BaseHalePlayers[player.Slot].IsHale)
                HudTexts[player.Slot].SetMessage($"분노: 0%\n점프 쿨다운: {BaseHalePlayers[player.Slot].SuperJumpCooldown}\n내려 찍기 쿨다운: {BaseHalePlayers[player.Slot].WeightDownCooldown}", new HudTextAttribute());
            else
                HudTexts[player.Slot].SetMessage($"현재 피해량: {BaseGamePlayers[player.Slot].Damages} DMG", new HudTextAttribute());
        }
    }
}