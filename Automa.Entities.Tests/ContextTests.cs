using System;
using System.Collections.Generic;
using System.Text;
using Automa.Entities.Tests.Model;
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
            Context context = new Context();
            Assert.NotNull(context.EntityManager);
            Assert.AreEqual(0, context.EntityManager.EntityCount);
        }
    }
}
