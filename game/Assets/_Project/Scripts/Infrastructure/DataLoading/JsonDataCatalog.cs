using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aetherion.Application.Ports;
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
        public BondingDto bonding;
        public CodexDto codex;
        public string view_key;
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

    public sealed class JsonDataCatalog : IDataCatalog
    {
        private readonly Dictionary<string, CreatureDef> _creatures =
            new Dictionary<string, CreatureDef>(StringComparer.Ordinal);
        private readonly Dictionary<string, EncounterTable> _encounters =
            new Dictionary<string, EncounterTable>(StringComparer.Ordinal);

        public int CreatureCount => _creatures.Count;
        public IEnumerable<CreatureDef> AllCreatures => _creatures.Values;

        public void LoadCreatures(string creaturesDirectory)
        {
            _creatures.Clear();
            if (string.IsNullOrEmpty(creaturesDirectory) || !Directory.Exists(creaturesDirectory))
            {
                Debug.LogWarning($"[DataCatalog] Creatures directory missing: {creaturesDirectory}");
                return;
            }

            foreach (var path in Directory.GetFiles(creaturesDirectory, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(path);
                    var dto = JsonUtility.FromJson<CreatureJsonDto>(json);
                    if (dto == null || string.IsNullOrEmpty(dto.id))
                    {
                        Debug.LogWarning($"[DataCatalog] Skip invalid creature file: {path}");
                        continue;
                    }

                    var def = ToDef(dto);
                    _creatures[def.Id.Value] = def;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[DataCatalog] Failed to load {path}: {ex.Message}");
                }
            }

            Debug.Log($"[DataCatalog] Loaded {_creatures.Count} creature(s) from {creaturesDirectory}");
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
                    var json = File.ReadAllText(path);
                    var dto = JsonUtility.FromJson<EncounterJsonDto>(json);
                    if (dto == null || string.IsNullOrEmpty(dto.id))
                        continue;

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
                    Debug.LogError($"[DataCatalog] Failed encounter {path}: {ex.Message}");
                }
            }

            Debug.Log($"[DataCatalog] Loaded {_encounters.Count} encounter table(s)");
        }

        // Back-compat name used by VS0 boot
        public void LoadFromDirectory(string creaturesDirectory) => LoadCreatures(creaturesDirectory);

        public bool TryGetCreature(CreatureDefId id, out CreatureDef def) =>
            _creatures.TryGetValue(id.Value, out def);

        public IEnumerable<CreatureDef> CreaturesInRegion(string regionId)
        {
            if (string.IsNullOrEmpty(regionId))
                return Array.Empty<CreatureDef>();
            return _creatures.Values.Where(c => c.Regions != null && c.Regions.Contains(regionId));
        }

        public bool TryGetEncounterTable(string id, out EncounterTable table) =>
            _encounters.TryGetValue(id, out table);

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
                BondingTemplate = dto.bonding != null ? dto.bonding.template ?? string.Empty : string.Empty,
                ViewKey = dto.view_key ?? string.Empty,
                CodexScienceKey = dto.codex != null ? dto.codex.science_key ?? string.Empty : string.Empty,
                CodexPoemKey = dto.codex != null ? dto.codex.poem_key ?? string.Empty : string.Empty
            };
        }
    }
}
