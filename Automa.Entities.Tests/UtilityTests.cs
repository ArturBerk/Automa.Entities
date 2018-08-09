using System;
using System.Collections.Generic;
using System.Text;
using Automa.Common;
using Automa.Entities.Internal;
using NUnit.Framework;

namespace Automa.Entities.Tests
{
    [TestFixture]
    [Category("Utilites")]
    public class UtilityTests
    {

        [Test]
        public void FastListSetTest()
        {
            ArrayList<int> list = new ArrayList<int>(4) { 1, 2, 3 };
            list.SetAt(3, 4);
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(2, list[1]);
            Assert.AreEqual(3, list[2]);
            Assert.AreEqual(4, list[3]);

            list.SetAt(0, 4);
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual(4, list[0]);
            Assert.AreEqual(2, list[1]);
            Assert.AreEqual(3, list[2]);
            Assert.AreEqual(4, list[3]);
        }
    }
}
