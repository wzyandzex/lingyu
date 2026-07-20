using System;

namespace Aetherion.Domain.Common
{
    public readonly struct RegionId : IEquatable<RegionId>
    {
        public string Value { get; }

        public RegionId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("RegionId cannot be empty.", nameof(value));
            Value = value.Trim();
        }

        public bool Equals(RegionId other) =>
            string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is RegionId other && Equals(other);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

        public override string ToString() => Value;

        public static RegionId R01 => new RegionId("R01");
    }
}
