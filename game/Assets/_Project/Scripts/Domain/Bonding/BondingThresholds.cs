namespace Aetherion.Domain.Bonding
{
    /// <summary>VS2 tutorial thresholds — fail still possible, but learnable.</summary>
    public static class BondingThresholds
    {
        public const float SoftSpeed = 4.2f;
        public const float RushSeconds = 0.65f;
        public const float NearRadius = 1.05f;
        public const float ApproachComfortMin = 2.0f;

        public const float AttuneOk = 40f;
        public const float GuardOk = 45f;
        public const float FleeGuard = 100f;

        public const float ReadingProgressNeeded = 0.85f;
        public const float ApproachProgressNeeded = 0.75f;
        public const int TestingCorrectPhasesNeeded = 2;

        // Longer windows so players can read HUD and react
        public const float WalkPhaseSeconds = 2.2f;
        public const float HoldPhaseSeconds = 1.8f;
        public const float ResonanceWindowSeconds = 6f;

        public const float StillSpeedMax = 0.85f;
        public const float SlowApproachMax = 2.8f;

        // Fraction of a phase that must be aligned to count as "correct"
        public const float PhaseAlignRatio = 0.45f;
    }
}
