using Aetherion.Domain.Bonding;
using Aetherion.Domain.Creatures;
using NUnit.Framework;

namespace Aetherion.Tests.Domain
{
    public sealed class BondingSessionTests
    {
        private static BondingSession NewSession() =>
            new BondingSession(CreatureDefId.Parse("C001"));

        [Test]
        public void QuietFollow_SuccessPath_ReachesSuccess()
        {
            var s = NewSession();
            // Reading
            s.ApplyIntent(BondingIntent.HoldStill);
            s.ApplyIntent(BondingIntent.Observe);
            s.ApplyIntent(BondingIntent.HoldStill);
            Assert.That(s.State, Is.EqualTo(BondingState.Approaching));

            // Approaching slowly at safe distance
            for (var i = 0; i < 40; i++)
                s.Tick(0.1f, playerSpeed: 1.5f, distanceToTarget: 3f, playerStill: false);
            Assert.That(s.State, Is.EqualTo(BondingState.Testing));

            // Testing: align with phases
            var guard = 0;
            while (s.State == BondingState.Testing && guard++ < 200)
            {
                if (s.Phase == BondingPhase.Walk)
                    s.Tick(0.15f, playerSpeed: 1.8f, distanceToTarget: 2.5f, playerStill: false);
                else
                    s.Tick(0.15f, playerSpeed: 0.1f, distanceToTarget: 2.5f, playerStill: true);
            }

            Assert.That(s.State, Is.EqualTo(BondingState.ResonanceWindow));
            s.ApplyIntent(BondingIntent.ConfirmResonance);
            Assert.That(s.State, Is.EqualTo(BondingState.Success));
        }

        [Test]
        public void Reading_Rush_FailsTooFast()
        {
            var s = NewSession();
            s.Tick(0.5f, playerSpeed: 5f, distanceToTarget: 4f, playerStill: false);
            Assert.That(s.State, Is.EqualTo(BondingState.Failed));
            Assert.That(s.LastFailCode, Is.EqualTo(BondingFailCode.TooFast));
        }

        [Test]
        public void Approaching_TooCloseMoving_FailsPressure()
        {
            var s = NewSession();
            s.ApplyIntent(BondingIntent.HoldStill);
            s.ApplyIntent(BondingIntent.Observe);
            s.ApplyIntent(BondingIntent.HoldStill);
            Assert.That(s.State, Is.EqualTo(BondingState.Approaching));

            s.Tick(0.1f, playerSpeed: 1.5f, distanceToTarget: 0.8f, playerStill: false);
            Assert.That(s.State, Is.EqualTo(BondingState.Failed));
            Assert.That(s.LastFailCode, Is.EqualTo(BondingFailCode.Pressure));
        }

        [Test]
        public void Testing_WrongRhythm_FailsDesync()
        {
            var s = NewSession();
            s.ApplyIntent(BondingIntent.HoldStill);
            s.ApplyIntent(BondingIntent.Observe);
            s.ApplyIntent(BondingIntent.HoldStill);
            for (var i = 0; i < 40; i++)
                s.Tick(0.1f, 1.5f, 3f, false);
            Assert.That(s.State, Is.EqualTo(BondingState.Testing));

            // Sprint during hold or reverse hard
            s.Tick(0.5f, playerSpeed: 6f, distanceToTarget: 2.5f, playerStill: false);
            Assert.That(s.State, Is.EqualTo(BondingState.Failed));
            Assert.That(s.LastFailCode, Is.EqualTo(BondingFailCode.Desync));
        }

        [Test]
        public void Resonance_Timeout_ReturnsToTesting_WithWindowMiss()
        {
            var s = NewSession();
            s.ApplyIntent(BondingIntent.HoldStill);
            s.ApplyIntent(BondingIntent.Observe);
            s.ApplyIntent(BondingIntent.HoldStill);
            for (var i = 0; i < 40; i++)
                s.Tick(0.1f, 1.5f, 3f, false);
            var guard = 0;
            while (s.State == BondingState.Testing && guard++ < 200)
            {
                if (s.Phase == BondingPhase.Walk)
                    s.Tick(0.15f, 1.8f, 2.5f, false);
                else
                    s.Tick(0.15f, 0.1f, 2.5f, true);
            }
            Assert.That(s.State, Is.EqualTo(BondingState.ResonanceWindow));

            s.Tick(6f, 0f, 2.5f, true);
            Assert.That(s.State, Is.EqualTo(BondingState.Testing));
            Assert.That(s.LastFailCode, Is.EqualTo(BondingFailCode.WindowMiss));
        }

        [Test]
        public void Cancel_Aborts()
        {
            var s = NewSession();
            s.ApplyIntent(BondingIntent.Cancel);
            Assert.That(s.State, Is.EqualTo(BondingState.Aborted));
        }
    }
}
