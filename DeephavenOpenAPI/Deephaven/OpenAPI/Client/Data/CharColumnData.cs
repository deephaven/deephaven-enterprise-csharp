/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Shared.Data.Columns;

namespace Deephaven.OpenAPI.Client.Data
{
    /// <summary>
    /// An object representing character column data.
    /// Note that (<see cref="DeephavenConstants.NULL_CHAR"/>) is used as a null indicator,
    /// so this value cannot be represented with this type.
    /// </summary>
    public class CharColumnData : AbstractColumnData<char, CharArrayColumnData>
    {
        internal CharColumnData(CharArrayColumnData columnData) : base(columnData)
        {
        }
        internal CharColumnData(int size) : base(size)
        {
        }
        public CharColumnData(char[] data) : base(data)
        {
        }

        public override char GetChar(int row)
        {
            return GetValue(row);
        }

        public override char? GetNullableChar(int row)
        {
            var value = GetValue(row);
            return value == DeephavenConstants.NULL_CHAR ? (char?)null : value;
        }

        public override bool IsNull(int row)
        {
            return !GetNullableChar(row).HasValue;
        }

        protected sealed override string InternalGetColumnType()
        {
            return "char";
        }
    }
}
