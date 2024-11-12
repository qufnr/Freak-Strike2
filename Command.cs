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

            var text = DebugPlayers[player.Slot] ? "활성화" : "비활성화";
            player.PrintToChat($"[FS2] 디버그 모드가 {text} 되었습니다.");
        }

        [ConsoleCommand("css_hales", "헤일 정보를 확인합니다.")]
        public void OnHaleInfoCommand(CCSPlayerController? player, CommandInfo cmdInfo)
        {
            if (player is null || !player.IsValid)
            {
                return;
            }
            
            if (_hales.Count == 0)
            {
                player.PrintToChat("[FS2] 서버에 설정된 헤일 클래스가 없습니다.");
                return;
            }
            
            player.PrintToChat("[FS2] 콘솔에 출력되었습니다.");

            player.PrintToConsole("= = = = = = = = = = = = = = = = = = = = = = = = = = = ");
            player.PrintToConsole("[#Id]\t[Name]\t\t[Description]\t\t[Designer Name]");
            var haleIndex = 0;
            foreach(var hale in _hales)
            {
                player.PrintToConsole($"#{haleIndex}\t{hale.Name}\t\t{hale.Description}\t\t{hale.DesignerName}");
                haleIndex++;
            }
            player.PrintToConsole("= = = = = = = = = = = = = = = = = = = = = = = = = = = ");
        }
    }
}
