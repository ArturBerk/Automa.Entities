using Automa.Entities.Systems;
using NUnit.Framework;

#pragma warning disable 649

namespace Automa.Entities.Tests
{
    [TestFixture]
    [Category("Context")]
    public class ContextTests
    {
        [Test]
        public void ContextCreate()
        {
            var context = ContextFactory.CreateEntitiesContext();
            Assert.NotNull(context.GetManager<EntityManager>());
            Assert.NotNull(context.GetManager<SystemManager>());
        }
    }
}