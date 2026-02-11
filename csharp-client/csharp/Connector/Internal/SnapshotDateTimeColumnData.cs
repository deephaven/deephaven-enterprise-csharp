/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace Deephaven.Connector.Internal
{
    internal class SnapshotDateTimeColumnData : AbstractSnapshotColumnData<long>
    {
        private static readonly long NullValue = Int64.MinValue;

        public SnapshotDateTimeColumnData(long[] rawData) : base(rawData)
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
            return _data[index] == NullValue;
        }

        private static readonly DateTime unixYear0 = new DateTime(1970, 1, 1);

        // we assume TickerPerSecond less than or equals to 1 billion
        private const long NanosToTicksDivisor = 1_000_000_000L / TimeSpan.TicksPerSecond;

        private static DateTime TimeFromNanos(long nanos)
        {
            long unixTimeStampInTicks = nanos / NanosToTicksDivisor;
            DateTime dtUnix = new DateTime(unixYear0.Ticks + unixTimeStampInTicks, DateTimeKind.Utc);
            return dtUnix;
        }

        public override DateTime GetDateTime(int index)
        {
            return TimeFromNanos(_data[index]);
        }

        public override long GetInt64(int index)
        {
            return _data[index];
        }

        public override object GetValue(int index)
        {
            // the spec doesn't specify what to return for null, but the
            // SQL Server driver does this, so we imitate that.
            if(IsDBNull(index))
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
