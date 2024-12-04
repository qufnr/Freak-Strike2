using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace FreakStrike2.Classes;

public class GameConfig : BasePluginConfig
{
    //  게임이 시작되기 전 준비 시간 (Freeze Time 별도)
    [JsonPropertyName("ReadyInterval")] public int ReadyInterval { get; set; } = 10;
    //  플레이어가 큐포인트를 초기화 할 수 있는지?
    [JsonPropertyName("CanResetQueuePoint")] public bool CanResetQueuePoint { get; set; } = true;
    //  GMod 처럼 낙하 피해를 바꿉니다.
    [JsonPropertyName("UnrealityFallDamage")] public bool UnrealityFallDamage { get; set; } = true;
    //  피해량 순위표 Row 수
    [JsonPropertyName("DamageRankRows")] public int DamageRankRows { get; set; } = 3;
    //  큐포인트 순위표 Row 수
    [JsonPropertyName("QueuePointRankRows")] public int QueuePointRankRows { get; set; } = 5;
    //  플레이어 접속 시 인간 진영 클래스를 무작위로 선택할지?
    [JsonPropertyName("RandomHumanClassOnJoin")] public bool RandomHumanClassOnJoin { get; set; } = false;
}