/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace Deephaven.Connector.Internal
{
    internal class SnapshotDoubleColumnData : AbstractSnapshotColumnData<double>
    {
        private static readonly double NullValue = -double.MaxValue;

        public SnapshotDoubleColumnData(double[] data) : base(data)
        {
        }

        public override Type GetFieldType(int index)
        {
            return typeof(double);
        }

        public override string GetDataTypeName()
        {
            return typeof(double).FullName;
        }

        public override bool IsDBNull(int index)
        {
            return _data[index].Equals(NullValue);
        }

        public override double GetDouble(int index)
        {
            return _data[index];
        }
    }
}
