/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Shared.Data.Columns;

namespace Deephaven.OpenAPI.Client.Data
{
    /// <summary>
    /// An object representing single precision floating point column data.
    /// Note that <see cref="DeephavenConstants.NULL_FLOAT"/> is used as a null indicator,
    /// so this value cannot be represented with this type.
    /// </summary>
    public class FloatColumnData : AbstractColumnData<float, FloatArrayColumnData>
    {
        internal FloatColumnData(FloatArrayColumnData columnData) : base(columnData)
        {
        }

        internal FloatColumnData(int size) : base(size)
        {
        }

        public FloatColumnData(float[] data) : base(data)
        {
        }

        public override float GetFloat(int row)
        {
            return GetValue(row);
        }

        public override float? GetNullableFloat(int row)
        {
            var value = GetValue(row);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return value == DeephavenConstants.NULL_FLOAT ? (float?)null : value;
        }

        public override double GetDouble(int row)
        {
            return GetValue(row);
        }

        public override double? GetNullableDouble(int row)
        {
            var value = GetValue(row);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return value == DeephavenConstants.NULL_FLOAT ? (double?)null : value;
        }

        public override bool IsNull(int row)
        {
            return !GetNullableFloat(row).HasValue;
        }

        protected sealed override string InternalGetColumnType()
        {
            return "float";
        }
    }
}
