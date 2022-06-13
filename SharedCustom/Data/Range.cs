/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Core.API.Util;

namespace Deephaven.OpenAPI.Shared.Data
{
    public class Range : IComparable<Range>
    {
        public long First { get; set; }
        public long Last { get; set; }

        public Range()
        {
            First = Last = 0;
        }

        public Range(long first, long last)
        {
            if (first > last)
            {
                throw new ArgumentException(first + " > " + last);
            }
            First = first;
            Last = last;
        }

        public int CompareTo(Range other)
        {
            return First.CompareTo(other.First);
        }

        public Range Overlap(Range range)
        {
            if (range.First > Last + 1 || range.Last < First - 1)
            {
                return null;
            }
            return new Range(Math.Min(First, range.First), Math.Max(Last, range.Last));
        }

        public Range[] Minus(Range range)
        {
            if (range.First > Last || range.Last < First)
            {
                return null;
            }

            if (range.First <= First && range.Last >= Last)
            {
                return new Range[0];
            }

            if (range.First > First && range.Last < Last)
            {
                return new [] { new Range(First, range.First - 1), new Range(range.Last + 1, Last) };
            }

            if (range.First <= First)
            {
                ExceptionUtil.CheckState(range.Last >= First, "removed range expected to not end before existing range");
                return new [] { new Range(range.Last + 1, Last) };
            }
            else
            {
                ExceptionUtil.CheckState(range.Last >= Last, "removed range expected to end by the end of the existing range");
                ExceptionUtil.CheckState(range.First <= Last, "removed range expected to start before existing range");
                return new [] { new Range(First, range.First - 1) };
            }
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            if (obj == null || GetType() != obj.GetType())
                return false;
            var range = (Range) obj;
            if (First != range.First)
                return false;
            return Last == range.Last;
        }

        public override int GetHashCode()
        {
            var result = (int) ((uint)First ^ ((uint)First >> 32));
            return 31 * result + (int) ((uint)Last ^ ((uint)Last >> 32));
        }

        public long Size => (int)(Last - First + 1);

        public override string ToString()
        {
            return $"Range{{first={First}, last={Last}}}";
        }
    }
}
