/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using NUnit.Framework;

namespace Deephaven.OpenAPI.Shared.Data
{
    [TestFixture]
    public class RangeTest
    {
        [Test]
        public void TestCantMerge()
        {
            Range rangeA = new Range(0, 1);
            Range rangeB = new Range(3, 4);
            Assert.IsNull(rangeA.Overlap(rangeB));
            Assert.IsNull(rangeB.Overlap(rangeA));
        }

        [Test]
        public void TestOverlapMerge()
        {
            Range rangeA = new Range(0, 2);
            Range rangeB = new Range(2, 4);
            Range rangeC = new Range(3, 5);
            Range rangeD = new Range(4, 4);

            // share one item
            Assert.AreEqual(new Range(0, 4), rangeA.Overlap(rangeB));
            Assert.AreEqual(new Range(0, 4), rangeB.Overlap(rangeA));
            // share more than one item
            Assert.AreEqual(new Range(2, 5), rangeB.Overlap(rangeC));
            Assert.AreEqual(new Range(2, 5), rangeC.Overlap(rangeB));
            // share one item, one value is only that item
            Assert.AreEqual(new Range(2, 4), rangeB.Overlap(rangeD));
            Assert.AreEqual(new Range(2, 4), rangeD.Overlap(rangeB));
            // share one item, one range entirely within the other
            Assert.AreEqual(new Range(3, 5), rangeC.Overlap(rangeD));
            Assert.AreEqual(new Range(3, 5), rangeD.Overlap(rangeC));
        }

        [Test]
        public void TestAdjacentMerge()
        {
            Range rangeA = new Range(0, 2);
            Range rangeB = new Range(3, 5);

            Assert.AreEqual(new Range(0, 5), rangeA.Overlap(rangeB));
            Assert.AreEqual(new Range(0, 5), rangeB.Overlap(rangeA));
        }
    }
}
