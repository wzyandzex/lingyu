using System;
using System.Collections.Generic;
using Aetherion.Domain.Creatures;

namespace Aetherion.Domain.Party
{
    public sealed class CreatureInstance
    {
        public string InstanceId { get; }
        public CreatureDefId DefId { get; }

        public CreatureInstance(CreatureDefId defId, string instanceId = null)
        {
            DefId = defId;
            InstanceId = string.IsNullOrEmpty(instanceId) ? Guid.NewGuid().ToString("N") : instanceId;
        }
    }

    public sealed class PartyState
    {
        public const int DefaultMaxSize = 3;
        public int MaxSize { get; }
        private readonly List<CreatureInstance> _members = new List<CreatureInstance>();

        public PartyState(int maxSize = DefaultMaxSize)
        {
            MaxSize = maxSize > 0 ? maxSize : DefaultMaxSize;
        }

        public IReadOnlyList<CreatureInstance> Members => _members;

        public bool TryAdd(CreatureInstance instance)
        {
            if (instance == null) return false;
            if (_members.Count >= MaxSize) return false;
            _members.Add(instance);
            return true;
        }

        public void Clear() => _members.Clear();

        public void ReplaceAll(IEnumerable<CreatureInstance> members)
        {
            _members.Clear();
            if (members == null) return;
            foreach (var m in members)
            {
                if (m != null && _members.Count < MaxSize)
                    _members.Add(m);
            }
        }
    }
}
