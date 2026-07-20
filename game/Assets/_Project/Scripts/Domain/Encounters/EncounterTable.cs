using System.Collections.Generic;
using Aetherion.Domain.Creatures;

namespace Aetherion.Domain.Encounters
{
    public sealed class EncounterTable
    {
        public string Id { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public IReadOnlyList<EncounterEntry> Entries { get; set; } = System.Array.Empty<EncounterEntry>();
    }

    public sealed class EncounterEntry
    {
        public CreatureDefId DefId { get; set; }
        public float Weight { get; set; } = 1f;
    }
}
