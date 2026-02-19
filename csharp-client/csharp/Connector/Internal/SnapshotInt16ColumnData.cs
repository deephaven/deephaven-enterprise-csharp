/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace Deephaven.Connector.Internal
{
    internal class SnapshotInt16ColumnData : AbstractSnapshotColumnData<short>
    {
        private static readonly short NullValue = short.MinValue;

        public SnapshotInt16ColumnData(short[] data) : base(data)
        {
        }

        public override Type GetFieldType(int index)
        {
            return typeof(short);
        }

        public override string GetDataTypeName()
        {
            return typeof(short).FullName;
        }

        public override bool IsDBNull(int index)
        {
            return _data[index] == NullValue;
        }

        public override short GetInt16(int index)
        {
            return _data[index];
        }

        public override int GetInt32(int index)
        {
            return _data[index];
        }

        public override long GetInt64(int index)
        {
            return _data[index];
        }
    }
}
