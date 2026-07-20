using Aetherion.Domain.Codex;
using Aetherion.Domain.Creatures;
using NUnit.Framework;

namespace Aetherion.Tests.Domain
{
    public sealed class CodexProgressTests
    {
        [Test]
        public void RegisterSighting_FirstTime_UnlocksAppearance()
        {
            var progress = new CodexProgress();
            var id = CreatureDefId.Parse("C002");
            Assert.That(progress.RegisterSighting(id), Is.True);
            Assert.That(progress.IsUnlocked(id, CodexLayer.Appearance), Is.True);
        }

        [Test]
        public void RegisterSighting_SecondTime_IsIdempotent()
        {
            var progress = new CodexProgress();
            var id = CreatureDefId.Parse("C002");
            progress.RegisterSighting(id);
            Assert.That(progress.RegisterSighting(id), Is.False);
        }

        [Test]
        public void IsUnlocked_Unknown_IsFalse()
        {
            var progress = new CodexProgress();
            Assert.That(progress.IsUnlocked(CreatureDefId.Parse("C001"), CodexLayer.Appearance), Is.False);
        }
    }
}
