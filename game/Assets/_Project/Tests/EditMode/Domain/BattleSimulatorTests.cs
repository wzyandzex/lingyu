using Aetherion.Domain.Battle;
using NUnit.Framework;

namespace Aetherion.Tests.Domain
{
    public sealed class BattleSimulatorTests
    {
        private static SkillDef Mist() => new SkillDef
        {
            Id = "sk_mist_veil",
            NameKey = "skill.sk_mist_veil.name",
            Element = ElementMatrix.Tidal,
            Power = 40
        };

        private static SkillDef Vine() => new SkillDef
        {
            Id = "sk_vine_whisper",
            NameKey = "skill.sk_vine_whisper.name",
            Element = ElementMatrix.Verdant,
            Power = 35
        };

        private static SkillDef Ember() => new SkillDef
        {
            Id = "sk_ember_nibble",
            NameKey = "skill.sk_ember_nibble.name",
            Element = ElementMatrix.Pyric,
            Power = 28
        };

        [Test]
        public void Matrix_TidalVsPyric_IsSuper()
        {
            var m = ElementMatrix.GetMultiplier(ElementMatrix.Tidal, ElementMatrix.Pyric);
            Assert.That(m, Is.EqualTo(1.5f));
            Assert.That(ElementMatrix.Classify(m), Is.EqualTo(Effectiveness.Super));
        }

        [Test]
        public void Matrix_VerdantVsPyric_IsResist()
        {
            var m = ElementMatrix.GetMultiplier(ElementMatrix.Verdant, ElementMatrix.Pyric);
            Assert.That(m, Is.EqualTo(0.5f));
            Assert.That(ElementMatrix.Classify(m), Is.EqualTo(Effectiveness.Resist));
        }

        [Test]
        public void Matrix_PyricVsTidal_IsResist()
        {
            var m = ElementMatrix.GetMultiplier(ElementMatrix.Pyric, ElementMatrix.Tidal);
            Assert.That(m, Is.EqualTo(0.5f));
        }

        [Test]
        public void Fog_BoostsTidal()
        {
            var noFog = BattleSimulator.ComputeDamage(40, 10, 10, ElementMatrix.Tidal, ElementMatrix.Pyric, null, false);
            var fog = BattleSimulator.ComputeDamage(40, 10, 10, ElementMatrix.Tidal, ElementMatrix.Pyric, BattleSimulator.WeatherFog, false);
            Assert.That(fog, Is.GreaterThanOrEqualTo(noFog));
        }

        [Test]
        public void Guard_HalvesIncoming()
        {
            var raw = BattleSimulator.ComputeDamage(40, 10, 10, ElementMatrix.Tidal, ElementMatrix.Pyric, null, false);
            var guarded = BattleSimulator.ComputeDamage(40, 10, 10, ElementMatrix.Tidal, ElementMatrix.Pyric, null, true);
            Assert.That(guarded, Is.LessThan(raw));
        }

        [Test]
        public void FullFight_MistSpam_PlayerWins()
        {
            var player = new BattlerState
            {
                Id = BattleSimulator.PlayerId,
                DefId = "C001",
                NameKey = "creature.C001.name",
                PrimaryElement = ElementMatrix.Tidal,
                MaxHp = 42,
                Hp = 42,
                Atk = 10,
                Def = 12,
                SkillIds = new[] { "sk_mist_veil", "sk_vine_whisper" }
            };
            var enemy = new BattlerState
            {
                Id = BattleSimulator.EnemyId,
                DefId = "E001",
                NameKey = "enemy.E001.name",
                PrimaryElement = ElementMatrix.Pyric,
                MaxHp = 40,
                Hp = 40,
                Atk = 11,
                Def = 9,
                SkillIds = new[] { "sk_ember_nibble" }
            };
            var sim = new BattleSimulator(player, enemy, new[] { Mist(), Vine(), Ember() });
            sim.Begin();
            var guard = 0;
            while (sim.Result == BattleResult.Ongoing && guard++ < 30)
            {
                sim.SubmitPlayerAction(new BattleAction
                {
                    Type = BattleActionType.Skill,
                    SkillId = "sk_mist_veil"
                });
            }
            Assert.That(sim.Result, Is.EqualTo(BattleResult.PlayerWin));
            Assert.That(enemy.IsFainted, Is.True);
        }

        [Test]
        public void Flee_EndsImmediately()
        {
            var player = new BattlerState
            {
                Id = BattleSimulator.PlayerId,
                DefId = "C001",
                PrimaryElement = ElementMatrix.Tidal,
                MaxHp = 42,
                Hp = 42,
                Atk = 10,
                Def = 12,
                SkillIds = new[] { "sk_mist_veil" }
            };
            var enemy = new BattlerState
            {
                Id = BattleSimulator.EnemyId,
                DefId = "E001",
                PrimaryElement = ElementMatrix.Pyric,
                MaxHp = 40,
                Hp = 40,
                Atk = 11,
                Def = 9,
                SkillIds = new[] { "sk_ember_nibble" }
            };
            var sim = new BattleSimulator(player, enemy, new[] { Mist(), Ember() });
            sim.Begin();
            sim.SubmitPlayerAction(new BattleAction { Type = BattleActionType.Flee });
            Assert.That(sim.Result, Is.EqualTo(BattleResult.Fled));
        }
    }
}
