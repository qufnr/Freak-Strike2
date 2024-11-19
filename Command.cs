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
        [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_ONLY)]
        [RequiresPermissions("@css/cvar")]
        public void OnDebugCommand(CCSPlayerController? player, CommandInfo cmdInfo)
        {
            if (player is null || !player.IsValid)
                return;
            
            GamePlayer.SetPlayerDebugMode(player, !GamePlayer.PlayerIsDebugMode(player));

            var text = GamePlayer.PlayerIsDebugMode(player) ? "활성화" : "비활성화";
            player.PrintToChat($"[FS2] 디버그 모드가 {text} 되었습니다.");
        }

        [ConsoleCommand("css_qp", "Queuepoint 에 대한 처리를 진행합니다.")]
        [CommandHelper(minArgs: 0, usage: "<reset|rank>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        public void OnQueuepointCommand(CCSPlayerController? player, CommandInfo cmdInfo)
        {
            var arg = cmdInfo.GetArg(1);

            if (string.IsNullOrEmpty(arg) || "reset".Equals(arg))
            {
                cmdInfo.ReplyToCommand("[FS2] 클라이언트 측 명령어입니다.");
                return;
            }
            
            switch (arg)
            {
                //  초기화
                case "reset":
                    if (!Config.CanResetQueuepoints)
                    {
                        cmdInfo.ReplyToCommand("[FS2] 서버 측에서 Queuepoint 초기화가 비활성화 되어있습니다.");
                        return;
                    }
                    
                    PlayerQueuePoint.SetPlayerQueuepoint(player!.Slot, 0);
                    player.PrintToChat("[FS2] Queuepoint 를 초기화 했습니다.");
                    
                    return;
                
                //  순위
                case "rank":
                    var rankMap = PlayerQueuePoint.GetRank();
                    if (rankMap is not null && rankMap.Count > 0)
                    {
                        cmdInfo.ReplyToCommand(player is null ? 
                            "[FS2] -- [#Rank]\t[Player Name]\t\t[Queuepoints]" : 
                            "[FS2] -- Queuepoint 소지 순위");

                        var index = 1;
                        foreach (var rank in rankMap)
                        {
                            cmdInfo.ReplyToCommand(player is null ? 
                                $"[FS2] -- [#{index}]\t[{rank.Key.PlayerName}]\t\t[{rank.Value} QP]" : 
                                $"[FS2] -- #{index} | ${rank.Key.PlayerName} | {rank.Value} QP");
                        }
                    }
                    else
                        cmdInfo.ReplyToCommand("[FS2] 서버 내에서 Queuepoints 를 가지고 있는 플레이어가 없습니다.");

                    return;
            }

            if(player is not null && player.IsValid)
                cmdInfo.ReplyToCommand($"[FS2] 소지중인 Queuepoints: {PlayerQueuePoint.GetPlayerQueuepoint(player.Slot)} QP");
        }

        [ConsoleCommand("css_hales", "헤일 정보를 확인합니다.")]
        public void OnHaleInfoCommand(CCSPlayerController? player, CommandInfo cmdInfo)
        {
            var haleIndex = 0;
            if (player is null || !player.IsValid)
            {
                if (Hales.Count == 0)
                {
                    
                    Server.PrintToConsole("[FS2] 서버에 설정된 헤일 클래스가 없습니다.");
                    return;
                }
                Server.PrintToConsole("-----------------------------------------------------------------------------------------------------------------------");
                Server.PrintToConsole("[#Id]\t[Name]\t\t[Designer Name]\t\t\t[[Description]");
                foreach(var hale in Hales)
                {
                    haleIndex++;
                    Server.PrintToConsole($"#{haleIndex}\t{hale.Name}\t\t{hale.DesignerName}\t\t\t{hale.Description}");
                }
                Server.PrintToConsole("-----------------------------------------------------------------------------------------------------------------------");
                return;
            }
            
            if (Hales.Count == 0)
            {
                player.PrintToChat("[FS2] 서버에 설정된 헤일 클래스가 없습니다.");
                return;
            }
            
            player.PrintToChat("[FS2] 콘솔에 출력되었습니다.");

            player.PrintToConsole("-----------------------------------------------------------------------------------------------------------------------");
            player.PrintToConsole("[#Id]\t[Name]\t\t[Designer Name]\t\t\t[Description]");
            foreach(var hale in Hales)
            {
                haleIndex++;
                player.PrintToConsole($"#{haleIndex}\t{hale.Name}\t\t{hale.DesignerName}\t\t\t{hale.Description}");
            }
            player.PrintToConsole("-----------------------------------------------------------------------------------------------------------------------");
        }
    }
}
