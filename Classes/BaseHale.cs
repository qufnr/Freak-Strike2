using System.Text.Json;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Utils;

namespace FreakStrike2.Classes;
public class BaseHale
{
    public static float DynamicJumpMaximumHoldTime = 1f;        //  높이 점프 홀드 최대치
    public static float DynamicJumpMinimumHoldTime = .3f;       //  높이 점프 홀드 최소치 (이 때(Tick) 부터 점프 가능)
    public static float DynamicJumpAngleXRange = -45f;          //  높이 점프 시전 시 시점 X 각도 범위
    
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
    [JsonPropertyName("CanUseRage")] public bool CanUseRage { get; set; } = true;           //  분노 사용 가능 여부
    [JsonPropertyName("CanUseDynamicJump")] public bool CanUseDynamicJump { get; set; } = true; //  높이 점프 사용 가능 여부
    [JsonPropertyName("CanUseWeightDown")] public bool CanUseWeightDown { get; set; } = true;   //  내려찍기 사용 가능 여부
    [JsonPropertyName("DynamicJumpVectorScale")] public float DynamicJumpVectorScale { get; set; } = 1f;    //  높이 점프 백터 값
    [JsonPropertyName("DynamicJumpCooldown")] public float DynamicJumpCooldown { get; set; } = 5f;          //  높이 점프 쿨다운 시간
    [JsonPropertyName("TaggingImmunityScale")] public float TaggingImmunityScale { get; set; } = 1f;    //  테깅 면역 (1.0 - 기본, 0.1 - 많음, 1.5 - 적음)
    [JsonPropertyName("KnockbackImmunityScale")] public float KnockbackImmunityScale { get; set; } = 1f;//  넉백 면역 (1.0 - 기본, 0.1 - 많음, 1.5 - 적음)
    [JsonPropertyName("AttackSpeed")] public float AttackSpeed { get; set; } = 1f;          //  공격 속도

    [JsonPropertyName("Weapons")] public List<string> Weapons { get; set; } = new(){ "weapon_knife" }; //  사용 무기
    
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

    /// <summary>
    /// 플레이어 수에 비례해서 체력을 계산합니다.
    /// </summary>
    /// <returns>최종 체력</returns>
    public int GetTotalMaxHealth()
    {
        var players = Utilities.GetPlayers()
            .Where(player => player.IsValid && player.PawnIsAlive && player.Team is CsTeam.Terrorist)
            .Count();
        return players > 1 
                ? Convert.ToInt32(MaxHealth * (HealthMultiplier * (1.0f + (players / 10.0f)))) 
                : MaxHealth;
    }

    /// <summary>
    /// 플레이어 수에 비례해서 방어력을 계산합니다.
    /// </summary>
    /// <returns>최종 방어력</returns>
    public int GetTotalArmor()
    {
        var players = Utilities.GetPlayers()
            .Where(player => player.IsValid && player.PawnIsAlive && player.Team is CsTeam.Terrorist)
            .Count();
        return players > 1 
            ? Convert.ToInt32(Armor * (ArmorMultiplier * (1.0f + (players / 10.0f)))) 
            : Armor;
    }

    public void EmitDeadSound()
    {
        if (DeathSoundEffects != null && DeathSoundEffects.Count > 0)
        {
            //  TODO :: 사망 효과음 재생
        }
    }

    public void EmitIntroSound()
    {
        if (IntroSoundEffects != null && IntroSoundEffects.Count > 0)
        {
            //  TODO :: 인트로 재생
        }
    }

    public void EmitJumpSound()
    {
        if (JumpSoundEffects != null && JumpSoundEffects.Count > 0)
        {
            //  TODO :: 점프 효과음 재생
        }
    }

    public void EmitHurtSound()
    {
        if (HurtSoundEffects != null && HurtSoundEffects.Count > 0)
        {
            //  TODO :: 피해 효과음 재생
        }
    }

    public void EmitRageSound()
    {
        if (RageSoundEffects != null && RageSoundEffects.Count > 0)
        {
            //  TODO :: 분노 효과음 재생
        }
    }

    public void EmitVictorySound()
    {
        if (VictorySoundEffects != null && VictorySoundEffects.Count > 0)
        {
            //  TODO :: 승리 효과음 재생
        }
    }

    public void EmitThemeSound()
    {
        if (BackgroundMusics != null && BackgroundMusics.Count > 0)
        {
            //  TODO :: 배경음 재생
        }
    }

    /// <summary>
    /// 플레이어 상태를 헤일로 업데이트
    /// </summary>
    /// <param name="player"></param>
    public void SetPlayerHaleState(CCSPlayerController player)
    {
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn is not null)
        {
            playerPawn.MaxHealth = GetTotalMaxHealth();
            playerPawn.ArmorValue = GetTotalArmor();
            playerPawn.Health = playerPawn.MaxHealth;
            playerPawn.VelocityModifier *= Laggedmovement;
            playerPawn.GravityScale = playerPawn.GravityScale;

            var itemServices = playerPawn.ItemServices;
            if (itemServices is not null)
            {
                CCSPlayer_ItemServices services = new(itemServices.Handle);
                services.HasHelmet = true;
                Utilities.SetStateChanged(playerPawn, "CBasePlayerPawn", "m_pItemServices");
            }
            
            player.RemoveWeapons();
            Weapons.ForEach(namedItem => player.GiveNamedItem(namedItem));
            
            Server.NextFrame(() =>
            {
                if (!string.IsNullOrEmpty(Model))
                    playerPawn.SetModel(Model);

                if (!string.IsNullOrEmpty(Viewmodel))
                {
                    var weapon = WeaponUtils.FindPlayerWeapon(player, "weapon_knife");
                    if (weapon is not null)
                        WeaponUtils.UpdatePlayerWeaponModel(player, weapon, Viewmodel, true);
                }
            });
        }
    }

    /// <summary>
    /// 플레이어를 헤일 스폰 지점으로 텔레포트를 시도합니다.
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    /// <returns>텔레포트하는데 성공했다면 true, 아니면 false 반환</returns>
    public bool TeleportToHaleSpawn(CCSPlayerController player)
    {
        var infoPlayerCounterterrorists = Utilities.FindAllEntitiesByDesignerName<CInfoPlayerCounterterrorist>("info_player_counterterrorist");
        
        var entities = new List<CInfoPlayerCounterterrorist>();
        foreach (var infoPlayerCounterterrorist in infoPlayerCounterterrorists)
            if (infoPlayerCounterterrorist.IsValid)
                entities.Add(infoPlayerCounterterrorist);
        
        if (entities.Count() > 0)
        {
            var candidate = CommonUtils.GetRandomInList(entities);
            if (candidate.IsValid && candidate.AbsOrigin is not null)
            {
                var origin = new Vector()
                {
                    X = candidate.AbsOrigin.X,
                    Y = candidate.AbsOrigin.Y + 1f,
                    Z = candidate.AbsOrigin.Z
                };
                
                player.Teleport(origin);
                
                return true;
            }
        }

        return false;
    }
}
