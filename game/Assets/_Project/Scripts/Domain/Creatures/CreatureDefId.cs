using System;

namespace Aetherion.Domain.Creatures
{
    /// <summary>Stable species id such as C001. Pure domain value object.</summary>
    public readonly struct CreatureDefId : IEquatable<CreatureDefId>
    {
        public string Value { get; }

        public CreatureDefId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("CreatureDefId cannot be empty.", nameof(value));

            Value = value.Trim();
        }

        public bool Equals(CreatureDefId other) =>
            string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object obj) =>
            obj is CreatureDefId other && Equals(other);

        public override int GetHashCode() =>
            StringComparer.Ordinal.GetHashCode(Value);

        public override string ToString() => Value;

        public static bool operator ==(CreatureDefId left, CreatureDefId right) => left.Equals(right);

        public static bool operator !=(CreatureDefId left, CreatureDefId right) => !left.Equals(right);

        public static CreatureDefId Parse(string value) => new CreatureDefId(value);
    }
}
