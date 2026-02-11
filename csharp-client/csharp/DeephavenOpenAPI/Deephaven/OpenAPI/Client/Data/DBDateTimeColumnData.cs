/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Diagnostics;
using System.Linq;
using Deephaven.OpenAPI.Client.Internal;
using Deephaven.OpenAPI.Shared.Data.Columns;

namespace Deephaven.OpenAPI.Client.Data
{
    /// <summary>
    /// An object representing date time column data. The Deephaven date-time
    /// has nanonsecond precision, which is not representable by the .NET
    /// <see cref="DateTime"/> type, so while convenient, the <see cref="GetDateTime(int)"/> method
    /// can result in loss of precision. The <see cref="GetInt64(int)"/> method
    /// provides access to full precision value, definend as nanoseconds since
    /// the epoch. The range of this value is implied by the range of
    /// nanoseconds representable by a 64-bit signed integer, with the exception
    /// of <see cref="Int64.MinValue"/>, which is used as a null-indicator.
    /// The <see cref="MinValue"/> and <see cref="MaxValue"/> constants provide
    /// the representable range as <see cref="DateTime"/> values.
    /// </summary>
    public class DBDateTimeColumnData : AbstractColumnData<DBDateTime, long, LongArrayColumnData>
    {
        // public static readonly DateTime MinValue = DateTimeUtil.NanosToDateTime(long.MinValue + 1).Value;
        // public static readonly DateTime MaxValue = DateTimeUtil.NanosToDateTime(long.MaxValue).Value;

        public DBDateTimeColumnData(DBDateTime[] data) : base(ToLongArray(data))
        {
        }
        internal DBDateTimeColumnData(int size) : base(size)
        {
        }
        internal DBDateTimeColumnData(LongArrayColumnData columnData) : base(columnData)
        {
        }

        protected sealed override string InternalGetColumnType()
        {
            return "com.illumon.iris.db.tables.utils.DBDateTime";
        }

        public override DBDateTime GetValue(int row)
        {
            return DBDateTime.FromNanos(ColumnData.Data[row]);
        }

        public override void SetValue(int row, DBDateTime value)
        {
            ColumnData.Data[row] = value.Nanos;
        }

        public override DBDateTime GetDBDateTime(int row) => GetValue(row);

        public override long GetInt64(int row) => ColumnData.Data[row];

        public override bool IsNull(int row) => GetInt64(row) == DeephavenConstants.NULL_LONG;

        private static long[] ToLongArray(DBDateTime[] dateTimes)
        {
            return dateTimes.Select(DBDateTime.ToNanos).ToArray();
        }
    }
}
