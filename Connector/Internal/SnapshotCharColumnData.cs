/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace Deephaven.Connector.Internal
{
    internal class SnapshotCharColumnData : AbstractSnapshotColumnData<char>
    {
        private static readonly char NullValue = (char)(char.MaxValue-1);

        public SnapshotCharColumnData(char[] data) : base(data)
        {
        }

        public override Type GetFieldType(int index)
        {
            return typeof(char);
        }

        public override string GetDataTypeName()
        {
            return typeof(char).FullName;
        }

        public override bool IsDBNull(int index)
        {
            return _data[index] == NullValue;
        }

        public override char GetChar(int index)
        {
            return _data[index];
        }

        public override string GetString(int index)
        {
            return new string(new char[] { _data[index] });
        }
    }
}
