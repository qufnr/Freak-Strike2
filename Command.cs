using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace FreakStrike2
{
    public partial class FreakStrike2
    {
        [ConsoleCommand("css_fs2debug", "FS2 에 대한 디버깅을 실행합니다.")]
        [RequiresPermissions("@css/cvar")]
        public void OnDebugCommand(CCSPlayerController? player, CommandInfo cmdInfo)
        {
            if (player is null || player.Slot < 0)
            {
                Server.PrintToConsole("[FS2] 해당 명령어는 클라이언트 측 명령어입니다.");
                return;
            }

            DebugPlayers[player.Slot] = !DebugPlayers[player.Slot];

            string text = DebugPlayers[player.Slot] ? "활성화" : "비활성화";
            player.PrintToChat($"[FS2] 디버그 모드가 {text} 되었습니다.");
        }
    }
}
