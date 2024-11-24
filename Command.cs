using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using FreakStrike2.Classes;
using FreakStrike2.Models;
using FreakStrike2.Utils;

namespace FreakStrike2;
public partial class FreakStrike2
{
    [ConsoleCommand("css_fs2", "FS2 Default command.")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnFs2Command(CCSPlayerController? player, CommandInfo cmdInfo)
    {
        var arg = cmdInfo.GetArg(1);

        if (string.IsNullOrEmpty(arg))
        {
            cmdInfo.ReplyToCommand($"[FS2] -- {ModuleDescription} (Made by. {ModuleAuthor})");
            cmdInfo.ReplyToCommand($"[FS2] -- Now running {ModuleName}({ModuleVersion}) on this server!");
            return;
        }

        if (string.Equals(arg, "help", StringComparison.OrdinalIgnoreCase))
        {
            cmdInfo.ReplyToCommand("[FS2] -- Help Commands! (<> - required, [] - optional)");
            cmdInfo.ReplyToCommand("[FS2] -- css_fs2 help - 도움말을 확인합니다. (Client / Console)");
            cmdInfo.ReplyToCommand("[FS2] -- css_qp [rank|reset|info <#userid|name>] - 큐포인트 정보를 확인합니다. (Client / Console)");
            if (player != null && player.IsValid && AdminManager.PlayerHasPermissions(player, "@css/root"))
            {
                cmdInfo.ReplyToCommand("[FS2] -- css_qp set <#userid|name> <value> - 플레이어의 큐포인트를 설정합니다. (Root Client / Console)");
                cmdInfo.ReplyToCommand("[FS2] -- css_fs2 sethale <#userid|name> [hale] - 플레이어를 헤일로 설정합니다. (Root Client / Console)");
                cmdInfo.ReplyToCommand("[FS2] -- css_fs2 debug - FS2 디버그 모드를 활성화 또는 비활성화 합니다. (Root Client)");
            }

            return;
        }
        
        //  Root Commands
        if (AdminManager.PlayerHasPermissions(player, "@css/root"))
        {
            //  디버그
            if (string.Equals(arg, "debug", StringComparison.OrdinalIgnoreCase))
            {
                if (player != null && player.IsValid)
                {
                    var toggle = BaseGamePlayers[player.Slot].ToggleDebugMode();
                    var text = toggle ? "활성화" : "비활성화";
                    
                    cmdInfo.ReplyToCommand($"[FS2] 디버그 모드를 {text} 했습니다.");
                }

                return;
            }

            //  플레이어를 헤일로 설정
            if (string.Equals(arg, "sethale", StringComparison.OrdinalIgnoreCase))
            {
                var playerNameOrUserId = cmdInfo.GetArg(2);
                if (string.IsNullOrEmpty(playerNameOrUserId))
                    cmdInfo.ReplyToCommand("[FS2] Usage: css_fs2 sethale <#userid|name> [hale]");
                else
                {
                    var target = PlayerUtils.FindPlayerByNameOrUserId(playerNameOrUserId);
                    if (target != null && target.IsValid)
                    {
                        if (InGameStatus != GameStatus.Start)
                            cmdInfo.ReplyToCommand("[FS2] 게임이 시작된 상태가 아닙니다.");
                        else if (!target.PawnIsAlive)
                            cmdInfo.ReplyToCommand($"[FS2] 플레이어 {target.PlayerName} 이(가) 살아있지 않습니다.");
                        else if (BaseHalePlayers[target.Slot].IsHale)
                            cmdInfo.ReplyToCommand($"[FS2] 플레이어 {target.PlayerName} 은(는) 이미 헤일 {BaseHalePlayers[target.Slot].MyHale!.Name} (으)로 플레이하고 있습니다.");
                        else
                        {
                            var haleName = cmdInfo.GetArg(3);
                            var hale = string.IsNullOrEmpty(haleName)
                                ? CommonUtils.GetRandomInList(Hales)
                                : FindHaleByDesignerName(haleName);
                            
                            if (hale == null)
                                cmdInfo.ReplyToCommand("[FS2] 존재하지 않은 헤일입니다.");
                            else
                            {
                                cmdInfo.ReplyToCommand($"[FS2] 플레이어 {target.PlayerName} 을(를) 헤일 {hale.Name} (으)로 설정했습니다.");
                                Server.PrintToChatAll($"[FS2] 관리자에 의해 {target.PlayerName} 이(가) 헤일 {hale.Name} (으)로 설정되었습니다.");
                                BaseHalePlayers[target.Slot] = new BaseHalePlayer(target, hale);
                            }
                        }
                    }
                }

                return;
            }
        }
    }
    
    [ConsoleCommand("css_qp", "큐포인트 정보를 확인합니다.")]
    [CommandHelper(minArgs: 0, usage: "<reset|rank>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnQueuepointCommand(CCSPlayerController? player, CommandInfo cmdInfo)
    {
        var arg = cmdInfo.GetArg(1);

        if ((player == null || !player.IsValid) && (string.IsNullOrEmpty(arg) || "reset".Equals(arg)))
        {
            cmdInfo.ReplyToCommand("[FS2] 클라이언트 측 명령어입니다.");
            return;
        }
        
        switch (arg)
        {
            //  초기화
            case "reset":
                if (!Config.CanResetQueuepoints)
                    cmdInfo.ReplyToCommand("[FS2] 서버측에서 큐포인트 초기화가 비활성화 되어있습니다.");

                else if (player is null || !player.IsValid)
                    cmdInfo.ReplyToCommand("[FS2] 클라이언트 측 명령어입니다.");
                else
                {
                    PlayerQueuePoints[player.Slot].Points = 0;
                    player.PrintToChat("[FS2] 큐포인트를 초기화 했습니다.");
                }
                
                return;
            
            //  순위
            case "rank":
                var ranks = PlayerQueuePoints.Where(pqp =>
                    {
                        var p = Utilities.GetPlayerFromSlot(pqp.Key);
                        return p != null && p.IsValid && !p.IsBot;
                    })
                    .OrderByDescending(pqp => pqp.Value.Points)
                    .ToDictionary(
                        pqp => Utilities.GetPlayerFromSlot(pqp.Key)!.PlayerName,
                        pqp => pqp.Value.Points
                    );
                
                if (ranks.Count > 0)
                {
                    cmdInfo.ReplyToCommand(player is null ? 
                        "[FS2] -- [#순위]\t[플레이어]\t\t[큐포인트]" : 
                        "[FS2] -- 큐포인트 소지 순위");

                    var index = 1;
                    foreach (var rank in ranks)
                    {
                        cmdInfo.ReplyToCommand(player is null ? 
                            $"[FS2] -- [#{index}]\t[{rank.Key}]\t\t[{rank.Value} QP]" : 
                            $"[FS2] -- #{index} | ${rank.Key} | {rank.Value} 큐포인트");
                        
                        index++;
                    }
                }
                else
                    cmdInfo.ReplyToCommand("[FS2] 서버 내에서 큐포인트를 가지고 있는 플레이어가 없습니다.");

                return;
            
            //  플레이어 큐포인트 설정
            case "set":
                if (player == null || AdminManager.PlayerHasPermissions(player, "@css/root"))
                {
                    var userIdOrName = cmdInfo.GetArg(2);
                    if (string.IsNullOrEmpty(userIdOrName))
                        cmdInfo.ReplyToCommand("[FS2] Usage: css_qp set <#userid|name> <value>");
                    else
                    {
                        var target = PlayerUtils.FindPlayerByNameOrUserId(userIdOrName);
                        if (target == null || !target.IsValid)
                            cmdInfo.ReplyToCommand("[FS2] 상대 플레이어를 찾을 수 없습니다.");
                        else
                        {
                            int value;
                            if (int.TryParse(cmdInfo.GetArg(3), out value))
                            {
                                cmdInfo.ReplyToCommand($"[FS2] 플레이어 {target.PlayerName} 의 큐포인트를 {value} 으(로) 설정했습니다.");
                                Server.PrintToChatAll($"[FS2] 관리자에 의해 플레이어 {target.PlayerName} 의 큐포인트가 {value} 으(로) 설정되었습니다.");
                                PlayerQueuePoints[target.Slot].Points = value;
                            }
                            else
                                cmdInfo.ReplyToCommand($"[FS2] Usage: css_qp set {userIdOrName} <value>");
                        }
                    }
                }
                
                break;
        }

        if(player is not null && player.IsValid)
            cmdInfo.ReplyToCommand($"[FS2] 소지중인 큐포인트: {PlayerQueuePoints[player.Slot].Points} QP");
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
