/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Numerics;
using NUnit.Framework;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.Java.Math
{
    [TestFixture]
    public class BigDecimalTest
    {
        [Test]
        public void TestFromDecimal()
        {
            Assert.AreEqual(0m, new BigDecimal(0m).ToDecimal());
            Assert.AreEqual(1m, new BigDecimal(1m).ToDecimal());
            Assert.AreEqual(0.1m, new BigDecimal(0.1m).ToDecimal());
            Assert.AreEqual(0.01m, new BigDecimal(0.01m).ToDecimal());
            Assert.AreEqual(-1m, new BigDecimal(-1m).ToDecimal());
            Assert.AreEqual(-0.1m, new BigDecimal(-0.1m).ToDecimal());
            Assert.AreEqual(-0.01m, new BigDecimal(-0.01m).ToDecimal());
            Assert.AreEqual(314m, new BigDecimal(314m).ToDecimal());
            Assert.AreEqual(31.4m, new BigDecimal(31.4m).ToDecimal());
            Assert.AreEqual(3.14m, new BigDecimal(3.14m).ToDecimal());
            Assert.AreEqual(0.000000314m, new BigDecimal(0.000000314m).ToDecimal());
            Assert.AreEqual(1234567.1234567m, new BigDecimal(1234567.1234567m).ToDecimal());
            Assert.AreEqual(-314m, new BigDecimal(-314m).ToDecimal());
            Assert.AreEqual(-31.4m, new BigDecimal(-31.4m).ToDecimal());
            Assert.AreEqual(-3.14m, new BigDecimal(-3.14m).ToDecimal());
            Assert.AreEqual(-0.000000314m, new BigDecimal(-0.000000314m).ToDecimal());
            Assert.AreEqual(-1234567.1234567m, new BigDecimal(-1234567.1234567m).ToDecimal());
            Assert.AreEqual((decimal)long.MaxValue, new BigDecimal(long.MaxValue).ToDecimal());
            Assert.AreEqual((decimal)long.MinValue, new BigDecimal(long.MinValue).ToDecimal());
            Assert.AreEqual(decimal.MaxValue, new BigDecimal(decimal.MaxValue).ToDecimal());
            Assert.AreEqual(decimal.MinValue, new BigDecimal(decimal.MinValue).ToDecimal());
            Assert.AreEqual(decimal.MaxValue - 1, new BigDecimal(decimal.MaxValue - 1).ToDecimal());
            Assert.AreEqual(decimal.MinValue + 1, new BigDecimal(decimal.MinValue + 1).ToDecimal());
        }

        [Test]
        public void TestToString()
        {
            Assert.AreEqual("0", new BigDecimal(BigInteger.Zero, 0).ToString());
            Assert.AreEqual("1", new BigDecimal(BigInteger.One, 0).ToString());
            Assert.AreEqual("0.1", new BigDecimal(new BigInteger(1), 1).ToString());
            Assert.AreEqual("0.01", new BigDecimal(new BigInteger(1), 2).ToString());
            Assert.AreEqual("-1", new BigDecimal(new BigInteger(-1), 0).ToString());
            Assert.AreEqual("-0.1", new BigDecimal(new BigInteger(-1), 1).ToString());
            Assert.AreEqual("-0.01", new BigDecimal(new BigInteger(-1), 2).ToString());
            Assert.AreEqual("314", new BigDecimal(new BigInteger(314), 0).ToString());
            Assert.AreEqual("31.4", new BigDecimal(new BigInteger(314), 1).ToString());
            Assert.AreEqual("3.14", new BigDecimal(new BigInteger(314), 2).ToString());
            Assert.AreEqual("0.000000314", new BigDecimal(new BigInteger(314), 9).ToString());
            Assert.AreEqual("1234567.1234567", new BigDecimal(new BigInteger(12345671234567), 7).ToString());
            Assert.AreEqual("-314", new BigDecimal(new BigInteger(-314), 0).ToString());
            Assert.AreEqual("-31.4", new BigDecimal(new BigInteger(-314), 1).ToString());
            Assert.AreEqual("-3.14", new BigDecimal(new BigInteger(-314), 2).ToString());
            Assert.AreEqual("-0.000000314", new BigDecimal(new BigInteger(-314), 9).ToString());
            Assert.AreEqual("-1234567.1234567", new BigDecimal(new BigInteger(-12345671234567), 7).ToString());
            Assert.AreEqual(long.MaxValue.ToString(), new BigDecimal(new BigInteger(long.MaxValue), 0).ToString());
            Assert.AreEqual(long.MinValue.ToString(), new BigDecimal(new BigInteger(long.MinValue), 0).ToString());
            Assert.AreEqual(decimal.MaxValue.ToString(), new BigDecimal(new BigInteger(decimal.MaxValue), 0).ToString());
            Assert.AreEqual(decimal.MinValue.ToString(), new BigDecimal(new BigInteger(decimal.MinValue), 0).ToString());
            Assert.AreEqual((decimal.MaxValue - 1).ToString(), new BigDecimal(new BigInteger(decimal.MaxValue - 1), 0).ToString());
            Assert.AreEqual((decimal.MinValue + 1).ToString(), new BigDecimal(new BigInteger(decimal.MinValue + 1), 0).ToString());
        }

        [Test]
        public void TestToDecimal()
        {
            Assert.AreEqual(0m, new BigDecimal(BigInteger.Zero, 0).ToDecimal());
            Assert.AreEqual(1m, new BigDecimal(BigInteger.One, 0).ToDecimal());
            Assert.AreEqual(0.1m, new BigDecimal(new BigInteger(1), 1).ToDecimal());
            Assert.AreEqual(0.01m, new BigDecimal(new BigInteger(1), 2).ToDecimal());
            Assert.AreEqual(-1m, new BigDecimal(new BigInteger(-1), 0).ToDecimal());
            Assert.AreEqual(-0.1m, new BigDecimal(new BigInteger(-1), 1).ToDecimal());
            Assert.AreEqual(-0.01m, new BigDecimal(new BigInteger(-1), 2).ToDecimal());
            Assert.AreEqual(314m, new BigDecimal(new BigInteger(314), 0).ToDecimal());
            Assert.AreEqual(31.4m, new BigDecimal(new BigInteger(314), 1).ToDecimal());
            Assert.AreEqual(3.14m, new BigDecimal(new BigInteger(314), 2).ToDecimal());
            Assert.AreEqual(0.000000314m, new BigDecimal(new BigInteger(314), 9).ToDecimal());
            Assert.AreEqual(1234567.1234567m, new BigDecimal(new BigInteger(12345671234567), 7).ToDecimal());
            Assert.AreEqual(-314m, new BigDecimal(new BigInteger(-314), 0).ToDecimal());
            Assert.AreEqual(-31.4m, new BigDecimal(new BigInteger(-314), 1).ToDecimal());
            Assert.AreEqual(-3.14m, new BigDecimal(new BigInteger(-314), 2).ToDecimal());
            Assert.AreEqual(-0.000000314m, new BigDecimal(new BigInteger(-314), 9).ToDecimal());
            Assert.AreEqual(-1234567.1234567m, new BigDecimal(new BigInteger(-12345671234567), 7).ToDecimal());
            Assert.AreEqual((decimal)long.MaxValue, new BigDecimal(new BigInteger(long.MaxValue), 0).ToDecimal());
            Assert.AreEqual((decimal)long.MinValue, new BigDecimal(new BigInteger(long.MinValue), 0).ToDecimal());
            Assert.AreEqual(decimal.MaxValue, new BigDecimal(new BigInteger(decimal.MaxValue), 0).ToDecimal());
            Assert.AreEqual(decimal.MinValue, new BigDecimal(new BigInteger(decimal.MinValue), 0).ToDecimal());
            Assert.AreEqual(decimal.MaxValue-1, new BigDecimal(new BigInteger(decimal.MaxValue-1), 0).ToDecimal());
            Assert.AreEqual(decimal.MinValue+1, new BigDecimal(new BigInteger(decimal.MinValue+1), 0).ToDecimal());
        }

        [Test]
        public void TestOverflow()
        {
            try
            {
                string str = decimal.MaxValue.ToString() + "0";
                BigInteger reallyBigNumber = BigInteger.Parse(str);
                decimal value = new BigDecimal(reallyBigNumber, str.Length).ToDecimal();
                Assert.Fail("No overflow occurred with too-large number!");
            }
            catch (OverflowException)
            {
                // this is what we expect
            }

            try
            {
                string str = decimal.MinValue.ToString() + "0";
                BigInteger reallyBigNumber = BigInteger.Parse(str);
                decimal value = new BigDecimal(reallyBigNumber, str.Length-1).ToDecimal();
                Assert.Fail("No overflow occurred with too-large number!");
            }
            catch (OverflowException)
            {
                // this is what we expect
            }
        }

        [Test]
        public void TestTruncate()
        {
            try
            {
                string str = decimal.MaxValue.ToString() + "0";
                BigInteger reallyBigNumber = BigInteger.Parse(str);
                decimal value = new BigDecimal(reallyBigNumber, str.Length).ToDecimal(false);
                Assert.AreEqual(decimal.MaxValue, value); // we expect it to truncate to max value
            }
            catch (OverflowException)
            {
                Assert.Fail("Overflow occurred when we expected a truncate!");
            }

            try
            {
                string str = decimal.MinValue.ToString() + "0";
                BigInteger reallyBigNumber = BigInteger.Parse(str);
                decimal value = new BigDecimal(reallyBigNumber, str.Length-1).ToDecimal(false);
                Assert.AreEqual(decimal.MinValue, value); // we expect it to truncate to max value
            }
            catch (OverflowException)
            {
                Assert.Fail("Overflow occurred when we expected a truncate!");
            }
        }
    }
}
