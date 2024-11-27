using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace FreakStrike2
{
    public partial class FreakStrike2
    {
        private static void RemoveEntities()
        {
            var entityNames = new List<string>() { "func_bomb_target", "func_hostage_rescue", "hostage_entity", "c4", "func_buyzone" };
            foreach (var entityName in entityNames)
            {
                var entities = Utilities.FindAllEntitiesByDesignerName<CEntityInstance>(entityName);
                foreach (var entity in entities)
                    entity.Remove();
            }
        }
    }
}
