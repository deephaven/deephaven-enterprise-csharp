/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace Deephaven.Connector.Internal
{
    internal class SnapshotStringColumnData : AbstractSnapshotColumnData<string>
    {
        public SnapshotStringColumnData(string[] data) : base(data)
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

        public override string GetString(int index)
        {
            return _data[index];
        }

        public override long GetChars(int index, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            if(dataOffset > Int32.MaxValue)
            {
                throw new ArgumentException(nameof(dataOffset));
            }
            if (length > Int32.MaxValue)
            {
                throw new ArgumentException(nameof(length));
            }
            int len = Math.Min((int)length, _data[index].Length - (int)dataOffset);
            _data[index].CopyTo((int)dataOffset, buffer, bufferOffset, len);
            return len;
        }
    }
}
