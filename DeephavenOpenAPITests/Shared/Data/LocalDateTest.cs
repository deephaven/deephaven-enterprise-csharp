/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using NUnit.Framework;

namespace Deephaven.OpenAPI.Shared.Data
{
    [TestFixture]
    public class LocalDateTest
    {
        [Test]
        public void TestToString()
        {
            Assert.AreEqual("2019-01-01", new LocalDate(2019, 1, 1).ToString());
            Assert.AreEqual("0001-01-01", new LocalDate(0001, 1, 1).ToString());
            Assert.AreEqual("9999-01-01", new LocalDate(9999, 1, 1).ToString());
            Assert.AreEqual("1977-03-05", new LocalDate(1977, 3, 5).ToString());
            Assert.AreEqual("999999999-12-31", new LocalDate(999_999_999, 12, 31).ToString());
            Assert.AreEqual("-999999999-01-01", new LocalDate(-999_999_999, 1, 1).ToString());
        }
    }
}
