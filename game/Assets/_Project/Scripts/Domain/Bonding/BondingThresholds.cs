namespace Aetherion.Domain.Bonding
{
    /// <summary>VS2 tutorial thresholds — must still allow failure.</summary>
    public static class BondingThresholds
    {
        public const float SoftSpeed = 3.5f;
        public const float RushSeconds = 0.45f;
        public const float NearRadius = 1.2f;
        public const float ApproachComfortMin = 2.0f;

        // Softened for tutorial but still fail-capable
        public const float AttuneOk = 50f;
        public const float GuardOk = 40f;
        public const float FleeGuard = 85f;

        public const float ReadingProgressNeeded = 1f;
        public const float ApproachProgressNeeded = 1f;
        public const int TestingCorrectPhasesNeeded = 3;

        public const float WalkPhaseSeconds = 1.2f;
        public const float HoldPhaseSeconds = 0.9f;
        public const float ResonanceWindowSeconds = 5f;

        public const float StillSpeedMax = 0.6f;
        public const float SlowApproachMax = 2.2f;
    }
}
