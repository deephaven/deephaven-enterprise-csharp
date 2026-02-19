/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Shared.Data.Columns;

namespace Deephaven.OpenAPI.Client.Data
{
    /// <summary>
    /// An object representing 64-bit signed integer column data.
    /// Note that <see cref="DeephavenConstants.NULL_LONG"/> is used as a null indicator,
    /// so this value cannot be represented with this type.
    /// The <see cref="DeephavenConstants.MIN_LONG"/> and <see cref="DeephavenConstants.MAX_LONG"/> constants
    /// represent the legally representable range.
    /// </summary>
    ///
    public class LongColumnData : AbstractColumnData<long, LongArrayColumnData>
    {
        internal LongColumnData(LongArrayColumnData columnData) : base(columnData)
        {
        }

        internal LongColumnData(int size) : base(size)
        {
        }

        public LongColumnData(long[] data) : base(data)
        {
        }

        public override long GetInt64(int row)
        {
            return GetValue(row);
        }

        public override long? GetNullableInt64(int row)
        {
            var value = GetValue(row);
            return value == DeephavenConstants.NULL_LONG ? (long?)null : value;
        }

        public override bool IsNull(int row)
        {
            return !GetNullableInt64(row).HasValue;
        }

        protected sealed override string InternalGetColumnType()
        {
            return "long";
        }
    }
}
