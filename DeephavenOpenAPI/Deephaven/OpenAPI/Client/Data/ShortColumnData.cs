/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Shared.Data.Columns;

namespace Deephaven.OpenAPI.Client.Data
{
    /// <summary>
    /// An object representing 16-bit signed integer column data.
    /// Note that <see cref="DeephavenConstants.NULL_SHORT"/> is used as a null indicator,
    /// so this value cannot be represented with this type.
    /// The <see cref="DeephavenConstants.MIN_SHORT"/> and <see cref="DeephavenConstants.MAX_SHORT"/> constants
    /// represent the legally representable range.
    /// </summary>
    public class ShortColumnData : AbstractColumnData<short, ShortArrayColumnData>
    {
        internal ShortColumnData(ShortArrayColumnData columnData) : base(columnData)
        {
        }

        internal ShortColumnData(int size) : base(size)
        {
        }

        public ShortColumnData(short[] data) : base(data)
        {
        }

        public override short GetInt16(int row)
        {
            return GetValue(row);
        }

        public override short? GetNullableInt16(int row)
        {
            var value = GetValue(row);
            return value == DeephavenConstants.NULL_SHORT ? (short?)null : value;
        }

        public override bool IsNull(int row)
        {
            return !GetNullableInt16(row).HasValue;
        }

        protected sealed override string InternalGetColumnType()
        {
            return "short";
        }
    }
}
