using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Classes;
using FreakStrike2.Models;
using FreakStrike2.Utils;

namespace FreakStrike2;
public partial class FreakStrike2
{
    /// <summary>
    /// 이벤트 등록
    /// </summary>
    private void GameEventRegister()
    {
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventRoundFreezeEnd>(OnRoundFreezeEnd);
        RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        RegisterEventHandler<EventWeaponFire>(OnWeaponFirePre, HookMode.Pre);
        
        RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
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
        DeregisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        DeregisterEventHandler<EventWeaponFire>(OnWeaponFirePre, HookMode.Pre);

        RemoveListener(OnServerPrecacheResources);
        RemoveListener(OnMapStart);
        RemoveListener(OnMapEnd);
        RemoveListener(OnClientPutInServer);
        RemoveListener(OnClientDisconnect);
        RemoveListener(OnTick);
    }

    private void OnServerPrecacheResources(ResourceManifest manifest)
    {
        PrecacheHaleModels(manifest);
        PrecacheHumanModels(manifest);
    }

    /// <summary>
    /// 맵 시작
    /// </summary>
    /// <param name="mapName">맵 이름</param>
    private void OnMapStart(string mapName)
    {
        BaseGamePlayers = new Dictionary<int, BaseGamePlayer>(Server.MaxPlayers);
        BaseHumanPlayers = new Dictionary<int, BaseHumanPlayer>(Server.MaxPlayers);
        BaseHalePlayers = new Dictionary<int, BaseHalePlayer>(Server.MaxPlayers);
        PlayerQueuePoints = new Dictionary<int, BaseQueuePoint>(Server.MaxPlayers);
        
        KillInGameTimer();
        CreateInGameGlobalTimer();
    }

    private void OnMapEnd()
    {
        KillInGameGlobalTimer();
    }

    /// <summary>
    /// 클라이언트 서버 접속
    /// </summary>
    /// <param name="client">클라이언트</param>
    private void OnClientPutInServer(int client)
    {
        BaseGamePlayers[client] = new BaseGamePlayer();
        BaseHumanPlayers[client] = new BaseHumanPlayer(client);
        BaseHalePlayers[client] = new BaseHalePlayer();
        PlayerQueuePoints[client] = new BaseQueuePoint();
        
        GameStartOnClientPutInServer();                 //  게임 시작 처리
        TeamChangeOnClientPutInServer(client);          //  접속 시 팀 변경 처리
    }

    /// <summary>
    /// 클라이언트 서버 퇴장
    /// </summary>
    /// <param name="client">클라이언트</param>
    private void OnClientDisconnect(int client)
    {
        PlayerQueuePoints.Remove(client);
        BaseGamePlayers.Remove(client);
        BaseHalePlayers[client].Remove(client, InGameStatus);
        BaseHalePlayers.Remove(client);
    }

    /// <summary>
    /// 틱
    /// </summary>
    private void OnTick()
    {
    }

    /// <summary>
    /// 라운드 시작
    /// </summary>
    /// <param name="event">이벤트</param>
    /// <param name="eventInfo">이벤트 정보</param>
    /// <returns>훅 결과</returns>
    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo eventInfo)
    {
        RemoveEntities();                       //  맵에 불필요한 엔티티 제거
        CreateInGameTimer();                    //  게임 타이머 생성
        CreateRoundTimerOnRoundStart();         //  라운드 타이머 생성
        
        Utilities.GetPlayers()
            .Where(player => player.IsValid)
            .ToList()
            .ForEach(player =>
            {
                BaseGamePlayers[player.Slot].Reset(player); //  게임 플레이어 초기화
                BaseHalePlayers[player.Slot].Remove();      //  헤일 플레이어 초기화
            });
        
        CreateHalePlayerOnRoundStart();         //  헤일 플레이어 선정
        
        return HookResult.Continue;
    }

    /// <summary>
    /// 라운드 시작 전 대기 시간
    /// </summary>
    /// <param name="event">이벤트</param>
    /// <param name="eventInfo">이벤트 정보</param>
    /// <returns>훅 결과</returns>
    private HookResult OnRoundFreezeEnd(EventRoundFreezeEnd @event, GameEventInfo eventInfo)
    {
        CreateInGameTimer();

        return HookResult.Continue;
    }

    /// <summary>
    /// 라운드 종료 
    /// </summary>
    /// <param name="event">이벤트</param>
    /// <param name="eventInfo">이벤트 정보</param>
    /// <returns>훅 결과</returns>
    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo eventInfo)
    {
        DistributeQueuePointsOnRoundEnd();
        
        InGameStatus = GameStatus.End;
        
        return HookResult.Continue;
    }

    /// <summary>
    /// 플레이어 피해
    /// </summary>
    /// <param name="event">이벤트</param>
    /// <param name="eventInfo">이벤트 정보</param>
    /// <returns>훅 결과</returns>
    private HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo eventInfo)
    {
        var victim = @event.Userid;
        var attacker = @event.Attacker;
        var damage = @event.DmgHealth;
        var weapon = @event.Weapon;
        var hitgroup = @event.Hitgroup;

        if (victim != null && victim.IsValid && attacker != null && attacker.IsValid)
        {
            //  피해량 추가
            AddDamageOnPlayerHurt(victim, attacker, damage);
            KnockbackOnPlayerHurt(victim, attacker, damage, weapon, hitgroup);    //  넉백 계산
        }
        
        return HookResult.Continue;
    }

    /// <summary>
    /// 플레이어 스폰
    /// </summary>
    /// <param name="event">이벤트</param>
    /// <param name="eventInfo">이벤트 정보</param>
    /// <returns>훅 결과</returns>
    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo eventInfo)
    {
        var player = @event.Userid;

        if (player != null && player.IsValid)
        {
            BaseGamePlayers[player.Slot].Damages = 0; //  피해량 초기화
        }

        return HookResult.Continue;
    }

    /// <summary>
    /// 무기 발사
    /// </summary>
    /// <param name="event">이벤트</param>
    /// <param name="eventInfo">이벤트 정보</param>
    /// <returns>훅 결과</returns>
    private HookResult OnWeaponFirePre(EventWeaponFire @event, GameEventInfo eventInfo)
    {
        var player = @event.Userid;
        
        if (player != null && player.IsValid)
        {
            //  TODO :: 테스트 후 삭제
            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn != null && playerPawn.IsValid)
            {
                var activeWeapon = playerPawn.WeaponServices!.ActiveWeapon.Value;
                if (activeWeapon != null)
                {
                    activeWeapon.As<CCSWeaponBase>().FlRecoilIndex = 0;
                    activeWeapon.As<CCSWeaponBase>().AccuracyPenalty = 0;
                }
            }
        }

        return HookResult.Continue;
    }
}