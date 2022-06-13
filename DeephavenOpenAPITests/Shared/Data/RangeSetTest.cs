/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Deephaven.OpenAPI.Shared.Data
{
    public class RangeSetTest
    {
        [Test]
        public void TestEmpty()
        {
            RangeSet empty = RangeSet.Empty();
            Assert.AreEqual(0, empty.Size);
            Assert.IsFalse(empty.RangeIterator().MoveNext());
            Assert.IsFalse(empty.IndexIterator().MoveNext());
        }

        [Test]
        public void TestSingleItem()
        {
            RangeSet item = new RangeSet();
            item.AddRange(new Range(1, 1));

            Assert.AreEqual(1, item.Size);
            Assert.AreEqual(1, AsList(item).Count);
            Assert.AreEqual(new Range(1, 1), AsList(item)[0]);

            var allIndicies = new List<long>();
            using (IEnumerator<long> indexEnumerator = item.IndexIterator())
            {
                while(indexEnumerator.MoveNext())
                {
                    allIndicies.Add(indexEnumerator.Current);
                }
            }
            Assert.AreEqual(1, allIndicies.Count);
            Assert.AreEqual(1L, allIndicies[0]);
        }

        private List<Range> AsList(RangeSet rangeSet)
        {
            var allRanges = new List<Range>();
            using (IEnumerator<Range> rangeEnumerator = rangeSet.RangeIterator())
            {
                while(rangeEnumerator.MoveNext())
                {
                    allRanges.Add(rangeEnumerator.Current);
                }
            }
            return allRanges;
        }

        private RangeSet Of(params Range[] ranges)
        {
            RangeSet rs = new RangeSet();
            foreach (Range range in ranges)
            {
                rs.AddRange(range);
            }
            return rs;
        }

        [Test]
        public void TestOverlappingRanges()
        {
            RangeSet rangeSet = new RangeSet();
            rangeSet.AddRange(new Range(1, 20));
            Assert.AreEqual(20, rangeSet.Size);
            Assert.AreEqual(SingletonList(new Range(1, 20)), AsList(rangeSet));

            //exactly the same
            rangeSet.AddRange(new Range(1, 20));
            Assert.AreEqual(20, rangeSet.Size);
            Assert.AreEqual(SingletonList(new Range(1, 20)), AsList(rangeSet));

            //entirely contained
            rangeSet.AddRange(new Range(6, 15));
            Assert.AreEqual(20, rangeSet.Size);
            Assert.AreEqual(SingletonList(new Range(1, 20)), AsList(rangeSet));

            //overlapping, sharing no boundaries
            rangeSet.AddRange(new Range(18, 25));
            Assert.AreEqual(25, rangeSet.Size);
            Assert.AreEqual(SingletonList(new Range(1, 25)), AsList(rangeSet));

            //overlapping, sharing one boundary
            rangeSet.AddRange(new Range(0, 1));
            Assert.AreEqual(26, rangeSet.Size);
            Assert.AreEqual(SingletonList(new Range(0, 25)), AsList(rangeSet));
        }

        private List<Range[]> Permutations(Range[] ranges)
        {
            var output = new List<Range[]>();
            Permutations(ranges, 0, output);
            return output;
        }

        private void Permutations(Range[] ranges, int startIndex, List<Range[]> output)
        {
            if (startIndex == ranges.Length)
                output.Add(ranges);
            for(var i = startIndex; i < ranges.Length; i++)
            {
                Permutations(Swap(ranges, startIndex, i), startIndex + 1, output);
            }
        }

        private Range[] Swap(Range[] ranges, int i, int j)
        {
            Range[] ranges1 = new Range[ranges.Length];
            System.Array.Copy(ranges, ranges1, ranges.Length);
            ranges1[i] = ranges[j];
            ranges1[j] = ranges[i];
            return ranges1;
        }

        [Test]
        public void TestOverlappingRangesInDifferentOrder()
        {

            //add three items in each possible order to a rangeset, ensure results are always the same
            Range rangeA = new Range(100, 108);
            Range rangeB = new Range(105, 112);
            Range rangeC = new Range(110, 115);
            Range[] ranges = { rangeA, rangeB, rangeC };

            List<Range[]> p = Permutations(ranges);

            Permutations(ranges).ForEach(array => {
                RangeSet rangeSet = new RangeSet();
                array.ToList().ForEach(r => rangeSet.AddRange(r));
                Assert.AreEqual(16, rangeSet.Size);
                Assert.AreEqual(SingletonList(new Range(100, 115)), AsList(rangeSet));
            });

            //same three items, but with another before that will not overlap with them
            Range before = new Range(0, 4);
            ranges = new Range[] { before, rangeA, rangeB, rangeC };
            Permutations(ranges).ForEach(array => {
                RangeSet rangeSet = new RangeSet();
                array.ToList().ForEach(r => rangeSet.AddRange(r));

                Assert.AreEqual(21, rangeSet.Size);
                Assert.AreEqual(new Range[] { new Range(0, 4), new Range(100, 115) }.ToList(), AsList(rangeSet));
            });

            //same three items, but with another following that will not overlap with them
            Range after = new Range(200, 204);
            ranges = new Range[] { after, rangeA, rangeB, rangeC };
            Permutations(ranges).ForEach(array => {
                RangeSet rangeSet = new RangeSet();
                array.ToList().ForEach(r => rangeSet.AddRange(r));

                Assert.AreEqual(21, rangeSet.Size);
                Assert.AreEqual(new Range[] { new Range(100, 115), new Range(200, 204) }.ToList(), AsList(rangeSet));
            });
        }

        private List<Range> SingletonList(Range range)
        {
            List<Range> list = new List<Range>();
            list.Add(range);
            return list;
        }

        [Test]
        public void TestNonOverlappingRanges()
        {
            RangeSet rangeSet = new RangeSet();
            rangeSet.AddRange(new Range(1, 20));

            // touching, without sharing a boundary
            rangeSet.AddRange(new Range(21, 30));
            Assert.AreEqual(SingletonList(new Range(1, 30)), AsList(rangeSet));
            Assert.AreEqual(30, rangeSet.Size);

            // not touching at all
            rangeSet.AddRange(new Range(41, 50));
            Assert.AreEqual(40, rangeSet.Size);
            Range[] ranges = { new Range(1, 30), new Range(41, 50) };
            Assert.AreEqual(ranges.ToList(), AsList(rangeSet));
        }

        [Test]
        public void TestIncludesAllOf()
        {
            RangeSet rangeSet = new RangeSet();
            rangeSet.AddRange(new Range(0, 19));
            rangeSet.AddRange(new Range(50, 54));

            Assert.IsTrue(rangeSet.IncludesAllOf(RangeSet.OfRange(0, 19)));
            Assert.IsTrue(rangeSet.IncludesAllOf(RangeSet.OfRange(50, 54)));

            using (IEnumerator<long> indexEnumerator = rangeSet.IndexIterator())
            {
                while (indexEnumerator.MoveNext())
                {
                    var l = indexEnumerator.Current;
                    Assert.IsTrue(rangeSet.IncludesAllOf(RangeSet.OfRange(l, l)));
                }
            }

            Assert.IsFalse(rangeSet.IncludesAllOf(RangeSet.OfRange(0, 20)));
            Assert.IsFalse(rangeSet.IncludesAllOf(RangeSet.OfRange(10, 20)));
            Assert.IsFalse(rangeSet.IncludesAllOf(RangeSet.OfRange(19, 20)));

            Assert.IsFalse(rangeSet.IncludesAllOf(RangeSet.OfRange(19, 30)));
            Assert.IsFalse(rangeSet.IncludesAllOf(RangeSet.OfRange(20, 30)));
            Assert.IsFalse(rangeSet.IncludesAllOf(RangeSet.OfRange(21, 30)));

            Assert.IsFalse(rangeSet.IncludesAllOf(RangeSet.OfRange(30, 40)));

            Assert.IsFalse(rangeSet.IncludesAllOf(RangeSet.OfRange(40, 49)));
            Assert.IsFalse(rangeSet.IncludesAllOf(RangeSet.OfRange(40, 50)));
            Assert.IsFalse(rangeSet.IncludesAllOf(RangeSet.OfRange(40, 41)));
            Assert.IsFalse(rangeSet.IncludesAllOf(RangeSet.OfRange(40, 54)));

            Assert.IsFalse(rangeSet.IncludesAllOf(RangeSet.OfRange(49, 54)));
            Assert.IsFalse(rangeSet.IncludesAllOf(RangeSet.OfRange(50, 55)));
            Assert.IsFalse(rangeSet.IncludesAllOf(RangeSet.OfRange(50, 60)));

            Assert.IsFalse(rangeSet.IncludesAllOf(RangeSet.OfRange(54, 60)));
        }

        private RangeSet CreateRangeSet()
        {
            return Of(new Range(5, 10),
                    new Range(15, 20),
                    new Range(25, 30));
        }

        [Test]
        public void TestRemove()
        {
            // Remove when nothing is present
            RangeSet rangeSet = RangeSet.Empty();
            rangeSet.RemoveRange(new Range(3, 5));
            Assert.AreEqual(RangeSet.Empty(), rangeSet);
            // Remove until nothing is left
            rangeSet = RangeSet.OfRange(0, 9);
            rangeSet.RemoveRange(new Range(0, 9));
            Assert.AreEqual(RangeSet.Empty(), rangeSet);
            rangeSet = RangeSet.OfRange(1, 8);
            rangeSet.RemoveRange(new Range(0, 9));
            Assert.AreEqual(RangeSet.Empty(), rangeSet);

            // Remove section before/between/after any actual existing element (no effect)
            rangeSet = RangeSet.OfRange(5, 10);
            rangeSet.RemoveRange(new Range(0, 3));
            rangeSet.RemoveRange(new Range(11, 12));
            Assert.AreEqual(RangeSet.OfRange(5, 10), rangeSet);
            rangeSet = RangeSet.OfItems(5, 8, 10);
            rangeSet.RemoveRange(new Range(6, 7));
            rangeSet.RemoveRange(new Range(9, 9));
            Assert.AreEqual(RangeSet.OfItems(5, 8, 10), rangeSet);

            //Remove the very first or very last item from a region
            rangeSet = RangeSet.OfRange(5, 10);
            rangeSet.RemoveRange(new Range(5, 5));
            Assert.AreEqual(RangeSet.OfRange(6, 10), rangeSet);

            rangeSet = RangeSet.OfRange(5, 10);
            rangeSet.RemoveRange(new Range(10, 10));
            Assert.AreEqual(RangeSet.OfRange(5, 9), rangeSet);

            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(3, 6));
            Assert.AreEqual(Of(
                    new Range(7, 10),
                    new Range(15, 20),
                    new Range(25, 30)
            ), rangeSet);
            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(8, 12));
            Assert.AreEqual(Of(
                    new Range(5, 7),
                    new Range(15, 20),
                    new Range(25, 30)
            ), rangeSet);

            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(12, 16));
            Assert.AreEqual(Of(
                    new Range(5, 10),
                    new Range(17, 20),
                    new Range(25, 30)
            ), rangeSet);
            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(18, 22));
            Assert.AreEqual(Of(
                    new Range(5, 10),
                    new Range(15, 17),
                    new Range(25, 30)
            ), rangeSet);

            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(22, 27));
            Assert.AreEqual(Of(
                    new Range(5, 10),
                    new Range(15, 20),
                    new Range(28, 30)
            ), rangeSet);
            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(26, 31));
            Assert.AreEqual(Of(
                    new Range(5, 10),
                    new Range(15, 20),
                    new Range(25, 25)
            ), rangeSet);

            // Remove section entirely within another range, touching start or end or none
            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(5, 7));
            Assert.AreEqual(Of(
                    new Range(8, 10),
                    new Range(15, 20),
                    new Range(25, 30)
            ), rangeSet);
            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(7, 10));
            Assert.AreEqual(Of(
                    new Range(5, 6),
                    new Range(15, 20),
                    new Range(25, 30)
            ), rangeSet);
            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(6, 8));
            Assert.AreEqual(Of(
                    new Range(5, 5),
                    new Range(9, 10),
                    new Range(15, 20),
                    new Range(25, 30)
            ), rangeSet);

            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(15, 17));
            Assert.AreEqual(Of(
                    new Range(5, 10),
                    new Range(18, 20),
                    new Range(25, 30)
            ), rangeSet);
            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(17, 20));
            Assert.AreEqual(Of(
                    new Range(5, 10),
                    new Range(15, 16),
                    new Range(25, 30)
            ), rangeSet);
            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(16, 18));
            Assert.AreEqual(Of(
                    new Range(5, 10),
                    new Range(15, 15),
                    new Range(19, 20),
                    new Range(25, 30)
            ), rangeSet);

            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(25, 27));
            Assert.AreEqual(Of(
                    new Range(5, 10),
                    new Range(15, 20),
                    new Range(28, 30)
            ), rangeSet);
            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(27, 30));
            Assert.AreEqual(Of(
                    new Range(5, 10),
                    new Range(15, 20),
                    new Range(25, 26)
            ), rangeSet);
            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(26, 28));
            Assert.AreEqual(Of(
                    new Range(5, 10),
                    new Range(15, 20),
                    new Range(25, 25),
                    new Range(29, 30)
            ), rangeSet);


            // Remove section overlapping 2+ sections
            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(5, 20));
            Assert.AreEqual(RangeSet.OfRange(25, 30), rangeSet);

            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(15, 30));
            Assert.AreEqual(RangeSet.OfRange(5, 10), rangeSet);

            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(5, 30));
            Assert.AreEqual(RangeSet.Empty(), rangeSet);

            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(4, 16));
            Assert.AreEqual(Of(
                    new Range(17, 20),
                    new Range(25, 30)
            ), rangeSet);

            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(6, 21));
            Assert.AreEqual(Of(
                    new Range(5, 5),
                    new Range(25, 30)
            ), rangeSet);

            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(9, 26));
            Assert.AreEqual(Of(
                    new Range(5, 8),
                    new Range(27, 30)
            ), rangeSet);

            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(11, 31));
            Assert.AreEqual(Of(
                    new Range(5, 10)
            ), rangeSet);

            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(4, 31));
            Assert.AreEqual(RangeSet.Empty(), rangeSet);

            // Remove exact section
            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(5, 10));
            Assert.AreEqual(Of(
                    new Range(15, 20),
                    new Range(25, 30)
            ), rangeSet);
            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(15, 20));
            Assert.AreEqual(Of(
                    new Range(5, 10),
                    new Range(25, 30)
            ), rangeSet);
            rangeSet = CreateRangeSet();
            rangeSet.RemoveRange(new Range(25, 30));
            Assert.AreEqual(Of(
                    new Range(5, 10),
                    new Range(15, 20)
            ), rangeSet);
        }
    }
}
