/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Shared.Data.Columns;

namespace Deephaven.OpenAPI.Client.Data
{
    /// <summary>
    /// An object representing 32-bit signed integer column data.
    /// Note that <see cref="DeephavenConstants.NULL_INT"/> is used as a null indicator,
    /// so this value cannot be represented with this type.
    /// The <see cref="DeephavenConstants.MIN_INT"/> and <see cref="DeephavenConstants.MAX_INT"/> constants
    /// represent the legally representable range.
    /// </summary>
    public class IntColumnData : AbstractColumnData<int, IntArrayColumnData>
    {
        internal IntColumnData(IntArrayColumnData columnData) : base(columnData)
        {
        }
        internal IntColumnData(int size) : base(size)
        {
        }
        public IntColumnData(int[] data) : base(data)
        {
        }

        public override int GetInt32(int row)
        {
            return GetValue(row);
        }

        public override int? GetNullableInt32(int row)
        {
            var value = GetValue(row);
            return value == DeephavenConstants.NULL_INT ? (int?)null : value;
        }

        public override long GetInt64(int row)
        {
            return GetValue(row);
        }

        public override long? GetNullableInt64(int row)
        {
            var value = GetValue(row);
            return value == DeephavenConstants.NULL_INT ? (long?)null : value;
        }

        public override bool IsNull(int row)
        {
            return !GetNullableInt32(row).HasValue;
        }

        protected sealed override string InternalGetColumnType()
        {
            return "int";
        }
    }
}
