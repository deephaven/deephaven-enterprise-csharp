/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.Connector.Internal
{
    internal class SnapshotLocalTimeColumnData : AbstractSnapshotColumnData<LocalTime>
    {
        public SnapshotLocalTimeColumnData(LocalTime[] data) : base(data)
        {
        }

        public override Type GetFieldType(int index)
        {
            return typeof(DateTime);
        }

        public override string GetDataTypeName()
        {
            return typeof(DateTime).FullName;
        }

        public override bool IsDBNull(int index)
        {
            return _data[index] == null;
        }

        private static readonly DateTime unixYear0 = new DateTime(1970, 1, 1);

        public override DateTime GetDateTime(int index)
        {
            if (_data[index] == null)
            {
                return DateTime.MinValue;
            }
            LocalTime localTime = _data[index];

            // we represent time of day as start of the unix epoch plus the time span
            // we add millis seperately as a double to get some addition resolution beyond even milliseconds.
            // (TimeSpan and DateTime ticks are 100-nano units)
            return unixYear0.Add(new TimeSpan(localTime.Hour, localTime.Minute, localTime.Second))
                .AddMilliseconds(localTime.Nano / 1_000_000);
        }

        public override string GetString(int index)
        {
            if (_data[index] == null)
                return null;
            return _data[index].ToString();
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
                return GetDateTime(index);
            }
        }
    }
}
