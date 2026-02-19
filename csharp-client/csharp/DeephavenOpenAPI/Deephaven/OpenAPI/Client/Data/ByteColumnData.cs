/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Shared.Data.Columns;

namespace Deephaven.OpenAPI.Client.Data
{
    /// <summary>
    /// An object representing byte column data. Deephaven byte columns are
    /// always signed, so the type sbyte is used.
    /// Note that <see cref="DeephavenConstants.NULL_BYTE"/> is used as a null indicator,
    /// so this value cannot be represented with this type.
    /// </summary>
    public class ByteColumnData : AbstractColumnData<sbyte, ByteArrayColumnData>
    {
        internal ByteColumnData(ByteArrayColumnData columnData) : base(columnData)
        {
        }
        internal ByteColumnData(int size) : base(size)
        {
        }
        public ByteColumnData(sbyte[] data) : base(data)
        {
        }

        public override sbyte GetByte(int row)
        {
            return GetValue(row);
        }

        public override sbyte? GetNullableByte(int row)
        {
            var value = GetValue(row);
            return value == DeephavenConstants.NULL_BYTE ? (sbyte?)null : value;
        }

        public override bool IsNull(int row)
        {
            return !GetNullableByte(row).HasValue;
        }

        protected sealed override string InternalGetColumnType()
        {
            return "byte";
        }
    }
}
