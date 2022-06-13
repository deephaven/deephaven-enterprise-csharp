/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace Deephaven.Connector.Internal
{
    internal class SnapshotInt32ColumnData : AbstractSnapshotColumnData<int>
    {
        private static readonly int NullValue = int.MinValue;

        public SnapshotInt32ColumnData(int[] data) : base(data)
        {
        }

        public override Type GetFieldType(int index)
        {
            return typeof(int);
        }

        public override string GetDataTypeName()
        {
            return typeof(int).FullName;
        }

        public override bool IsDBNull(int index)
        {
            return _data[index] == NullValue;
        }

        public override int GetInt32(int index)
        {
            return _data[index];
        }

        public override long GetInt64(int index)
        {
            return (long)_data[index];
        }
    }
}
