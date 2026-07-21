using System;
using System.Collections.Generic;
using Aetherion.Domain.Creatures;

namespace Aetherion.Domain.Codex
{
    /// <summary>Per-save codex unlock progress. Owned by WorldSession.</summary>
    public sealed class CodexProgress
    {
        private readonly Dictionary<string, HashSet<CodexLayer>> _layers =
            new Dictionary<string, HashSet<CodexLayer>>(StringComparer.Ordinal);

        public IReadOnlyCollection<string> UnlockedDefIds => _layers.Keys;

        public bool RegisterSighting(CreatureDefId defId)
        {
            return UnlockLayer(defId, CodexLayer.Appearance);
        }

        public bool UnlockLayer(CreatureDefId defId, CodexLayer layer)
        {
            if (string.IsNullOrEmpty(defId.Value))
                return false;

            if (!_layers.TryGetValue(defId.Value, out var set))
            {
                set = new HashSet<CodexLayer>();
                _layers[defId.Value] = set;
            }

            return set.Add(layer);
        }

        public bool IsUnlocked(CreatureDefId defId, CodexLayer layer)
        {
            return _layers.TryGetValue(defId.Value, out var set) && set.Contains(layer);
        }

        public bool HasAnyUnlock(CreatureDefId defId)
        {
            return _layers.TryGetValue(defId.Value, out var set) && set.Count > 0;
        }

        public IReadOnlyCollection<CodexLayer> GetLayers(CreatureDefId defId)
        {
            if (_layers.TryGetValue(defId.Value, out var set))
                return set;
            return Array.Empty<CodexLayer>();
        }

        public void Clear() => _layers.Clear();

        public void LoadFrom(IEnumerable<CodexEntryState> entries)
        {
            _layers.Clear();
            if (entries == null) return;
            foreach (var e in entries)
            {
                if (e == null || string.IsNullOrEmpty(e.DefId)) continue;
                var set = new HashSet<CodexLayer>();
                if (e.Layers != null)
                {
                    foreach (var layer in e.Layers)
                        set.Add(layer);
                }
                _layers[e.DefId] = set;
            }
        }

        public List<CodexEntryState> ToEntries()
        {
            var list = new List<CodexEntryState>(_layers.Count);
            foreach (var kv in _layers)
            {
                list.Add(new CodexEntryState
                {
                    DefId = kv.Key,
                    Layers = new List<CodexLayer>(kv.Value)
                });
            }
            return list;
        }
    }

    public sealed class CodexEntryState
    {
        public string DefId { get; set; }
        public List<CodexLayer> Layers { get; set; } = new List<CodexLayer>();
    }
}
