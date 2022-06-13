/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Core.RPC.Serialization.Java.Math;
using Deephaven.OpenAPI.Shared.Data.Columns;

namespace Deephaven.OpenAPI.Client.Data
{
    /// <summary>
    /// An object representing fixed-point decimal column data. While the
    /// Deephaven fixed point type has unlimited precision, the .NET
    /// <see cref="decimal"/> type does not. If you have the (unusual) case
    /// where you need to represent values outside the range of
    /// the <see cref="decimal"/> type, use the <see cref="GetDHDecimal(int)"/>
    /// and <see cref="SetValue(int, DHDecimal?)"/> methods to avoid overflow
    /// exceptions. Otherwise the <see cref="GetDecimal(int)"/> and
    /// <see cref="SetValue(int, decimal?)"/> are likely the most convenient way
    /// to interact with this type.
    /// </summary>
    public class DecimalColumnData : AbstractColumnData<decimal?, BigDecimal?, BigDecimalArrayColumnData>
    {
        internal DecimalColumnData(BigDecimalArrayColumnData columnData) : base(columnData)
        {
        }
        internal DecimalColumnData(int size) : base(size)
        {
        }
        public DecimalColumnData(decimal?[] data) : base(DecimalArrayToBigDecimalArray(data))
        {
        }
        public DecimalColumnData(DHDecimal?[] data) : base(DHDecimalArrayToBigDecimalArray(data))
        {
        }

        public override decimal? GetValue(int row)
        {
            var rawValue = ColumnData.Data[row];
            return rawValue?.ToDecimal();
        }

        public override void SetValue(int row, decimal? value)
        {
            ColumnData.Data[row] = value.HasValue ? new BigDecimal(value.Value) : (BigDecimal?) null;
        }

        public void SetValue(int row, DHDecimal? value)
        {
            ColumnData.Data[row] = value?.GetBigDecimal();
        }

        /// <summary>
        /// Get the value as a DHDecimal struct. This allows access to data that
        /// would overload the C# decimal type (since the Deephaven decimal type
        /// can hold larger values).
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public override DHDecimal? GetDHDecimal(int row)
        {
            var rawValue = ColumnData.Data[row];
            return rawValue.HasValue ? new DHDecimal(rawValue.Value) : (DHDecimal?) null;
        }

        public override decimal? GetDecimal(int row)
        {
            return GetValue(row);
        }

        public override bool IsNull(int row)
        {
            return !GetValue(row).HasValue;
        }

        private static BigDecimal?[] DecimalArrayToBigDecimalArray(decimal?[] values)
        {
            var a = new BigDecimal?[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                if (values[i].HasValue)
                {
                    a[i] = new BigDecimal(values[i].Value);
                }
            }

            return a;
        }

        private static BigDecimal?[] DHDecimalArrayToBigDecimalArray(DHDecimal?[] values)
        {
            var a = new BigDecimal?[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                if (values[i].HasValue)
                {
                    a[i] = values[i].Value.GetBigDecimal();
                }
            }
            return a;
        }

        protected sealed override string InternalGetColumnType()
        {
            return "java.math.BigDecimal";
        }
    }
}

