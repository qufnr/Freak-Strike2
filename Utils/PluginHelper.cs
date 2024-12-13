namespace FreakStrike2.Utils;

public class PluginHelper
{
    /// <summary>
    /// Converts the "Weapons" section in config file into a tuple data.
    /// </summary>
    /// <param name="weapons">"Weapons" section in config file Data.</param>
    /// <returns>Weapon Model and Name tuple list.</returns>
    public static List<(string Model, string Name)> ExtractWeaponsFromConfig(List<string> weapons) =>
        weapons.Select(w =>
        {
            if (!w.Contains(":"))
                return (Model: string.Empty, Name: w);
            var explode = w.Split(':');
            return (Model: explode[0], Name: explode[1]);
        }).ToList();
}