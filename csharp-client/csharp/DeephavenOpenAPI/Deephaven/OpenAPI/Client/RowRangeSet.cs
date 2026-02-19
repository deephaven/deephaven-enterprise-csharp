/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client
{
    /// <summary>
    /// An object representing a set of row ranges, each of which has a first
    /// and last row index (inclusive).
    /// </summary>
    public class RowRangeSet
    {
        private readonly RangeSet _rangeSet;

        internal RowRangeSet(RangeSet rangeSet)
        {
            _rangeSet = rangeSet;
        }

        public RowRangeSet(params RowRange[] rowRanges)
        {
            _rangeSet = new RangeSet();
            foreach(RowRange rowRange in rowRanges)
            {
                _rangeSet.AddRange(new Range(rowRange.First, rowRange.Last));
            }
        }

        public long Count => _rangeSet.Size;

        internal RangeSet RangeSet => _rangeSet;

        public override string ToString()
        {
            return _rangeSet.ToString();
        }
    }
}
