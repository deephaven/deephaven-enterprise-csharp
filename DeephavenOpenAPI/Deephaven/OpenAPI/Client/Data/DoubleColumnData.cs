/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Shared.Data.Columns;

namespace Deephaven.OpenAPI.Client.Data
{
    /// <summary>
    /// An object representing double precision floating point column data.
    /// Note that <see cref="DeephavenConstants.NULL_DOUBLE"/> is used as a null indicator,
    /// so this value cannot be represented with this type.
    /// </summary>
    public class DoubleColumnData : AbstractColumnData<double, DoubleArrayColumnData>
    {
        internal DoubleColumnData(DoubleArrayColumnData columnData) : base(columnData)
        {
        }

        internal DoubleColumnData(int size) : base(size)
        {
        }

        public DoubleColumnData(double[] data) : base(data)
        {
        }

        public override double GetDouble(int row)
        {
            return GetValue(row);
        }

        public override double? GetNullableDouble(int row)
        {
            var value = GetValue(row);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return value == DeephavenConstants.NULL_DOUBLE ? (double?)null : value;
        }

        public override bool IsNull(int row)
        {
            return !GetNullableDouble(row).HasValue;
        }

        protected sealed override string InternalGetColumnType()
        {
            return "double";
        }
    }
}
