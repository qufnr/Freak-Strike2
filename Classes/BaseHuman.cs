using System.Text.Json;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using FreakStrike2.Utils;

namespace FreakStrike2.Classes;

public class BaseHuman
{
    [JsonPropertyName("Name")] public required string Name { get; set; }                    //  인간 이름
    [JsonPropertyName("DesignerName")] public required string DesignerName { get; set; }    //  인간 클래스명
    [JsonPropertyName("Description")] public string? Description { get; set; }              //  인간 설명
    [JsonPropertyName("Model")] public string? Model { get; set; }                          //  플레이어 모델
    [JsonPropertyName("ArmsModel")] public string? ArmsModel { get; set; }                  //  플레이어 손 모델
    [JsonPropertyName("MaxHealth")] public int MaxHealth { get; set; } = 100;
    [JsonPropertyName("Laggedmovement")] public float Laggedmovement { get; set; } = 1f;    //  이동 속도
    [JsonPropertyName("Gravity")] public float Gravity { get; set; } = 1f;                  //  중력
    [JsonPropertyName("ExclusiveWeapons")] public List<string> ExclusiveWeapons { get; set; } = new() { "weapon_knife" };  //  전용 무기 (무기 이름:모델 이름)
    
    public static List<BaseHuman> GetHumansFromJson(string jsonString)
    {
        var humans = JsonSerializer.Deserialize<List<BaseHuman>>(jsonString);
        if (humans == null || humans.Count == 0)
            throw new NullReferenceException("No human found.");
        return humans;
    }

    /// <summary>
    /// 해당 인간 클래스 무기의 모델 List를 반환합니다.
    /// </summary>
    /// <returns>무기 모델 List. 없으면 빈 List를 반환합니다.</returns>
    public List<string> GetExclusiveWeaponModels()
    {
        var models = new List<string>();
        foreach (var weapon in ExclusiveWeapons)
        {
            var weaponFormat = !weapon.Contains(":") ? $"{weapon}:" : weapon;
            var explode = weaponFormat.Split(':');
            if (!string.IsNullOrEmpty(explode[1]) && explode[1].EndsWith(".vmdl"))
                models.Add(explode[1]);
        }

        return models;
    }

    /// <summary>
    /// 인간 클래스의 무기 이름과 무기 모델을 반환합니다.
    /// </summary>
    /// <param name="exclusiveWeapon">이름:모델</param>
    /// <returns>첫 번째 배열은 무기 이름, 두 번째 배열은 무기 모델을 반환합니다.</returns>
    private string[] GetExclusiveWeapon(string exclusiveWeapon)
    {
        return (!exclusiveWeapon.Contains(':') ? $"{exclusiveWeapon}:" : exclusiveWeapon).Split(':');
    }

    /// <summary>
    /// 플레이어를 해당 인간 클래스로 지정
    /// </summary>
    /// <param name="player">플레이어 객체</param>
    public void SetPlayer(CCSPlayerController player)
    {
        var playerPawn = player.PlayerPawn.Value;
        if (!player.PawnIsAlive || playerPawn == null || !playerPawn.IsValid)
            return;

        player.SetHealth(MaxHealth, true);
        player.SetHelmet(true);
        player.SetMovementSpeed(1.0f * Laggedmovement);
        playerPawn.GravityScale = Gravity;
        
        player.RemoveWeapons();

        var modelWeapons = new List<string[]>();
        
        foreach (var exclusiveWeapon in ExclusiveWeapons)
        {
            var explode = GetExclusiveWeapon(exclusiveWeapon);
            player.GiveNamedItem(explode[0]);

            if (!string.IsNullOrEmpty(explode[1]) && explode[1].EndsWith(".vmdl"))
                modelWeapons.Add(explode);
        }
        
        Server.NextFrame(() =>
        {
            if (!string.IsNullOrEmpty(Model))
                playerPawn.SetModel(Model); 
            
            foreach (var modelWeapon in modelWeapons)
            {
                var weapon = player.FindWeapon(modelWeapon[0]);
                if (weapon != null)
                    player.UpdateWeaponModel(weapon, modelWeapon[1], true);
            }
        });
    }
}