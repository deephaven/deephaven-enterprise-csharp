/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Shared.Data.Columns;

namespace Deephaven.OpenAPI.Client.Data
{
    /// <summary>
    /// An object representing boolean column data.
    /// Note that the <see cref="GetBoolean(int)"/> method cannot return
    /// null values. Use the <see cref="IsNull(int)"/> or
    /// <see cref="GetValue(int)"/> methods to differentiate between null and
    /// false.
    /// </summary>
    public class BooleanColumnData : AbstractColumnData<bool?, sbyte, ByteArrayColumnData>
    {
        internal BooleanColumnData(int size) : base(size)
        {
        }

        internal BooleanColumnData(ByteArrayColumnData columnData) : base(columnData)
        {
        }

        public BooleanColumnData(bool?[] data) : base(ToSByteArray(data))
        {
        }

        public sealed override bool? GetValue(int row)
        {
            return SByteToBool(ColumnData.Data[row]);
        }

        public sealed override void SetValue(int row, bool? value)
        {
            ColumnData.Data[row] = BoolToSByte(value);
        }

        public sealed override bool GetBoolean(int row)
        {
            return GetValue(row) ?? false;
        }

        public sealed override bool? GetNullableBoolean(int row)
        {
            return GetValue(row);
        }

        public sealed override bool IsNull(int row)
        {
            return !GetValue(row).HasValue;
        }

        protected sealed override string InternalGetColumnType()
        {
            return "java.lang.Boolean";
        }

        private static bool? SByteToBool(sbyte b)
        {
            if (b == 0)
                return false;
            if (b == 1)
                return true;
            return null;
        }

        private static sbyte BoolToSByte(bool? b)
        {
            if (b.HasValue)
                return b.Value ? (sbyte) 1 : (sbyte) 0;
            return -1;
        }

        private static sbyte[] ToSByteArray(bool?[] data)
        {
            var result = new sbyte[data.Length];
            for (int i = 0; i < data.Length; i++)
                result[i] = BoolToSByte(data[i]);
            return result;
        }

    }
}
