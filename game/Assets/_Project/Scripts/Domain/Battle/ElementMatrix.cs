using System;

namespace Aetherion.Domain.Battle
{
    /// <summary>VS3 teaching matrix (locked in VS3-design v1.1).</summary>
    public static class ElementMatrix
    {
        public const string Verdant = "verdant";
        public const string Tidal = "tidal";
        public const string Pyric = "pyric";
        public const string Wane = "wane";

        public static float GetMultiplier(string attackElement, string defendElement)
        {
            var atk = (attackElement ?? "").ToLowerInvariant();
            var def = (defendElement ?? "").ToLowerInvariant();
            if (string.IsNullOrEmpty(atk) || string.IsNullOrEmpty(def))
                return 1f;

            // attack -> defend
            if (atk == Verdant)
            {
                if (def == Tidal) return 0.5f;
                if (def == Pyric) return 0.5f;
                return 1f;
            }
            if (atk == Tidal)
            {
                if (def == Verdant) return 1.5f;
                if (def == Pyric) return 1.5f;
                if (def == Tidal) return 1f;
                return 1f;
            }
            if (atk == Pyric)
            {
                if (def == Verdant) return 1.5f;
                if (def == Tidal) return 0.5f;
                return 1f;
            }
            // wane and others
            return 1f;
        }

        public static Effectiveness Classify(float mult)
        {
            if (mult >= 1.45f) return Effectiveness.Super;
            if (mult <= 0.55f) return Effectiveness.Resist;
            return Effectiveness.Neutral;
        }
    }
}
