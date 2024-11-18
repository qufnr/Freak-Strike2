using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
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
        RegisterEventHandler<EventRoundFreezeEnd>(OnRoundFreezeEnd);
        RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);
        RegisterEventHandler<EventItemEquip>(OnItemEquip);
        
        RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
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
        DeregisterEventHandler<EventRoundFreezeEnd>(OnRoundFreezeEnd);
        DeregisterEventHandler<EventRoundEnd>(OnRoundEnd);
        DeregisterEventHandler<EventPlayerHurt>(OnPlayerHurt);
        DeregisterEventHandler<EventItemEquip>(OnItemEquip);

        RemoveListener(OnServerPrecacheResources);
        RemoveListener(OnMapStart);
        RemoveListener(OnClientPutInServer);
        RemoveListener(OnClientDisconnect);
        RemoveListener(OnTick);
    }

    private void OnServerPrecacheResources(ResourceManifest manifest)
    {
        PrecacheHaleModels(manifest);
    }

    /// <summary>
    /// 맵 시작
    /// </summary>
    /// <param name="mapName">맵 이름</param>
    private void OnMapStart(string mapName)
    {
        KillGameTimer();
        CreateBasePlayerClasses();
    }

    /// <summary>
    /// 클라이언트 서버 접속
    /// </summary>
    /// <param name="client">클라이언트</param>
    private void OnClientPutInServer(int client)
    {
        _gamePlayer.CreateByPlayerSlot(client);
        _halePlayer.CreateByPlayerSlot(client);
        GameStartOnClientPutInServer();                 //  게임 시작 처리
        TeamChangeOnClientPutInServer(client);          //  접속 시 팀 변경 처리
    }

    /// <summary>
    /// 클라이언트 서버 퇴장
    /// </summary>
    /// <param name="client">클라이언트</param>
    private void OnClientDisconnect(int client)
    {
        _queuepoint.SetPlayerQueuepoint(client, 0); //  Queuepoint 초기화
        _halePlayer.Clear(Utilities.GetPlayerFromSlot(client), _gameStatus);
    }

    /// <summary>
    /// 틱
    /// </summary>
    private void OnTick()
    {
        DebugPrintGameCondition();  //  디버그 모드 출력
    }

    /// <summary>
    /// 라운드 시작
    /// </summary>
    /// <param name="event">이벤트</param>
    /// <param name="eventInfo">이벤트 정보</param>
    /// <returns>이벤트 훅</returns>
    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo eventInfo)
    {
        RemoveEntities();           //  맵에 불필요한 엔티티 제거
        CreateGameTimer(false);     //  게임 타이머 생성
        
        return HookResult.Continue;
    }

    /// <summary>
    /// 라운드 시작 전 대기 시간
    /// </summary>
    /// <param name="event">이벤트</param>
    /// <param name="eventInfo">이벤트 정보</param>
    /// <returns>이벤트 훅</returns>
    private HookResult OnRoundFreezeEnd(EventRoundFreezeEnd @event, GameEventInfo eventInfo)
    {
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
        
        _queuepoint.Calculate(_halePlayer, _gameStatus);   //  게임 종료 시 Queuepoint 계산 
        _gameStatus = GameStatus.End;
        
        return HookResult.Continue;
    }

    /// <summary>
    /// 플레이어 피해
    /// </summary>
    /// <param name="event">이벤트</param>
    /// <param name="eventInfo">이벤트 정보</param>
    /// <returns>이벤트 훅</returns>
    private HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo eventInfo)
    {
        var victim = @event.Userid;
        var attacker = @event.Attacker;
        var damage = @event.DmgHealth;
        var weapon = @event.Weapon;
        var hitgroup = @event.Hitgroup;
        
        _gamePlayer.AddPlayerDamage(victim, attacker, _halePlayer, _gameStatus, damage);
        
        if (GameNotStartDamageIgnoreOnPlayerHurt(victim, attacker))
            return HookResult.Handled;
        
        KnockbackOnPlayerTakeDamage(victim, attacker, damage, weapon, hitgroup);
        return HookResult.Continue;
    }

    private HookResult OnItemEquip(EventItemEquip @event, GameEventInfo eventInfo)
    {
        var player = @event.Userid;

        if (_halePlayer.PlayerIsHale(player))
            return HookResult.Handled;

        return HookResult.Continue;
    }
}