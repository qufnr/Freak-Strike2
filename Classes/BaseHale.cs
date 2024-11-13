using Newtonsoft.Json;

namespace FreakStrike2.Classes;
public class BaseHale
{
    public string Name { get; set; }                            //  헤일 이름
    public string Description { get; set; } = "No Description"; //  설명
    public string DesignerName { get; set; }    //  헤일 클래스명 (API 에서 사용)
    public string Model { get; set; }           //  플레이어 모델
    public string ArmsModel { get; set; }       //  플레이어 손 모델
    public string Viewmodel { get; set; }       //  플레이어 뷰모델
    public int MaxHealth { get; set; } = 1000;          //  최대 체력
    public float HealthMultiplier { get; set; } = 1.0f; //  체력 플레이어 비례 증가 값 (인원 수 * HealthMultipler) // 0.0 이하일 경우 플레이어에 비례하여 체력 증가하지 않음
    public int Armor { get; set; } = 0;                 //  방어구
    public float ArmorMultiplier { get; set; } = 1.0f;  //  방어구 플레이어 비례 증가 값
    public float Laggedmovement { get; set; } = 1.0f;   //  이동 속도
    public float Gravity { get; set; } = 1.0f;          //  중력
    public bool CanUseSkill { get; set; } = true;       //  스킬 사용가능 여부
    public float JumpVectorScale { get; set; } = 1.0f;  //  슈퍼 점프 백터
    public float TaggingImmunityScale { get; set; } = 1.0f;     //  태깅 면역 (1.0 - 기본, 0.1 - 많음, 1.5 - 적음)
    public float KnockbackImmunityScale { get; set; } = 1.0f;   //  넉백 면역 (1.0 - 기본, 0.1 - 많음, 1.5 - 적음)
    public float DamageMultiplier { get; set; } = 1.0f;         //  피해 가해량 증가 값 (기본 피해량 * DamageMultipler)
    public float AttackSpeed { get; set; } = 1.0f;              //  공격 속도
    
    public List<string> SFXDeath { get; set; }      //  사망 SFX
    public List<string> SFXIntro { get; set; }      //  인트로 (헤일 등장) SFX
    public List<string> SFXJump { get; set; }       //  점프 SFX
    public List<string> SFXKillEnemy { get; set; }  //  적 처치 SFX
    public List<string> SFXHurt { get; set; }       //  피해 SFX
    public List<string> SFXRage { get; set; }       //  분노 SFX
    public List<string> SFXVictory { get; set; }    //  승리 SFX
    
    public List<string> BGMTheme { get; set; }      //  테마 음악

    /**
     * 헤일 설정 JSON 파일로 부터 헤일 정보 찾기
     */
    public static List<BaseHale> GetHalesFromJson(string jsonString)
    {
        var hales = JsonConvert.DeserializeObject<List<BaseHale>>(jsonString);
        if (hales is null || hales.Count is 0)
        {
            throw new NullReferenceException("No hale found.");
        }

        return hales;
    }
}
