using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Exceptions;
using FreakStrike2.Utils.Helpers.Entity;

namespace FreakStrike2.Classes;

public class BaseHumanPlayer
{
    private int _client;
    private CCSPlayerController? Player => Utilities.GetPlayerFromSlot(_client);
    
    public BaseHuman? MyClass { get; private set; } = null;
    public bool HasClass => MyClass != null;

    /// <summary>
    /// 기본 생성자
    /// </summary>
    /// <param name="client">플레이어 슬롯</param>
    public BaseHumanPlayer(int client)
    {
        _client = client;
        Reset();
    }

    public void SetClass(BaseHuman human)
    {
        if (Player == null || !Player.IsValid)
            return;
        
        if (Player.PawnIsAlive)
            Player.CommitSuicide(false, true);
        
        if (!Player.IsBot)
            Server.PrintToChatAll($"{FreakStrike2.MessagePrefix}{Player.PlayerName} 이(가) " + (MyClass != null ? $"인간 진영 클래스를 {human.Name} (으)로 변경했습니다." : $"{human.Name} 인간 진영 클래스를 선택했습니다."));
        
        MyClass = human;
    }
    
    /// <summary>
    /// 스폰 시 플레이어의 클래스로 설정합니다.
    /// </summary>
    public void SetHumanClassState()
    {
        Server.NextFrame(() =>
        {
            if (Player == null || !Player.IsValid || FreakStrike2.Instance.BaseHalePlayers[_client].IsHale)
                return;
            
            if (MyClass == null)
            {
                Player.PrintToChat($"{FreakStrike2.MessagePrefix}\"css_hclass <human>\" 명령어로 인간 진영 클래스를 선택 해주세요!");
                if(Player.PawnIsAlive)
                    Player.ChangeTeamOnNextFrame(CsTeam.Spectator);
                return;
            }
            
            MyClass.SetPlayer(Player);
        });
    }
    
    /// <summary>
    /// 플레이어 인간 클래스 초기화
    /// </summary>
    public void Reset()
    {
        MyClass = null;
        
        if (Player != null && Player.IsValid && !Player.IsBot)
            Player.ChangeTeamOnNextFrame(CsTeam.Spectator);
    }
}