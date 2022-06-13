/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using NUnit.Framework;

namespace Deephaven.OpenAPI.Core.API.Util
{
    [TestFixture]
    public class UtilTest
    {
        [Test]
        public void TestObjectEquals()
        {
            string a = "Hello World!";
            string b = "Hello World!";
            string c = "yoyoyo";

            Assert.IsTrue(Object.Equals(a, b));
            Assert.IsTrue(Object.Equals(a, a));
            Assert.IsFalse(Object.Equals(a, null));
            Assert.IsFalse(Object.Equals(null, b));
            Assert.IsFalse(Object.Equals(a, c));
        }

        [Test]
        public void TestArrayEquals()
        {
            string[] a = new string[] { "A", "B", "C" };
            string[] b = new string[] { "A", "B", "C" };

            Assert.IsTrue(ArrayUtil.ArraysEqual(a, b));
            Assert.IsFalse(ArrayUtil.ArraysEqual(a, null));
            Assert.IsFalse(ArrayUtil.ArraysEqual(null, b));
        }
    }
}
