using System.Text.Json;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Exceptions;
using FreakStrike2.Models.Human;
using FreakStrike2.Utils;
using FreakStrike2.Utils.Helpers;
using FreakStrike2.Utils.Helpers.Entity;

namespace FreakStrike2.Models.SaxtonHale;

public class FSSaxtonHale
{
    private static string Filename = "SaxtonHales.json";
    
    public static CsTeam Team = CsTeam.CounterTerrorist;
    public static int NormalMaxHealth = 100;
    public static int NormalArmorValue = 100;
    
    [JsonPropertyName("Name")] public required string Name { get; set; }
    [JsonPropertyName("DesignerName")] public required string DesignerName { get; set; }
    [JsonPropertyName("Description")] public Dictionary<string, string> Description { get; set; } = new();    //  "en": "*description*", "ko": "*설명*"
    
    [JsonPropertyName("Model")] public string? Model { get; set; }
    [JsonPropertyName("Weapons")] public List<string> Weapons { get; set; } = new(){ ":weapon_knife" };         //  "viewmodel_path:weapon_name", ":weapon_name" or "weapon_name"
    [JsonPropertyName("MaxHealth")] public int MaxHealth { get; set; } = NormalMaxHealth;
    [JsonPropertyName("MaxHealthMultiplier")] public float MaxHealthMultiplier { get; set; } = 1f;
    [JsonPropertyName("ArmorValue")] public int ArmorValue { get; set; } = NormalArmorValue;
    [JsonPropertyName("ArmorValueMuliplier")] public float ArmorValueMuliplier { get; set; } = 1f;
    [JsonPropertyName("Laggedmovement")] public float Laggedmovement { get; set; } = 1f;
    [JsonPropertyName("GravityScale")] public float GravityScale { get; set; } = 1f;

    [JsonPropertyName("CanUseWeightDown")] public bool CanUseWeightDown { get; set; } = true;
    [JsonPropertyName("WeightDownCooldown")] public float WeightDownCooldown { get; set; } = 5.0f;
    
    [JsonPropertyName("CanUseRage")] public bool CanUseRage { get; set; } = true;
    [JsonPropertyName("RageDistance")] public float RageDistance { get; set; } = 800.0f;
    [JsonPropertyName("RageStunDuration")] public float RageStunDuration { get; set; } = 3.0f;
    [JsonPropertyName("RageCooldown")] public float RageCooldown { get; set; } = 5.0f;
    
    [JsonPropertyName("CanUseSuperJump")] public bool CanUseSuperJump { get; set; } = true;
    [JsonPropertyName("SuperJumpVectorScale")] public float SuperJumpVectorScale { get; set; } = 1.0f;
    [JsonPropertyName("SuperJumpCooldown")] public float SuperJumpCooldown { get; set; } = 5.0f;
    
    [JsonPropertyName("IntroSoundEffects")] public List<string> IntroSoundEffects { get; set; } = new();
    [JsonPropertyName("KillSoundEffects")] public List<string> KillSoundEffects { get; set; } = new();
    [JsonPropertyName("DeathSoundEffects")] public List<string> DeathSoundEffects { get; set; } = new();
    [JsonPropertyName("RageSoundEffects")] public List<string> RageSoundEffects { get; set; } = new();
    [JsonPropertyName("SuperJumpSoundEffects")] public List<string> SuperJumpSoundEffects { get; set; } = new();
    [JsonPropertyName("WeightDownLandSoundEffects")] public List<string> WeightDownLandSoundEffects { get; set; } = new();
    
    [JsonPropertyName("Theme")] public List<string> Theme { get; set; } = new();

    /// <summary>
    /// Read a Saxton Hale configuration file.
    /// </summary>
    /// <returns>List of Saxton Hale</returns>
    /// <exception cref="SaxtonHaleUndefinedException">Undefined in this server.</exception>
    public static List<FSSaxtonHale> JsonToList()
    {
        var fullPath = Path.Combine(Server.GameDirectory, FreakStrike2.PluginConfigDirectory);
        var saxtonHales = CommonUtils.JsonFileToObject<List<FSSaxtonHale>>(fullPath, Filename);
        if (saxtonHales == null || saxtonHales.Count == 0)
            throw new SaxtonHaleUndefinedException();
        return saxtonHales;
    }

    /// <summary>
    /// Player status change to Saxton hale!
    /// </summary>
    /// <param name="player">Player Controller</param>
    /// <returns>Returns true if successful, otherwise false.</returns>
    public bool SetPlayerStatus(CCSPlayerController player)
    {
        if (!player.IsValid || !player.PawnIsAlive || player.PlayerPawn.Value is not CCSPlayerPawn playerPawn)
            return false;
        
        player.SetHealth(GetTotalMaxHealth(), true);
        player.SetArmorValue(GetTotalArmorValue());
        player.SetHelmet(true);
        player.SetMovementSpeed(1 * Laggedmovement);
        playerPawn.GravityScale = GravityScale;

        player.RemoveWeapons();

        var weaponData = PluginHelper.ExtractWeaponsFromConfig(Weapons);
        
        foreach (var weapon in weaponData)
            player.GiveNamedItem(weapon.Name);
        
        Server.NextFrame(() =>
        {
            player.ChangeTeam(FSSaxtonHale.Team);
            player.TeleportToSpawnPoint(FSSaxtonHale.Team);
        });
        
        Server.NextWorldUpdate(() =>
        {
            if(!string.IsNullOrEmpty(Model))
                playerPawn.SetModel(Model);

            foreach (var weapon in weaponData)
            {
                if (!string.IsNullOrEmpty(weapon.Model))
                {
                    var match = player.FindWeapon(weapon.Name);
                    if (match != null)
                        player.UpdateWeaponModel(match, weapon.Model, true);
                }
            }
        });

        return true;
    }

    /// <summary>
    /// Calculate the total sum for Saxton Hale healths.
    /// </summary>
    /// <returns>Total Max Health</returns>
    public int GetTotalMaxHealth() => (int) (MaxHealth * (1.0 + (PlayerUtils.GetTeamAlivePlayers(FSHuman.Team) / 10f)));

    /// <summary>
    /// Calculate the total sum for Saxton Hale armor values. 
    /// </summary>
    /// <returns>Total Armor Value</returns>
    public int GetTotalArmorValue() => (int) (ArmorValue * (1.0 + (PlayerUtils.GetTeamAlivePlayers(FSHuman.Team) / 10f)));
}