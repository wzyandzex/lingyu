using System;
using Aetherion.Domain.Creatures;

namespace Aetherion.Domain.Bonding
{
    /// <summary>
    /// QuietFollow session. Forgiving Testing: score by time-in-phase alignment,
    /// only hard-fail DESYNC on sustained sprint; end-of-phase miss just retries phase.
    /// </summary>
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
        public float PhaseDuration { get; private set; }
        public float PhaseAlignedTime { get; private set; }
        public float ResonanceTimer { get; private set; }
        public float RushTimer { get; private set; }

        public float PhaseRemaining01 =>
            PhaseDuration <= 0.001f ? 0f : Math.Max(0f, PhaseTimer / PhaseDuration);

        public bool IsTerminal =>
            State == BondingState.Success ||
            State == BondingState.Failed ||
            State == BondingState.Aborted;

        public BondingSession(CreatureDefId targetDefId)
        {
            TargetDefId = targetDefId;
            State = BondingState.Reading;
            Phase = BondingPhase.Hold;
            BeginPhase(BondingPhase.Hold);
        }

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
                        ReadingProgress += 0.5f;
                        Attune += 15f;
                        if (ReadingProgress >= BondingThresholds.ReadingProgressNeeded)
                            State = BondingState.Approaching;
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

        public void Tick(float dt, float playerSpeed, float distanceToTarget, bool playerStill)
        {
            if (IsTerminal || dt <= 0f) return;

            if (playerSpeed > BondingThresholds.SoftSpeed)
                RushTimer += dt;
            else
                RushTimer = Math.Max(0f, RushTimer - dt * 0.5f);

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
            // Only fail if really rushing into its face
            if (RushTimer >= BondingThresholds.RushSeconds && distance < 3.5f)
                Fail(BondingFailCode.TooFast);
        }

        private void TickApproaching(float dt, float playerSpeed, float distance, bool playerStill)
        {
            if (distance < BondingThresholds.NearRadius && playerSpeed > BondingThresholds.StillSpeedMax)
            {
                Fail(BondingFailCode.Pressure);
                return;
            }

            if (RushTimer >= BondingThresholds.RushSeconds && distance < 4f)
            {
                Fail(BondingFailCode.TooFast);
                return;
            }

            if (playerSpeed > 0.15f && playerSpeed <= BondingThresholds.SlowApproachMax &&
                distance >= BondingThresholds.NearRadius)
            {
                ApproachProgress += dt * 0.7f;
                Attune += dt * 10f;
            }

            if (playerStill && distance < 4.5f && distance >= BondingThresholds.NearRadius)
                ApproachProgress += dt * 0.35f;

            if (ApproachProgress >= BondingThresholds.ApproachProgressNeeded)
            {
                State = BondingState.Testing;
                TestingCorrect = 0;
                BeginPhase(BondingPhase.Walk);
            }
        }

        private void TickTesting(float dt, float playerSpeed, bool playerStill)
        {
            // Hard DESYNC only on sustained sprint
            if (playerSpeed > BondingThresholds.SoftSpeed)
            {
                RushTimer += dt;
                if (RushTimer >= BondingThresholds.RushSeconds)
                {
                    Fail(BondingFailCode.Desync);
                    return;
                }
            }

            var aligned = Phase == BondingPhase.Walk
                ? playerSpeed > BondingThresholds.StillSpeedMax
                : playerSpeed <= BondingThresholds.StillSpeedMax;

            if (aligned)
                PhaseAlignedTime += dt;

            PhaseTimer -= dt;
            if (PhaseTimer > 0f) return;

            var ratio = PhaseDuration <= 0.001f ? 0f : PhaseAlignedTime / PhaseDuration;
            if (ratio >= BondingThresholds.PhaseAlignRatio)
            {
                TestingCorrect++;
                Attune += 12f;
                if (TestingCorrect >= BondingThresholds.TestingCorrectPhasesNeeded)
                {
                    State = BondingState.ResonanceWindow;
                    ResonanceTimer = BondingThresholds.ResonanceWindowSeconds;
                    LastFailCode = BondingFailCode.None;
                    return;
                }
            }
            // else: miss this beat — do NOT terminal fail; just next phase (forgiving)

            BeginPhase(Phase == BondingPhase.Walk ? BondingPhase.Hold : BondingPhase.Walk);
        }

        private void TickResonance(float dt)
        {
            ResonanceTimer -= dt;
            if (ResonanceTimer <= 0f)
            {
                LastFailCode = BondingFailCode.WindowMiss;
                State = BondingState.Testing;
                TestingCorrect = Math.Max(0, TestingCorrect - 1);
                BeginPhase(BondingPhase.Hold);
            }
        }

        private void BeginPhase(BondingPhase phase)
        {
            Phase = phase;
            PhaseDuration = phase == BondingPhase.Walk
                ? BondingThresholds.WalkPhaseSeconds
                : BondingThresholds.HoldPhaseSeconds;
            PhaseTimer = PhaseDuration;
            PhaseAlignedTime = 0f;
        }

        private void Fail(BondingFailCode code)
        {
            LastFailCode = code;
            State = BondingState.Failed;
            Guard = Math.Min(100f, Guard + 20f);
        }
    }
}
