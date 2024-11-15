using System.Text.Json;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API;

namespace FreakStrike2.Classes;
public class BaseHale
{
    [JsonPropertyName("Name")] public required string Name { get; set; }                    //  헤일 이름
    [JsonPropertyName("DesignerName")] public required string DesignerName { get; set; }    //  헤일 클래스명
    [JsonPropertyName("Description")] public string? Description { get; set; }              //  설명
    [JsonPropertyName("Model")] public string? Model { get; set; }                          //  플레이어 모델
    [JsonPropertyName("ArmsModel")] public string? ArmsModel { get; set; }                  //  플레이어 손 모델
    [JsonPropertyName("Viewmodel")] public string? Viewmodel { get; set; }                  //  플레이어 뷰 모델
    [JsonPropertyName("MaxHealth")] public int MaxHealth { get; set; } = 1000;              //  최대 체력
    [JsonPropertyName("HealthMultiplier")] public float HealthMultiplier { get; set; } = 1f;//  플레이어 비례 체력 증가 값 (인원수 * HealthMultiplier)    //  0.0 이하일 경우 증가하지 않음
    [JsonPropertyName("Armor")] public int Armor { get; set; } = 0;                         //  방어구
    [JsonPropertyName("ArmorMultiplier")] public float ArmorMultiplier { get; set; } = 1f;  //  플레이어 비례 방어구 증가 값
    [JsonPropertyName("Laggedmovement")] public float Laggedmovement { get; set; } = 1f;    //  이동 속도
    [JsonPropertyName("Gravity")] public float Gravity { get; set; } = 1f;                  //  중력
    [JsonPropertyName("CanUseSkill")] public bool CanUseSkill { get; set; } = true;         //  스킬 사용 가능 여부
    [JsonPropertyName("JumpVectorScale")] public float JumpVectorScale { get; set; } = 1f;  //  슈퍼 점프 백터 값
    [JsonPropertyName("TaggingImmunityScale")] public float TaggingImmunityScale { get; set; } = 1f;    //  테깅 면역 (1.0 - 기본, 0.1 - 많음, 1.5 - 적음)
    [JsonPropertyName("KnockbackImmunityScale")] public float KnockbackImmunityScale { get; set; } = 1f;//  넉백 면역 (1.0 - 기본, 0.1 - 많음, 1.5 - 적음)
    [JsonPropertyName("AttackSpeed")] public float AttackSpeed { get; set; } = 1f;          //  공격 속도
    
    [JsonPropertyName("SFXDeath")] public List<string>? DeathSoundEffects { get; set; }             //  사망 효과음
    [JsonPropertyName("SFXIntro")] public List<string>? IntroSoundEffects { get; set; }             //  인트로 효과음 (헤일 등장)
    [JsonPropertyName("SFXJump")] public List<string>? JumpSoundEffects { get; set; }               //  점프 효과음
    [JsonPropertyName("SFXKillEnemy")] public List<string>? KillEnemySoundEffects { get; set; }     //  적 처치 효과음
    [JsonPropertyName("SFXHurt")] public List<string>? HurtSoundEffects { get; set; }               //  피해 효과음
    [JsonPropertyName("SFXRage")] public List<string>? RageSoundEffects { get; set; }               //  분노 효과음
    [JsonPropertyName("SFXVictory")] public List<string>? VictorySoundEffects { get; set; }         //  승리 효과음
    
    [JsonPropertyName("BGMTheme")] public List<string>? BackgroundMusics { get; set; }              //  테마 음악

    /// <summary>
    /// 헤일 설정 JSON 파일로 부터 헤일 정보 찾기
    /// </summary>
    /// <param name="jsonString">JSON 파일 내용</param>
    /// <returns>파싱된 헤일 정보 목록</returns>
    /// <exception cref="NullReferenceException">예외 던지기</exception>
    public static List<BaseHale> GetHalesFromJson(string jsonString)
    {
        var hales = JsonSerializer.Deserialize<List<BaseHale>>(jsonString);
        if (hales is null || hales.Count is 0)
            throw new NullReferenceException("No hale found.");
        return hales;
    }
}
