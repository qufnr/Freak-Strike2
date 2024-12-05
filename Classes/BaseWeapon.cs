using System.Text.Json;

namespace FreakStrike2.Classes;

public class BaseWeapon
{
    public int Clip { get; set; }
    public int Ammo { get; set; }
    public float KnockbackScale { get; set; }
    public float KnockbackMaximumDistance { get; set; }
    public float FireRate { get; set; }
    public float Damage { get; set; }

    public static Dictionary<string, BaseWeapon> GetWeaponsFromJson(string jsonString)
    {
        var weapons = JsonSerializer.Deserialize<Dictionary<string, BaseWeapon>>(jsonString);
        if (weapons == null || weapons.Count <= 0)
            throw new Exception("Not found weapons.");

        return weapons;
    }
}