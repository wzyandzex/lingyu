using Aetherion.Domain.Creatures;
using NUnit.Framework;

namespace Aetherion.Tests.Domain
{
    public sealed class CreatureDefIdTests
    {
        [Test]
        public void Equals_SameValue_IsTrue()
        {
            var a = CreatureDefId.Parse("C001");
            var b = new CreatureDefId("C001");
            Assert.That(a == b, Is.True);
            Assert.That(a.Equals(b), Is.True);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        public void Equals_DifferentValue_IsFalse()
        {
            var a = CreatureDefId.Parse("C001");
            var b = CreatureDefId.Parse("C000");
            Assert.That(a != b, Is.True);
        }

        [Test]
        public void Parse_Whitespace_IsTrimmed()
        {
            var id = new CreatureDefId("  C001  ");
            Assert.That(id.Value, Is.EqualTo("C001"));
        }

        [Test]
        public void Parse_Empty_Throws()
        {
            Assert.That(() => new CreatureDefId(""), Throws.ArgumentException);
            Assert.That(() => new CreatureDefId("   "), Throws.ArgumentException);
        }
    }
}
