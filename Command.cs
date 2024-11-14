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

        [ConsoleCommand("css_qp")]
        public void OnQueuepointCommand(CCSPlayerController? player, CommandInfo cmdInfo)
        {
            switch (cmdInfo.GetArg(1))
            {
                //  초기화
                case "reset":
                    if (IsServerSideCalled(player))
                    {
                        return;
                    }
                    
                    if (!Config.CanResetQueuepoints)
                    {
                        player!.PrintToChat("[FS2] 서버 측에서 Queuepoint 초기화가 비활성화 되어있습니다.");
                        return;
                    }
                    
                    _queuepoint.SetPlayerQueuepoint(player!.Slot, 0);
                    player.PrintToChat("[FS2] Queuepoint 를 초기화 했습니다.");
                    
                    return;
                
                //  순위
                case "rank":
                    var rankMap = _queuepoint.GetRank();
                    if (rankMap is not null && rankMap.Count > 0)
                    {
                        if (player is null)
                            Server.PrintToConsole("[FS2] -- [#Rank]\t[Player Name]\t\t[Queuepoints]");
                        else
                            player.PrintToChat("[FS2] -- [#Rank]\t[Player Name]\t\t[Queuepoints]");

                        var index = 1;
                        foreach (var rank in rankMap)
                        {
                            if (player is null)
                                Server.PrintToConsole($"[FS2] -- [#{index}]\t[{rank.Key.PlayerName}]\t\t[{rank.Value}qp]");
                            else
                                player.PrintToChat($"[FS2] -- [#{index}]\t[{rank.Key.PlayerName}]\t\t[{rank.Value}qp]");
                        }
                    }
                    else
                    {
                        if(player is null)
                            Server.PrintToConsole("[FS2] 서버 내에서 Queuepoints 를 가지고 있는 플레이어가 없습니다.");
                        else
                            player.PrintToChat("[FS2] 서버 내에서 Queuepoints 를 가지고 있는 플레이어가 없습니다.");
                    }

                    return;
            }

            if (!IsServerSideCalled(player))
            {
                player!.PrintToChat($"[FS2] 소지중인 Queuepoints: {_queuepoint.GetPlayerQueuepoint(player.Slot)} qp");
                player!.PrintToChat("[FS2] 그 외 다른 명령어: <command> reset - Queuepoint 초기화, <command> rank - Queuepoint 순위권 확인");
            }
        }

        [ConsoleCommand("css_hales", "헤일 정보를 확인합니다.")]
        public void OnHaleInfoCommand(CCSPlayerController? player, CommandInfo cmdInfo)
        {
            var haleIndex = 0;
            if (player is null || !player.IsValid)
            {
                if (_hales.Count == 0)
                {
                    
                    Server.PrintToConsole("[FS2] 서버에 설정된 헤일 클래스가 없습니다.");
                    return;
                }
                Server.PrintToConsole("= = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = ");
                Server.PrintToConsole("[#Id]\t[Name]\t\t[Description]\t\t[Designer Name]");
                foreach(var hale in _hales)
                {
                    haleIndex++;
                    Server.PrintToConsole($"#{haleIndex}\t{hale.Name}\t\t{hale.Description}\t\t{hale.DesignerName}");
                }
                Server.PrintToConsole("= = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = ");
                return;
            }
            
            if (_hales.Count == 0)
            {
                player.PrintToChat("[FS2] 서버에 설정된 헤일 클래스가 없습니다.");
                return;
            }
            
            player.PrintToChat("[FS2] 콘솔에 출력되었습니다.");

            player.PrintToConsole("= = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = ");
            player.PrintToConsole("[#Id]\t[Name]\t\t[Description]\t\t[Designer Name]");
            foreach(var hale in _hales)
            {
                haleIndex++;
                player.PrintToConsole($"#{haleIndex}\t{hale.Name}\t\t{hale.Description}\t\t{hale.DesignerName}");
            }
            player.PrintToConsole("= = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = ");
        }

        private bool IsServerSideCalled(CCSPlayerController? player)
        {
            if (player is null || player.IsBot)
            {
                Server.PrintToConsole("[FS2] 클라이언트 측 명령어입니다.");
                return true;
            }

            return false;
        }
    }
}
