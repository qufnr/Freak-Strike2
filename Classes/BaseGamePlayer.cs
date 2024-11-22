using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using FreakStrike2.Models;

namespace FreakStrike2.Classes;

public class BaseGamePlayer
{
    public int Damages { get; set; }            //  플레이어가 입힌 피해량
    public bool DebugMode { get; set; }         //  디버그 모드 활성화 여부

    public BaseGamePlayer()
    {
        Damages = 0;
        DebugMode = false;
    }

    /// <summary>
    /// 플레이어의 디버그 모드를 토글합니다.
    /// </summary>
    /// <returns>디버그 모드 여부</returns>
    public bool ToggleDebugMode()
    {
        DebugMode = !DebugMode;
        return DebugMode;
    }
    
    /// <summary>
    /// 플레이어의 입힌 피해량을 추가합니다. (OnPlayerHurt)
    /// </summary>
    /// <param name="victim">피해자</param>
    /// <param name="attacker">가해자</param>
    /// <param name="baseHalePlayers">플레이어 헤일 정보</param>
    /// <param name="gameStatus">게임 상태</param>
    /// <param name="damage">피해량</param>
    public void AddPlayerDamage(CCSPlayerController? victim, CCSPlayerController? attacker, Dictionary<int, BaseHalePlayer> baseHalePlayers, GameStatus gameStatus, int damage)
    {
        if (victim is not null && attacker is not null &&
            gameStatus is GameStatus.Start &&
            baseHalePlayers[victim.Slot].IsHale &&
            !baseHalePlayers[attacker.Slot].IsHale)
            Damages += damage;
    }
}