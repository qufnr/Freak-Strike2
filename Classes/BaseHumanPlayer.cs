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
            if (player != null && player.IsValid)
                player.ChangeTeam(CsTeam.Spectator);
        }
    }
}