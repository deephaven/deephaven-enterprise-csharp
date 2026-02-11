/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Data;
using Range = Deephaven.OpenAPI.Shared.Data.Range;

namespace Deephaven.OpenAPI.Client
{
    /// <summary>
    /// An object representing a range of table rows by the first and last row
    /// index (inclusive).
    /// </summary>
    public class RowRange
    {
        private Range _range;

        private RowRange(Range range)
        {
            _range = range;
        }

        public RowRange(long first, long last)
        {
            _range = new Range(first, last);
        }

        public long First { get { return _range.First; } }
        public long Last { get { return _range.Last; } }
    }
}
