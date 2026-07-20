using System;
using Aetherion.Domain.Creatures;

namespace Aetherion.Domain.Bonding
{
    /// <summary>Pure QuietFollow session. Zero random. Presentation feeds speed/distance/still.</summary>
    public sealed class BondingSession
    {
        public CreatureDefId TargetDefId { get; }
        public BondingState State { get; private set; }
        public BondingPhase Phase { get; private set; } = BondingPhase.Hold;
        public BondingFailCode LastFailCode { get; private set; }
        public float Guard { get; private set; }
        public float Attune { get; private set; }
        public float ReadingProgress { get; private set; }
        public float ApproachProgress { get; private set; }
        public int TestingCorrect { get; private set; }
        public float PhaseTimer { get; private set; }
        public float ResonanceTimer { get; private set; }
        public float RushTimer { get; private set; }
        public bool IsTerminal =>
            State == BondingState.Success ||
            State == BondingState.Failed ||
            State == BondingState.Aborted;

        public BondingSession(CreatureDefId targetDefId)
        {
            TargetDefId = targetDefId;
            State = BondingState.Reading;
            Phase = BondingPhase.Hold;
            PhaseTimer = BondingThresholds.HoldPhaseSeconds;
        }

        /// <summary>Player discrete intents (confirm / cancel / observe).</summary>
        public void ApplyIntent(BondingIntent intent)
        {
            if (IsTerminal) return;

            if (intent == BondingIntent.Cancel)
            {
                State = BondingState.Aborted;
                return;
            }

            switch (State)
            {
                case BondingState.Reading:
                    if (intent == BondingIntent.HoldStill || intent == BondingIntent.Observe)
                    {
                        ReadingProgress += 0.4f;
                        Attune += 12f;
                        if (ReadingProgress >= BondingThresholds.ReadingProgressNeeded &&
                            Attune >= BondingThresholds.AttuneOk * 0.4f)
                        {
                            State = BondingState.Approaching;
                        }
                    }
                    break;

                case BondingState.ResonanceWindow:
                    if (intent == BondingIntent.ConfirmResonance)
                    {
                        State = BondingState.Success;
                        LastFailCode = BondingFailCode.None;
                    }
                    break;
            }
        }

        /// <summary>Continuous evaluation from presentation each frame.</summary>
        public void Tick(float dt, float playerSpeed, float distanceToTarget, bool playerStill)
        {
            if (IsTerminal || dt <= 0f) return;

            if (playerSpeed > BondingThresholds.SoftSpeed)
                RushTimer += dt;
            else
                RushTimer = 0f;

            switch (State)
            {
                case BondingState.Reading:
                    TickReading(playerSpeed, distanceToTarget);
                    break;
                case BondingState.Approaching:
                    TickApproaching(dt, playerSpeed, distanceToTarget, playerStill);
                    break;
                case BondingState.Testing:
                    TickTesting(dt, playerSpeed, playerStill);
                    break;
                case BondingState.ResonanceWindow:
                    TickResonance(dt);
                    break;
            }
        }

        private void TickReading(float playerSpeed, float distance)
        {
            if (RushTimer >= BondingThresholds.RushSeconds ||
                (distance < BondingThresholds.NearRadius && playerSpeed > BondingThresholds.SlowApproachMax))
            {
                Fail(BondingFailCode.TooFast);
            }
        }

        private void TickApproaching(float dt, float playerSpeed, float distance, bool playerStill)
        {
            if (distance < BondingThresholds.NearRadius && !playerStill)
            {
                Fail(BondingFailCode.Pressure);
                return;
            }

            if (RushTimer >= BondingThresholds.RushSeconds)
            {
                Fail(BondingFailCode.TooFast);
                return;
            }

            // Gentle approach in comfort band earns progress
            if (playerSpeed > 0.2f && playerSpeed <= BondingThresholds.SlowApproachMax &&
                distance >= BondingThresholds.NearRadius)
            {
                ApproachProgress += dt * 0.55f;
                Attune += dt * 8f;
            }

            if (playerStill && distance < BondingThresholds.ApproachComfortMin + 1.5f &&
                distance >= BondingThresholds.NearRadius)
            {
                ApproachProgress += dt * 0.25f;
            }

            if (ApproachProgress >= BondingThresholds.ApproachProgressNeeded)
            {
                State = BondingState.Testing;
                TestingCorrect = 0;
                Phase = BondingPhase.Walk;
                PhaseTimer = BondingThresholds.WalkPhaseSeconds;
            }
        }

        private void TickTesting(float dt, float playerSpeed, bool playerStill)
        {
            var aligned = Phase == BondingPhase.Walk
                ? playerSpeed > BondingThresholds.StillSpeedMax
                : playerStill || playerSpeed <= BondingThresholds.StillSpeedMax;

            if (!aligned && playerSpeed > BondingThresholds.SoftSpeed)
            {
                Fail(BondingFailCode.Desync);
                return;
            }

            // Mild misalignment doesn't instantly fail; severe reverse does
            if (!aligned)
            {
                Guard += dt * 15f;
                if (Guard >= BondingThresholds.FleeGuard)
                {
                    Fail(BondingFailCode.Desync);
                    return;
                }
            }
            else
            {
                Attune += dt * 6f;
            }

            PhaseTimer -= dt;
            if (PhaseTimer > 0f) return;

            if (aligned)
            {
                TestingCorrect++;
                if (TestingCorrect >= BondingThresholds.TestingCorrectPhasesNeeded &&
                    Attune >= BondingThresholds.AttuneOk)
                {
                    State = BondingState.ResonanceWindow;
                    ResonanceTimer = BondingThresholds.ResonanceWindowSeconds;
                    LastFailCode = BondingFailCode.None;
                    return;
                }
            }
            else
            {
                // End of phase misaligned — desync fail
                Fail(BondingFailCode.Desync);
                return;
            }

            // Advance metronome
            if (Phase == BondingPhase.Walk)
            {
                Phase = BondingPhase.Hold;
                PhaseTimer = BondingThresholds.HoldPhaseSeconds;
            }
            else
            {
                Phase = BondingPhase.Walk;
                PhaseTimer = BondingThresholds.WalkPhaseSeconds;
            }
        }

        private void TickResonance(float dt)
        {
            ResonanceTimer -= dt;
            if (ResonanceTimer <= 0f)
            {
                // Soft miss: return to testing, not full fail terminal
                LastFailCode = BondingFailCode.WindowMiss;
                State = BondingState.Testing;
                TestingCorrect = Math.Max(0, TestingCorrect - 1);
                Phase = BondingPhase.Hold;
                PhaseTimer = BondingThresholds.HoldPhaseSeconds;
            }
        }

        private void Fail(BondingFailCode code)
        {
            LastFailCode = code;
            State = BondingState.Failed;
            Guard = Math.Min(100f, Guard + 20f);
        }
    }
}
