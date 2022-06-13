/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Numerics;
using Deephaven.OpenAPI.Shared.Data.Columns;

namespace Deephaven.OpenAPI.Client.Data
{
    /// <summary>
    /// An object representing integer column data of unlimited precision.
    /// </summary>
    public class BigIntegerColumnData : AbstractColumnData<BigInteger?, BigIntegerArrayColumnData>
    {
        internal BigIntegerColumnData(BigIntegerArrayColumnData columnData) : base(columnData)
        {
        }

        internal BigIntegerColumnData(int size) : base(size)
        {
        }

        public BigIntegerColumnData(BigInteger?[] data) : base(data)
        {
        }

        public sealed override BigInteger? GetBigInteger(int row)
        {
            return GetValue(row);
        }

        public sealed override bool IsNull(int row)
        {
            return !ColumnData.Data[row].HasValue;
        }

        protected sealed override string InternalGetColumnType()
        {
            return "java.math.BigInteger";
        }
    }
}
