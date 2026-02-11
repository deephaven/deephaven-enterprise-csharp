/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Deephaven.OpenAPI.Core.API.Util;

namespace Deephaven.OpenAPI.Shared.Data
{
    public class RangeSet
    {
        public Range[] SortedRanges { get; set; }

        public RangeSet()
        {
            SortedRanges = new Range[0];
        }

        public static RangeSet Empty()
        {
            return new RangeSet();
        }

        public static RangeSet OfRange(long first, long last)
        {
            var rangeSet = new RangeSet();
            rangeSet.AddRange(new Range(first, last));
            return rangeSet;
        }

        public static RangeSet OfItems(params long[] items)
        {
            RangeSet rangeSet = new RangeSet();
            for (int i = 0; i < items.Length; i++)
            {
                long item = items[i];
                rangeSet.AddRange(new Range(item, item));
            }
            return rangeSet;
        }

        public static RangeSet FromSortedRanges(Range[] sortedRanges)
        {
            ExceptionUtil.CheckState(OrderedAndNonOverlapping(sortedRanges), "Ranges not ordered and non-overlapping");
            RangeSet rangeSet = new RangeSet();
            rangeSet.SortedRanges = sortedRanges;
            return rangeSet;
        }

        private static bool OrderedAndNonOverlapping(Range[] sortedRanges)
        {
            long lastSeen = -1;
            for (int i = 0; i < sortedRanges.Length; i++)
            {
                if (lastSeen >= sortedRanges[i].First)
                {
                    return false;
                }
                lastSeen = sortedRanges[i].Last;
            }
            return true;
        }

        public long FirstRow => SortedRanges[0].First;

        public long LastRow => SortedRanges[SortedRanges.Length - 1].Last;

        public void AddRange(Range range)
        {
            if (SortedRanges.Length == 0)
            {
                SortedRanges = new [] {range};
                return;
            }

            if (SortedRanges.Length == 1)
            {
                var existing = SortedRanges[0];
                var overlap = range.Overlap(existing);
                if (overlap != null)
                {
                    SortedRanges = new[] {overlap};
                }
                else if (existing.CompareTo(range) < 0)
                {
                    SortedRanges = new[] {existing, range};
                }
                else
                {
                    ExceptionUtil.CheckState(existing.CompareTo(range) > 0, "");
                    SortedRanges = new[] {range, existing};
                }
                return;
            }

            // if more than one other entry, binarySearch to find before and after entry, and test both for overlapping
            var index = Array.BinarySearch(SortedRanges, range);
            if (index >= 0)
            {
                // starting with that item, check to see if each following item is part of the existing range
                // we know that no range before it will need to be considered, since the set should previously
                // have been broken into non-contiguous ranges
                var merged = range;
                var end = SortedRanges.Length - 1;
                for (int i = index; i < SortedRanges.Length; i++)
                {
                    var existing = SortedRanges[i];
                    // there is an item with the same start, either new item falls within it, or should replace it
                    var overlap = existing.Overlap(merged);

                    if (overlap == null)
                    {
                        // index before this one is the last item to be replaced
                        end = i - 1;
                        break;
                    }
                    //grow the region used for replacing
                    merged = overlap;
                }
                // splice out [index, end] items, replacing with the newly grown overlap object (may be the same
                // size, and only replacing one item)
                var newLength = SortedRanges.Length - (end - index);
                var newArray = new Range[newLength];
                if (index > 0)
                {
                    Array.Copy(SortedRanges, 0, newArray, 0, index);
                }
                newArray[index] = merged;
                if (end < SortedRanges.Length - 1)
                {
                    Array.Copy(SortedRanges, end, newArray, index + 1, SortedRanges.Length - end);
                }
                SortedRanges = newArray;
            }
            else
            {
                var proposedIndex = -(index) - 1;
                var merged = range;
                // test the item before the proposed location (if any), try to merge it
                if (proposedIndex > 0)
                {
                    Range before = SortedRanges[proposedIndex - 1];
                    Range overlap = before.Overlap(merged);
                    if (overlap != null)
                    {
                        //replace the range that we are merging, and start the slice here instead
                        merged = overlap;
                        proposedIndex--;
                        //TODO this will make the loop start here, considering this item twice. not ideal, but not a big deal either
                    }
                }
                // "end" represents the last item that needs to be merged in to the newly added item. if no items are to be
                // merged in, then end will be proposedIndex-1, meaning nothing gets merged in, and the array will grow
                // instead of shrinking.
                // if we never find an item we cannot merge with, the end of the replaced range is the last item of the old
                // array, which could result in the new array having as little as only 1 item
                var end = SortedRanges.Length - 1;
                //until we quit finding matches, test subsequent items
                for (var i = proposedIndex; i < SortedRanges.Length; i++)
                {
                    var existing = SortedRanges[i];
                    var overlap = existing.Overlap(merged);
                    if (overlap == null)
                    {
                        // stop at the item before this one
                        end = i - 1;
                        break;
                    }
                    merged = overlap;
                }
                var newLength = SortedRanges.Length - (end - proposedIndex);
                ExceptionUtil.CheckState(newLength > 0 && newLength <= SortedRanges.Length + 1, "");
                var newArray = new Range[newLength];
                if (proposedIndex > 0)
                {
                    Array.Copy(SortedRanges, 0, newArray, 0, proposedIndex);
                }
                newArray[proposedIndex] = merged;
                if (end < SortedRanges.Length - 1)
                {
                    Array.Copy(SortedRanges, end + 1, newArray, proposedIndex + 1, SortedRanges.Length - (end + 1));
                }
                SortedRanges = newArray;
            }
        }

        public void RemoveRange(Range range)
        {
            // if empty, nothing to do
            if (SortedRanges.Length == 0)
            {
                return;
            }

            // search the sorted list of ranges and find where the current range starts. two case here when using
            // binarySearch, either the removed range starts in the same place as an existing range starts, or
            // it starts before an item (and so we check the item before and the item after)
            int index = Array.BinarySearch(SortedRanges, range);
            if (index < 0)
            {
                // adjusted index notes where the item would be if it were added, minus _one more_ to see if
                // it overlaps the item before it. To compute "the position where the new item belongs", we
                // would do (-index - 1), so to examine one item prior to that we'll subtract one more. Then,
                // to confirm that we are inserting in a valid position, take the max of that value and zero.
                index = Math.Max(0, -index - 2);
            }

            int beforeCount = -1;
            int toRemove = 0;
            for (; index < SortedRanges.Length; index++)
            {
                Range toCheck = SortedRanges[index];
                if (toCheck.First > range.Last)
                {
                    break;//done, this is entirely after the range we're removing
                }
                if (toCheck.Last < range.First)
                {
                    continue;//skip, we don't overlap at all yet
                }
                Range[] remaining = toCheck.Minus(range);
                if(remaining == null)
                {
                    throw new InvalidOperationException("Only early ranges are allowed to not match at all");
                }

                if (remaining.Length == 2)
                {
                    // Removed region is entirely within the range we are checking:
                    // Splice in the one extra item and we're done - this entry
                    // both started before and ended after the removed section,
                    // so we don't even "break", we just return
                    ExceptionUtil.CheckState(toCheck.First < range.First, "Expected " + range + " to start after " + toCheck);
                    ExceptionUtil.CheckState(toCheck.Last > range.Last, "Expected " + range + " to end after " + toCheck);
                    ExceptionUtil.CheckState(toRemove == 0 && beforeCount == -1, "Expected that no previous items in the RangeSet had been removed toRemove=" + toRemove + ", beforeCount=" + beforeCount);

                    Range[] replacement = new Range[SortedRanges.Length + 1];
                    if (index > 0)
                    {
                        Array.Copy(SortedRanges, replacement, index);
                    }
                    replacement[index] = remaining[0];
                    replacement[index + 1] = remaining[1];
                    Array.Copy(SortedRanges, index + 1, replacement, index + 2, SortedRanges.Length - (index + 1));

                    SortedRanges = replacement;

                    return;
                }
                if (remaining.Length == 1)
                {
                    // swap shortened item and move on
                    SortedRanges[index] = remaining[0];
                }
                else
                {
                    if (remaining.Length != 0)
                    {
                        throw new InvalidOperationException("Array contains a surprising number of items: " + remaining.Length);
                    }

                    // splice out this item as nothing exists here any more and move on
                    if (toRemove == 0)
                    {
                        beforeCount = index;
                    }
                    toRemove++;
                }

            }
            if (toRemove > 0)
            {
                Range[] replacement = new Range[SortedRanges.Length - toRemove];
                Array.Copy(SortedRanges, 0, replacement, 0, beforeCount);
                Array.Copy(SortedRanges, beforeCount + toRemove, replacement, beforeCount, SortedRanges.Length - beforeCount - toRemove);

                SortedRanges = replacement;
            }
            else
            {
                ExceptionUtil.CheckState(beforeCount == -1, "No items to remove, but beforeCount set?");
            }
        }

        public IEnumerator<Range> RangeIterator()
        {
            return SortedRanges.AsEnumerable<Range>().GetEnumerator();
        }

        // there doesn't seem to be a built-in range function for int64
        private IEnumerable<long> RangeClosed(long first, long last)
        {
            for(var i = first; i <= last; i++)
            {
                yield return i;
            }
        }

        public IEnumerator<long> IndexIterator()
        {
            return SortedRanges.SelectMany(r => RangeClosed(r.First, r.Last)).GetEnumerator();
        }

        public long Size => SortedRanges.Select(r => r.Size).Sum();

        public bool Contains(long value)
        {
            // TODO this should be the simple case, optimize this
            return IncludesAllOf(RangeSet.OfItems(value));
        }

        public bool IncludesAllOf(RangeSet other)
        {
            IEnumerator<Range> seenIterator = RangeIterator();
            IEnumerator<Range> mustMatchIterator = other.RangeIterator();

            bool defaultReturn = true;
            while (mustMatchIterator.MoveNext())
            {
                defaultReturn = false;
                Range match = mustMatchIterator.Current;
                while (seenIterator.MoveNext())
                {
                    defaultReturn = true;
                    Range current = seenIterator.Current;
                    if (match.First < current.First)
                    {
                        // can't match at all, starts too early
                        return false;
                    }
                    if (match.First > current.Last)
                    {
                        // doesn't start until after the current range, so keep moving forward
                        continue;
                    }
                    if (match.Last > current.Last)
                    {
                        // since the match starts within current, if it ends afterward, we know at least one item is missing: current.getLast() + 1
                        return false;
                    }
                    // else, the match is fully contained in current, so move on to the next item
                    break;
                }
            }
            return defaultReturn;
        }

        public override string ToString()
        {
            var concatenated = string.Join(",", SortedRanges.Select(x => x.ToString()).ToArray());
            return "RangeSet{SortedRanges=" + concatenated + "}";
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            if (obj == null || GetType() != obj.GetType())
                return false;
            var rangeSet = (RangeSet) obj;
            return Enumerable.SequenceEqual(SortedRanges, rangeSet.SortedRanges);
        }

        public override int GetHashCode()
        {
            return ArrayUtil.GetHashCode(SortedRanges);
        }
    }
}
