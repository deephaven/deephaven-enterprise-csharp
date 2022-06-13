/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace Deephaven.Connector.Internal
{
    internal class SnapshotByteColumnData : AbstractSnapshotColumnData<sbyte>
    {
        private static readonly sbyte NullValue = sbyte.MinValue;

        public SnapshotByteColumnData(sbyte[] data) : base(data)
        {
        }

        public override Type GetFieldType(int index)
        {
            return typeof(byte);    // DbDataReader is designed for unsigned bytes
        }

        public override string GetDataTypeName()
        {
            return typeof(byte).FullName;
        }

        public override bool IsDBNull(int index)
        {
            return _data[index] == NullValue;
        }

        public override byte GetByte(int index)
        {
            return (byte)_data[index];
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
