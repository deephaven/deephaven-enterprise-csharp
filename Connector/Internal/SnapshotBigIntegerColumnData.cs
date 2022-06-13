/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Numerics;

namespace Deephaven.Connector.Internal
{
    internal class SnapshotBigIntegerColumnData : AbstractSnapshotColumnData<BigInteger?>
    {
        public SnapshotBigIntegerColumnData(BigInteger?[] data) : base(data)
        {
        }

        public override Type GetFieldType(int index)
        {
            return typeof(decimal);
        }

        public override string GetDataTypeName()
        {
            return typeof(decimal).FullName;
        }

        public override bool IsDBNull(int index)
        {
            return _data[index] == null;
        }

        public override decimal GetDecimal(int index)
        {
            if (_data[index] == null)
                return 0;
            return (decimal)_data[index].Value;
        }

        public override string GetString(int index)
        {
            if (_data[index] == null)
                return null;
            return _data[index].Value.ToString();
        }

        public override object GetValue(int index)
        {
            // the spec doesn't specify what to return for null, but the
            // SQL Server driver does this, so we imitate that.
            if (IsDBNull(index))
            {
                return DBNull.Value;
            }
            else
            {
                return GetDecimal(index);
            }
        }
    }
}
