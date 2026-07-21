using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aetherion.Application.Ports;
using Aetherion.Domain.Battle;
using Aetherion.Domain.Creatures;
using Aetherion.Domain.Encounters;
using UnityEngine;

namespace Aetherion.Infrastructure.DataLoading
{
    [Serializable]
    internal sealed class CreatureJsonDto
    {
        public string id;
        public string name_key;
        public string aspect_primary;
        public string aspect_secondary;
        public string size_class;
        public string discovery_tier;
        public string[] regions;
        public string[] skills;
        public BaseStatsDto base_stats;
        public BondingDto bonding;
        public CodexDto codex;
        public string view_key;
    }

    [Serializable]
    internal sealed class BaseStatsDto
    {
        public int hp;
        public int atk;
        public int def;
        public int spa;
        public int spd;
        public int spe;
    }

    [Serializable]
    internal sealed class BondingDto
    {
        public string template;
    }

    [Serializable]
    internal sealed class CodexDto
    {
        public string science_key;
        public string poem_key;
    }

    [Serializable]
    internal sealed class EncounterJsonDto
    {
        public string id;
        public string region;
        public EncounterEntryDto[] entries;
    }

    [Serializable]
    internal sealed class EncounterEntryDto
    {
        public string def_id;
        public float weight;
    }

    [Serializable]
    internal sealed class SkillJsonDto
    {
        public string id;
        public string name_key;
        public string element;
        public int power;
    }

    public sealed class JsonDataCatalog : IDataCatalog
    {
        private readonly Dictionary<string, CreatureDef> _creatures =
            new Dictionary<string, CreatureDef>(StringComparer.Ordinal);
        private readonly Dictionary<string, CreatureDef> _enemies =
            new Dictionary<string, CreatureDef>(StringComparer.Ordinal);
        private readonly Dictionary<string, EncounterTable> _encounters =
            new Dictionary<string, EncounterTable>(StringComparer.Ordinal);
        private readonly Dictionary<string, SkillDef> _skills =
            new Dictionary<string, SkillDef>(StringComparer.Ordinal);

        public int CreatureCount => _creatures.Count;
        public IEnumerable<CreatureDef> AllCreatures => _creatures.Values;
        public IEnumerable<SkillDef> AllSkills => _skills.Values;

        public void LoadCreatures(string creaturesDirectory)
        {
            _creatures.Clear();
            LoadCreatureFolder(creaturesDirectory, _creatures, "creature");
        }

        public void LoadEnemies(string enemiesDirectory)
        {
            _enemies.Clear();
            LoadCreatureFolder(enemiesDirectory, _enemies, "enemy");
        }

        public void LoadSkills(string skillsDirectory)
        {
            _skills.Clear();
            if (string.IsNullOrEmpty(skillsDirectory) || !Directory.Exists(skillsDirectory))
            {
                Debug.LogWarning($"[DataCatalog] Skills directory missing: {skillsDirectory}");
                return;
            }
            foreach (var path in Directory.GetFiles(skillsDirectory, "*.json"))
            {
                try
                {
                    var dto = JsonUtility.FromJson<SkillJsonDto>(File.ReadAllText(path));
                    if (dto == null || string.IsNullOrEmpty(dto.id)) continue;
                    _skills[dto.id] = new SkillDef
                    {
                        Id = dto.id,
                        NameKey = dto.name_key ?? dto.id,
                        Element = dto.element ?? string.Empty,
                        Power = dto.power
                    };
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[DataCatalog] Skill {path}: {ex.Message}");
                }
            }
            Debug.Log($"[DataCatalog] Loaded {_skills.Count} skill(s)");
        }

        public void LoadEncounters(string encountersDirectory)
        {
            _encounters.Clear();
            if (string.IsNullOrEmpty(encountersDirectory) || !Directory.Exists(encountersDirectory))
            {
                Debug.LogWarning($"[DataCatalog] Encounters directory missing: {encountersDirectory}");
                return;
            }
            foreach (var path in Directory.GetFiles(encountersDirectory, "*.json"))
            {
                try
                {
                    var dto = JsonUtility.FromJson<EncounterJsonDto>(File.ReadAllText(path));
                    if (dto == null || string.IsNullOrEmpty(dto.id)) continue;
                    var entries = new List<EncounterEntry>();
                    if (dto.entries != null)
                    {
                        foreach (var e in dto.entries)
                        {
                            if (e == null || string.IsNullOrEmpty(e.def_id)) continue;
                            entries.Add(new EncounterEntry
                            {
                                DefId = CreatureDefId.Parse(e.def_id),
                                Weight = e.weight <= 0f ? 1f : e.weight
                            });
                        }
                    }
                    _encounters[dto.id] = new EncounterTable
                    {
                        Id = dto.id,
                        Region = dto.region ?? string.Empty,
                        Entries = entries
                    };
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[DataCatalog] Encounter {path}: {ex.Message}");
                }
            }
        }

        public void LoadFromDirectory(string creaturesDirectory) => LoadCreatures(creaturesDirectory);

        public bool TryGetCreature(CreatureDefId id, out CreatureDef def) =>
            _creatures.TryGetValue(id.Value, out def);

        public bool TryGetEnemy(string enemyId, out CreatureDef enemy) =>
            _enemies.TryGetValue(enemyId, out enemy);

        public bool TryGetSkill(string skillId, out SkillDef skill) =>
            _skills.TryGetValue(skillId, out skill);

        public IEnumerable<CreatureDef> CreaturesInRegion(string regionId)
        {
            if (string.IsNullOrEmpty(regionId))
                return Array.Empty<CreatureDef>();
            return _creatures.Values.Where(c => c.Regions != null && c.Regions.Contains(regionId));
        }

        public bool TryGetEncounterTable(string id, out EncounterTable table) =>
            _encounters.TryGetValue(id, out table);

        private static void LoadCreatureFolder(string dir, Dictionary<string, CreatureDef> map, string label)
        {
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            {
                Debug.LogWarning($"[DataCatalog] {label} directory missing: {dir}");
                return;
            }
            foreach (var path in Directory.GetFiles(dir, "*.json"))
            {
                try
                {
                    var dto = JsonUtility.FromJson<CreatureJsonDto>(File.ReadAllText(path));
                    if (dto == null || string.IsNullOrEmpty(dto.id)) continue;
                    var def = ToDef(dto);
                    map[def.Id.Value] = def;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[DataCatalog] {label} {path}: {ex.Message}");
                }
            }
            Debug.Log($"[DataCatalog] Loaded {map.Count} {label}(s) from {dir}");
        }

        private static CreatureDef ToDef(CreatureJsonDto dto)
        {
            return new CreatureDef
            {
                Id = CreatureDefId.Parse(dto.id),
                NameKey = dto.name_key ?? string.Empty,
                AspectPrimary = dto.aspect_primary ?? string.Empty,
                AspectSecondary = dto.aspect_secondary,
                SizeClass = dto.size_class ?? string.Empty,
                DiscoveryTier = dto.discovery_tier ?? string.Empty,
                Regions = dto.regions ?? Array.Empty<string>(),
                SkillIds = dto.skills ?? Array.Empty<string>(),
                BondingTemplate = dto.bonding != null ? dto.bonding.template ?? string.Empty : string.Empty,
                ViewKey = dto.view_key ?? string.Empty,
                CodexScienceKey = dto.codex != null ? dto.codex.science_key ?? string.Empty : string.Empty,
                CodexPoemKey = dto.codex != null ? dto.codex.poem_key ?? string.Empty : string.Empty,
                BaseHp = dto.base_stats != null && dto.base_stats.hp > 0 ? dto.base_stats.hp : 40,
                BaseAtk = dto.base_stats != null && dto.base_stats.atk > 0 ? dto.base_stats.atk : 10,
                BaseDef = dto.base_stats != null && dto.base_stats.def > 0 ? dto.base_stats.def : 10
            };
        }
    }
}
