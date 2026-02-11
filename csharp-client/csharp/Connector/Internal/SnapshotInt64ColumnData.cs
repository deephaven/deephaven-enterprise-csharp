/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace Deephaven.Connector.Internal
{
    internal class SnapshotInt64ColumnData : AbstractSnapshotColumnData<long>
    {
        private static readonly long NullValue = long.MinValue;

        public SnapshotInt64ColumnData(long[] data) : base(data)
        {
        }

        public override Type GetFieldType(int index)
        {
            return typeof(long);
        }

        public override string GetDataTypeName() => typeof(long).FullName;

        public override bool IsDBNull(int index)
        {
            return _data[index] == NullValue;
        }

        public override long GetInt64(int index)
        {
            return _data[index];
        }
    }
}
