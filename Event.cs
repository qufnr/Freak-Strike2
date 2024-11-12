using CounterStrikeSharp.API.Core;
using FreakStrike2.Models;

namespace FreakStrike2;
public partial class FreakStrike2
{
    /// <summary>
    /// 이벤트 등록
    /// </summary>
    private void GameEventInitialize()
    {
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        RegisterListener<Listeners.OnClientPutInServer>(OnClientPutInServer);
        RegisterListener<Listeners.OnClientDisconnect>(OnClientDisconnect);
        RegisterListener<Listeners.OnTick>(OnTick);
    }

    /// <summary>
    /// 이벤트 제거
    /// </summary>
    private void GameEventDeregister()
    {
        DeregisterEventHandler<EventRoundStart>(OnRoundStart);
        DeregisterEventHandler<EventRoundEnd>(OnRoundEnd);

        RemoveListener(OnMapStart);
        RemoveListener(OnClientPutInServer);
        RemoveListener(OnClientDisconnect);
        RemoveListener(OnTick);
    }

    /// <summary>
    /// 맵 시작
    /// </summary>
    /// <param name="mapName">맵 이름</param>
    private void OnMapStart(string mapName)
    {
        KillGameTimer();
        CreatePlayerQueuepoint();
    }

    /// <summary>
    /// 클라이언트 서버 접속
    /// </summary>
    /// <param name="client">클라이언트</param>
    private void OnClientPutInServer(int client)
    {
        GameStartOnClientPutInServer();
        DebugDisableOnClientPutInServer(client);
        TeamChangeOnClientPutInServer(client);
        // var player = Utilities.GetPlayerFromSlot(client);
        // if (player is not null && player.IsValid && !player.IsBot && !player.IsHLTV)
        // {
        //     Logger.LogInformation($"[FreakStrike2] Player {player.PlayerName} ({player.AuthorizedSteamID?.SteamId64}) is put in server with {player.Slot}.");
        //
        // }
    }

    /// <summary>
    /// 클라이언트 서버 퇴장
    /// </summary>
    /// <param name="client">클라이언트</param>
    private void OnClientDisconnect(int client)
    {
        ResetQueuepointOnClientDisconnect(client);
    }

    /// <summary>
    /// 틱
    /// </summary>
    private void OnTick()
    {
        DebugPrintGameCondition();
    }

    /// <summary>
    /// 라운드 시작
    /// </summary>
    /// <param name="event">이벤트</param>
    /// <param name="eventInfo">이벤트 정보</param>
    /// <returns>이벤트 훅</returns>
    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo eventInfo)
    {
        RemoveEntities();
        CreateGameTimer();
        
        return HookResult.Continue;
    }

    /// <summary>
    /// 라운드 종료 
    /// </summary>
    /// <param name="event">이벤트</param>
    /// <param name="eventInfo">이벤트 정보</param>
    /// <returns>이벤트 훅</returns>
    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo eventInfo)
    {
        if (_gameStatus is GameStatus.PlayerWaiting)
        {
            return HookResult.Handled;
        }
        
        _gameStatus = GameStatus.End;
        _queuepoint.CalculatePlayerQueuepoints(_halePlayers);
        
        return HookResult.Continue;
    }
}