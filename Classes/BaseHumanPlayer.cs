using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace FreakStrike2.Classes;

public class BaseHumanPlayer
{
    public BaseHuman? MyClass { get; private set; } = null;
    public bool HasClass => MyClass != null;

    /// <summary>
    /// 기본 생성자
    /// </summary>
    /// <param name="slot">플레이어 슬롯</param>
    public BaseHumanPlayer(int slot = -1) => Reset(slot);

    public void SetClass(CCSPlayerController player, BaseHuman human)
    {
        MyClass = human;
        
        if (player.PawnIsAlive)
            player.CommitSuicide(false, true);
        
        if (!player.IsBot)
            Server.PrintToChatAll($"{FreakStrike2.MessagePrefix}{player.PlayerName} 이(가) " + (MyClass != null ? $"클래스를 {human.Name} (으)로 변경했습니다." : $"{human.Name} 클래스를 선택했습니다."));
    }
    
    /// <summary>
    /// 스폰 시 플레이어의 클래스로 설정합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    public void SetHumanClassState(CCSPlayerController player)
    {
        if (MyClass == null)
        {
            player.PrintToChat("[FS2] 인간 클래스를 선택하지 않았습니다!");
            if(player.PawnIsAlive)
                player.ChangeTeam(CsTeam.Spectator);
            return;
        }
        
        MyClass.SetPlayer(player);
    }
    
    /// <summary>
    /// 플레이어 인간 클래스 초기화
    /// </summary>
    /// <param name="slot"></param>
    public void Reset(int slot = -1)
    {
        MyClass = null;
        
        if (slot != -1)
        {
            var player = Utilities.GetPlayerFromSlot(slot);
            if (player != null && player.IsValid && !player.IsBot)
                player.ChangeTeam(CsTeam.Spectator);
        }
    }
}