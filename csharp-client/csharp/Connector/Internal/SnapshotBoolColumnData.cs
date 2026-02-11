/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace Deephaven.Connector.Internal
{
    internal class SnapshotBoolColumnData : AbstractSnapshotColumnData<byte>
    {
        private static readonly byte NullValue = 255;

        public SnapshotBoolColumnData(byte[] data) : base(data)
        {
        }

        public override Type GetFieldType(int index)
        {
            return typeof(bool);
        }

        public override string GetDataTypeName()
        {
            return typeof(bool).FullName;
        }

        public override bool IsDBNull(int index)
        {
            return _data[index] == NullValue;
        }

        public override bool GetBoolean(int index)
        {
            return _data[index] == 1 ? true : false;
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
                return GetBoolean(index);
            }
        }
    }
}
