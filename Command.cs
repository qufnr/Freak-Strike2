using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
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
            cmdInfo.ReplyToCommand($"{MessagePrefix}-- {ModuleDescription} (Made by. {ModuleAuthor})");
            cmdInfo.ReplyToCommand($"{MessagePrefix}-- Now running {ModuleName}({ModuleVersion}) on this server!");
            return;
        }

        if (string.Equals(arg, "help", StringComparison.OrdinalIgnoreCase))
        {
            cmdInfo.ReplyToCommand($"{MessagePrefix}-- Help Commands! (<> - required, [] - optional)");
            cmdInfo.ReplyToCommand($"{MessagePrefix}-- css_fs2 help - 도움말을 확인합니다. (Client / Console)");
            cmdInfo.ReplyToCommand($"{MessagePrefix}-- css_hclass -- 인간 진영 클래스를 선택합니다. (Client)");
            cmdInfo.ReplyToCommand($"{MessagePrefix}-- css_qp [rank|reset|info <#userid|name>] - 큐포인트 정보를 확인합니다. (Client / Console)");
            if (player != null && player.IsValid && AdminManager.PlayerHasPermissions(player, "@css/root"))
            {
                cmdInfo.ReplyToCommand($"{MessagePrefix}-- css_qp set <#userid|name> <value> - 플레이어의 큐포인트를 설정합니다. (Root Client / Console)");
                cmdInfo.ReplyToCommand($"{MessagePrefix}-- css_fs2 sethale <#userid|name> [hale] - 플레이어를 헤일로 설정합니다. (Root Client / Console)");
                cmdInfo.ReplyToCommand($"{MessagePrefix}-- css_fs2 setstun <#userid|name> <stuntime> - 플레이어를 스턴 상태로 설정합니다. (Root Client / Console)");
                cmdInfo.ReplyToCommand($"{MessagePrefix}-- css_fs2 debug - FS2 디버그 모드를 활성화 또는 비활성화 합니다. (Root Client)");
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
                    
                    cmdInfo.ReplyToCommand($"{MessagePrefix}디버그 모드를 {text} 했습니다.");
                }

                return;
            }

            //  플레이어를 헤일로 설정
            if (string.Equals(arg, "sethale", StringComparison.OrdinalIgnoreCase))
            {
                var playerNameOrUserId = cmdInfo.GetArg(2);
                if (string.IsNullOrEmpty(playerNameOrUserId))
                    cmdInfo.ReplyToCommand($"{MessagePrefix}Usage: css_fs2 sethale <#userid|name> [hale]");
                else
                {
                    var target = PlayerUtils.FindPlayerByNameOrUserId(playerNameOrUserId);
                    if (target == null || !target.IsValid)
                        cmdInfo.ReplyToCommand($"{MessagePrefix}상대 플레이어가 유효하지 않습니다.");
                    else if (InGameStatus != GameStatus.Start)
                        cmdInfo.ReplyToCommand($"{MessagePrefix}게임이 시작된 상태가 아닙니다.");
                    else if (!target.PawnIsAlive)
                        cmdInfo.ReplyToCommand($"{MessagePrefix}플레이어 {target.PlayerName} 이(가) 살아있지 않습니다.");
                    else if (BaseHalePlayers[target.Slot].IsHale)
                        cmdInfo.ReplyToCommand($"{MessagePrefix}플레이어 {target.PlayerName} 은(는) 이미 헤일 {BaseHalePlayers[target.Slot].MyHale!.Name} (으)로 플레이하고 있습니다.");
                    else
                    {
                        var haleName = cmdInfo.GetArg(3);
                        var hale = string.IsNullOrEmpty(haleName)
                            ? CommonUtils.GetRandomInList(Hales)
                            : FindHaleByDesignerName(haleName);
                        
                        if (hale == null)
                            cmdInfo.ReplyToCommand($"{MessagePrefix}존재하지 않은 헤일입니다.");
                        else
                        {
                            cmdInfo.ReplyToCommand($"{MessagePrefix}플레이어 {target.PlayerName} 을(를) 헤일 {hale.Name} (으)로 설정했습니다.");
                            Server.PrintToChatAll($"{MessagePrefix}관리자에 의해 {target.PlayerName} 이(가) 헤일 {hale.Name} (으)로 설정되었습니다.");
                            BaseHalePlayers[target.Slot] = new BaseHalePlayer(target, hale);
                        }
                    }
            }

                return;
            }
            
            //  플레이어 스턴 상태 설정
            if (string.Equals(arg, "setstun", StringComparison.OrdinalIgnoreCase))
            {
                var playerNameOrUserId = cmdInfo.GetArg(2);
                if (string.IsNullOrEmpty(playerNameOrUserId))
                    cmdInfo.ReplyToCommand($"{MessagePrefix}Usage: css_fs2 setstun <#userid|name> <stuntime>");
                else
                {
                    var target = PlayerUtils.FindPlayerByNameOrUserId(playerNameOrUserId);
                    if (target == null || !target.IsValid)
                        cmdInfo.ReplyToCommand($"{MessagePrefix}상대 플레이어가 유효하지 않습니다.");
                    else if(!target.PawnIsAlive)
                        cmdInfo.ReplyToCommand($"{MessagePrefix}플레이어 {target.PlayerName} 이(가) 살아있지 않습니다.");
                    else
                    {
                        var stringStunTime = cmdInfo.GetArg(3);
                        if (int.TryParse(stringStunTime, out var stunTime))
                        {
                            if (stunTime <= 0)
                                cmdInfo.ReplyToCommand($"{MessagePrefix}스턴 시간은 0 이상이어야 합니다.");
                            else
                            {
                                var targetPawn = target.PlayerPawn.Value;
                                if (targetPawn != null && targetPawn.IsValid)
                                {
                                    BaseGamePlayers[target.Slot].ActivateStun(targetPawn, 
                                        AddTimer(0.1f, 
                                            BaseGamePlayers[target.Slot]
                                                .StunTimerCallback(target, InGameStatus, BaseGamePlayers[target.Slot].DebugMode), 
                                            TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE),
                                        stunTime);
                                    cmdInfo.ReplyToCommand($"{MessagePrefix}플레이어 {target.PlayerName} 을(를) 스턴 상태({stunTime}초)로 변경했습니다.");
                                    Server.PrintToChatAll($"{MessagePrefix}관리자에 의해 플레이어 {target.PlayerName} 이(가) 스턴 상태({stunTime}초)로 변경되었습니다.");
                                }
                            }
                        }
                        else
                            cmdInfo.ReplyToCommand($"{MessagePrefix}Usage: css_fs2 setstun {playerNameOrUserId} <stuntime>");
                    }
                }
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
            cmdInfo.ReplyToCommand($"{MessagePrefix}클라이언트 측 명령어입니다.");
            return;
        }
        
        switch (arg)
        {
            //  초기화
            case "reset":
                if (!Config.CanResetQueuepoints)
                    cmdInfo.ReplyToCommand($"{MessagePrefix}서버측에서 큐포인트 초기화가 비활성화 되어있습니다.");
                else if (player is null || !player.IsValid)
                    cmdInfo.ReplyToCommand($"{MessagePrefix}클라이언트 측 명령어입니다.");
                else
                {
                    PlayerQueuePoints[player.Slot].Points = 0;
                    player.PrintToChat($"{MessagePrefix}큐포인트를 초기화 했습니다.");
                }
                
                return;
            
            //  순위
            case "rank":
                PrintRankOfQueuePoints(player);
                return;
            
            //  플레이어 큐포인트 설정
            case "set":
                if (player == null || AdminManager.PlayerHasPermissions(player, "@css/root"))
                {
                    var userIdOrName = cmdInfo.GetArg(2);
                    if (string.IsNullOrEmpty(userIdOrName))
                        cmdInfo.ReplyToCommand($"{MessagePrefix}Usage: css_qp set <#userid|name> <value>");
                    else
                    {
                        var target = PlayerUtils.FindPlayerByNameOrUserId(userIdOrName);
                        if (target == null || !target.IsValid)
                            cmdInfo.ReplyToCommand($"{MessagePrefix}상대 플레이어를 찾을 수 없습니다.");
                        else
                        {
                            int value;
                            if (int.TryParse(cmdInfo.GetArg(3), out value))
                            {
                                cmdInfo.ReplyToCommand($"{MessagePrefix}플레이어 {target.PlayerName} 의 큐포인트를 {value} 으(로) 설정했습니다.");
                                Server.PrintToChatAll($"{MessagePrefix}관리자에 의해 플레이어 {target.PlayerName} 의 큐포인트가 {value} 으(로) 설정되었습니다.");
                                PlayerQueuePoints[target.Slot].Points = value;
                            }
                            else
                                cmdInfo.ReplyToCommand($"{MessagePrefix}Usage: css_qp set {userIdOrName} <value>");
                        }
                    }
                }
                
                break;
        }

        if(player is not null && player.IsValid)
            cmdInfo.ReplyToCommand($"{MessagePrefix}소지중인 큐포인트: {PlayerQueuePoints[player.Slot].Points} QP");
    }

    [ConsoleCommand("css_hales", "헤일 정보를 확인합니다.")]
    public void OnHaleInfoCommand(CCSPlayerController? player, CommandInfo cmdInfo)
    {
        var haleIndex = 0;
        if (player is null || !player.IsValid)
        {
            if (Hales.Count == 0)
            {
                
                Server.PrintToConsole($"{MessagePrefix}서버에 설정된 헤일 클래스가 없습니다.");
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
            player.PrintToChat($"{MessagePrefix}서버에 설정된 헤일 클래스가 없습니다.");
            return;
        }
        
        player.PrintToChat($"{MessagePrefix}콘솔에 출력되었습니다.");

        player.PrintToConsole("-----------------------------------------------------------------------------------------------------------------------");
        player.PrintToConsole("[#Id]\t[Name]\t\t[Designer Name]\t\t\t[Description]");
        foreach(var hale in Hales)
        {
            haleIndex++;
            player.PrintToConsole($"#{haleIndex}\t{hale.Name}\t\t{hale.DesignerName}\t\t\t{hale.Description}");
        }
        player.PrintToConsole("-----------------------------------------------------------------------------------------------------------------------");
    }

    [ConsoleCommand("css_hclass", "인간 클래스 정보를 확인합니다.")]
    public void OnHumanClassCommand(CCSPlayerController? player, CommandInfo cmdInfo)
    {
        //  Server side
        if (player == null || !player.IsValid)
        {
            var index = 0;
            Server.PrintToConsole("-----------------------------------------------------------------------------------------------------------------------");
            Server.PrintToConsole("[#Id]\t[Name]\t\t[Designer Name]\t\t\t[[Description]");
            foreach (var human in Humans)
            {
                index++;
                Server.PrintToConsole($"#{index}\t{human.Name}\t\t{human.DesignerName}\t\t\t{human.Description}");
            }
            if(index == 0)
                Server.PrintToConsole("No human class.");
            Server.PrintToConsole("-----------------------------------------------------------------------------------------------------------------------");
        }
        else
        {
            var arg = cmdInfo.GetArg(1);
            if (string.IsNullOrEmpty(arg))
            {
                var index = 0;
                cmdInfo.ReplyToCommand($"{MessagePrefix}List of Human Classes:");
                cmdInfo.ReplyToCommand($"{MessagePrefix}#번호 | 클래스 명 | 부가 설명");
                foreach (var human in Humans)
                {
                    index++;
                    cmdInfo.ReplyToCommand($"{MessagePrefix}#{index} | {human.Name} | {human.Description}");
                }
                cmdInfo.ReplyToCommand($"{MessagePrefix}＊ Type \"css_hclass <number|classname>\" to choose your human class!");
            }
            else
            {
                BaseHuman? myHuman;
                
                //  번호로 찾을 때
                if (int.TryParse(arg, out var i))
                    myHuman = Humans.Where((_, x) => x == i - 1).FirstOrDefault();
                else
                    myHuman = Humans.Where(human => human.Name.Contains(arg)).FirstOrDefault();
                
                if (myHuman == null)
                    cmdInfo.ReplyToCommand($"{MessagePrefix}유효하지 않은 인간 진영 클래스입니다. Type \"css_hclass\" 명령어로 인간 진영 클래스 목록을 확인 해주세요.");
                else if (myHuman == BaseHumanPlayers[player.Slot].MyClass)
                    cmdInfo.ReplyToCommand($"{MessagePrefix}이미 선택된 클래스입니다.");
                else
                {
                    BaseHumanPlayers[player.Slot].SetClass(player, myHuman);
                    if (InGameStatus != GameStatus.Start && !BaseHalePlayers[player.Slot].IsHale)
                    {
                        player.SwitchTeam(CsTeam.Terrorist);
                        if (!player.PawnIsAlive)
                        {
                            player.Respawn();
                            BaseHumanPlayers[player.Slot].SetHumanClassState(player);
                        }
                    }
                }
            }
        }
    }
}
