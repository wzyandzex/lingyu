using System.Collections.Generic;

namespace Aetherion.Domain.Creatures
{
    /// <summary>Definition data loaded from data/creatures. Not saved as authority.</summary>
    public sealed class CreatureDef
    {
        public CreatureDefId Id { get; set; }
        public string NameKey { get; set; } = string.Empty;
        public string AspectPrimary { get; set; } = string.Empty;
        public string AspectSecondary { get; set; }
        public string SizeClass { get; set; } = string.Empty;
        public string DiscoveryTier { get; set; } = string.Empty;
        public IReadOnlyList<string> Regions { get; set; } = System.Array.Empty<string>();
        public string BondingTemplate { get; set; } = string.Empty;
        public string ViewKey { get; set; } = string.Empty;
        public string CodexScienceKey { get; set; } = string.Empty;
        public string CodexPoemKey { get; set; } = string.Empty;
    }
}
