namespace FreakStrike2.Models;

public enum GameStatus
{
    None = -1,  //  없음
    Warmup,     //  준비 시간
    Start,      //  게임 시작
    End,        //  게임 종료
    Ready,      //  게임 시작 전 준비
    PlayerWaiting   //  플레이어 기다림
}