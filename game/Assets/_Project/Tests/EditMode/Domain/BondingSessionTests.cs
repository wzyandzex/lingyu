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
            s.ApplyIntent(BondingIntent.HoldStill);
            s.ApplyIntent(BondingIntent.Observe);
            Assert.That(s.State, Is.EqualTo(BondingState.Approaching));

            for (var i = 0; i < 30; i++)
                s.Tick(0.1f, playerSpeed: 1.5f, distanceToTarget: 3f, playerStill: false);
            Assert.That(s.State, Is.EqualTo(BondingState.Testing));

            var guard = 0;
            while (s.State == BondingState.Testing && guard++ < 400)
            {
                // Stay mostly aligned entire phase
                if (s.Phase == BondingPhase.Walk)
                    s.Tick(0.1f, playerSpeed: 1.5f, distanceToTarget: 2.5f, playerStill: false);
                else
                    s.Tick(0.1f, playerSpeed: 0.1f, distanceToTarget: 2.5f, playerStill: true);
            }

            Assert.That(s.State, Is.EqualTo(BondingState.ResonanceWindow));
            s.ApplyIntent(BondingIntent.ConfirmResonance);
            Assert.That(s.State, Is.EqualTo(BondingState.Success));
        }

        [Test]
        public void Reading_RushNear_FailsTooFast()
        {
            var s = NewSession();
            s.Tick(0.7f, playerSpeed: 5f, distanceToTarget: 2f, playerStill: false);
            Assert.That(s.State, Is.EqualTo(BondingState.Failed));
            Assert.That(s.LastFailCode, Is.EqualTo(BondingFailCode.TooFast));
        }

        [Test]
        public void Approaching_TooCloseMoving_FailsPressure()
        {
            var s = NewSession();
            s.ApplyIntent(BondingIntent.HoldStill);
            s.ApplyIntent(BondingIntent.Observe);
            Assert.That(s.State, Is.EqualTo(BondingState.Approaching));

            s.Tick(0.1f, playerSpeed: 1.5f, distanceToTarget: 0.8f, playerStill: false);
            Assert.That(s.State, Is.EqualTo(BondingState.Failed));
            Assert.That(s.LastFailCode, Is.EqualTo(BondingFailCode.Pressure));
        }

        [Test]
        public void Testing_Sprint_FailsDesync()
        {
            var s = NewSession();
            s.ApplyIntent(BondingIntent.HoldStill);
            s.ApplyIntent(BondingIntent.Observe);
            for (var i = 0; i < 30; i++)
                s.Tick(0.1f, 1.5f, 3f, false);
            Assert.That(s.State, Is.EqualTo(BondingState.Testing));

            s.Tick(0.7f, playerSpeed: 6f, distanceToTarget: 2.5f, playerStill: false);
            Assert.That(s.State, Is.EqualTo(BondingState.Failed));
            Assert.That(s.LastFailCode, Is.EqualTo(BondingFailCode.Desync));
        }

        [Test]
        public void Testing_MissedBeat_DoesNotInstantFail()
        {
            var s = NewSession();
            s.ApplyIntent(BondingIntent.HoldStill);
            s.ApplyIntent(BondingIntent.Observe);
            for (var i = 0; i < 30; i++)
                s.Tick(0.1f, 1.5f, 3f, false);
            Assert.That(s.State, Is.EqualTo(BondingState.Testing));

            // Completely wrong for a whole short burst but under soft speed — should survive phase end
            var phase = s.Phase;
            while (s.Phase == phase && s.State == BondingState.Testing)
            {
                // opposite of walk expectation
                if (phase == BondingPhase.Walk)
                    s.Tick(0.1f, 0.1f, 2.5f, true);
                else
                    s.Tick(0.1f, 1.5f, 2.5f, false);
            }

            Assert.That(s.State, Is.EqualTo(BondingState.Testing));
            Assert.That(s.IsTerminal, Is.False);
        }

        [Test]
        public void Resonance_Timeout_ReturnsToTesting()
        {
            var s = NewSession();
            s.ApplyIntent(BondingIntent.HoldStill);
            s.ApplyIntent(BondingIntent.Observe);
            for (var i = 0; i < 30; i++)
                s.Tick(0.1f, 1.5f, 3f, false);
            var guard = 0;
            while (s.State == BondingState.Testing && guard++ < 400)
            {
                if (s.Phase == BondingPhase.Walk)
                    s.Tick(0.1f, 1.5f, 2.5f, false);
                else
                    s.Tick(0.1f, 0.1f, 2.5f, true);
            }
            Assert.That(s.State, Is.EqualTo(BondingState.ResonanceWindow));
            s.Tick(7f, 0f, 2.5f, true);
            Assert.That(s.State, Is.EqualTo(BondingState.Testing));
            Assert.That(s.LastFailCode, Is.EqualTo(BondingFailCode.WindowMiss));
        }
    }
}
