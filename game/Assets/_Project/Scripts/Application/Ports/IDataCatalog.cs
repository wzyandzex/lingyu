using System.Collections.Generic;
using Aetherion.Domain.Battle;
using Aetherion.Domain.Creatures;
using Aetherion.Domain.Encounters;

namespace Aetherion.Application.Ports
{
    public interface IDataCatalog
    {
        bool TryGetCreature(CreatureDefId id, out CreatureDef def);
        int CreatureCount { get; }
        IEnumerable<CreatureDef> AllCreatures { get; }
        IEnumerable<CreatureDef> CreaturesInRegion(string regionId);
        bool TryGetEncounterTable(string id, out EncounterTable table);
        bool TryGetSkill(string skillId, out SkillDef skill);
        bool TryGetEnemy(string enemyId, out CreatureDef enemy);
        IEnumerable<SkillDef> AllSkills { get; }
    }
}
