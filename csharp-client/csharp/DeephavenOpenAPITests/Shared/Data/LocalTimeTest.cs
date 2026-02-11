/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using NUnit.Framework;

namespace Deephaven.OpenAPI.Shared.Data
{
    [TestFixture]
    public class LocalTimeTest
    {
        [Test]
        public void TestToString()
        {
            Assert.AreEqual("00:00:00.000000000", new LocalTime(0, 0, 0, 0).ToString());
            Assert.AreEqual("23:59:59.999999999", new LocalTime(23, 59, 59, 999_999_999).ToString());
            Assert.AreEqual("01:35:05.000000123", new LocalTime(1, 35, 5, 123).ToString());
        }
    }
}
