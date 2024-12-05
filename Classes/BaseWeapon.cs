using System.Text.Json;

namespace FreakStrike2.Classes;

public class BaseWeapon
{
    public int Clip { get; set; } = 0;
    public int Ammo { get; set; } = 0;
    public float KnockbackScale { get; set; } = 1.0f;
    public float KnockbackMaximumDistance { get; set; } = 800.0f;
    public float FireRate { get; set; } = 1.0f;
    public float Damage { get; set; } = 1.0f;

    public static Dictionary<string, BaseWeapon> GetWeaponsFromJson(string jsonString)
    {
        var weapons = JsonSerializer.Deserialize<Dictionary<string, BaseWeapon>>(jsonString);
        if (weapons == null || weapons.Count <= 0)
            throw new Exception("Not found weapons.");

        return weapons;
    }
}