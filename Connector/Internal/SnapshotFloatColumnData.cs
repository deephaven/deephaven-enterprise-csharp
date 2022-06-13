/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace Deephaven.Connector.Internal
{
    internal class SnapshotFloatColumnData : AbstractSnapshotColumnData<float>
    {
        private static readonly float NullValue = -float.MaxValue;

        public SnapshotFloatColumnData(float[] data) : base(data)
        {
        }

        public override Type GetFieldType(int index)
        {
            return typeof(float);
        }

        public override string GetDataTypeName()
        {
            return typeof(float).FullName;
        }

        public override bool IsDBNull(int index)
        {
            return _data[index].Equals(NullValue);
        }

        public override float GetFloat(int index)
        {
            return _data[index];
        }

        public override double GetDouble(int index)
        {
            return _data[index];
        }
    }
}
