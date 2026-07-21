using System.Collections.Generic;
using Aetherion.Domain.Creatures;

namespace Aetherion.Domain.Creatures
{
    public sealed class CreatureDef
    {
        public CreatureDefId Id { get; set; }
        public string NameKey { get; set; } = string.Empty;
        public string AspectPrimary { get; set; } = string.Empty;
        public string AspectSecondary { get; set; }
        public string SizeClass { get; set; } = string.Empty;
        public string DiscoveryTier { get; set; } = string.Empty;
        public IReadOnlyList<string> Regions { get; set; } = System.Array.Empty<string>();
        public IReadOnlyList<string> SkillIds { get; set; } = System.Array.Empty<string>();
        public string BondingTemplate { get; set; } = string.Empty;
        public string ViewKey { get; set; } = string.Empty;
        public string CodexScienceKey { get; set; } = string.Empty;
        public string CodexPoemKey { get; set; } = string.Empty;
        public int BaseHp { get; set; } = 40;
        public int BaseAtk { get; set; } = 10;
        public int BaseDef { get; set; } = 10;
    }
}
