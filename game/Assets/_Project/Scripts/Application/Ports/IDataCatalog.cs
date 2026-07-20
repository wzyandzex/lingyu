using Aetherion.Domain.Creatures;

namespace Aetherion.Application.Ports
{
    public interface IDataCatalog
    {
        bool TryGetCreature(CreatureDefId id, out CreatureDef def);
        int CreatureCount { get; }
    }
}
