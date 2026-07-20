using System;
using System.Collections.Generic;
using System.IO;
using Aetherion.Application.Ports;
using Aetherion.Domain.Creatures;
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

    /// <summary>
    /// Loads creature definitions from StreamingAssets/data (Player)
    /// or repository data/ (Editor dual-mode).
    /// Uses Unity JsonUtility for VS0 (field names use snake_case via DTO).
    /// </summary>
    public sealed class JsonDataCatalog : IDataCatalog
    {
        private readonly Dictionary<string, CreatureDef> _creatures =
            new Dictionary<string, CreatureDef>(StringComparer.Ordinal);

        public int CreatureCount => _creatures.Count;

        public void LoadFromDirectory(string creaturesDirectory)
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

            Debug.Log($"[DataCatalog] Loaded {_creatures.Count} creature definition(s) from {creaturesDirectory}");
        }

        public bool TryGetCreature(CreatureDefId id, out CreatureDef def) =>
            _creatures.TryGetValue(id.Value, out def);

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
