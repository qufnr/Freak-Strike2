using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using FreakStrike2.Models;

namespace FreakStrike2.Classes;

public class BaseGamePlayer
{
    public int Damages { get; set; }            //  플레이어가 입힌 피해량
    // public bool IsStunned { get; private set; } //  스턴 상태 여부
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
}