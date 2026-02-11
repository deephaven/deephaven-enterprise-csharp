/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace Deephaven.Connector.Internal
{
    internal class SnapshotStringArrayColumnData : AbstractSnapshotColumnData<string[]>
    {
        public SnapshotStringArrayColumnData(string[][] data) : base(data)
        {
        }

        public override Type GetFieldType(int index)
        {
            return typeof(string);
        }

        public override string GetDataTypeName()
        {
            return typeof(string).FullName;
        }

        public override bool IsDBNull(int index)
        {
            return _data[index] == null;
        }
    }
}
