/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Data;
using Deephaven.OpenAPI.Shared.Data.Columns;

namespace Deephaven.OpenAPI.Client.Data
{
    /// <summary>
    /// An object representing time-of-day column data. Since there is no
    /// appropriate .NET type, the <see cref="DHTime"/> type is used to
    /// represent time-of-day values.
    /// </summary>
    public class TimeColumnData : AbstractColumnData<DHTime, LocalTime, LocalTimeArrayColumnData>
    {
        internal TimeColumnData(LocalTimeArrayColumnData columnData) : base(columnData)
        {
        }

        internal TimeColumnData(int size) : base(size)
        {
        }

        public TimeColumnData(DHTime[] data) : base(ToLocalTimeArray(data))
        {
        }

        public override DHTime GetValue(int row)
        {
            var localTime = ColumnData.Data[row];
            return localTime == null ? null : new DHTime(localTime);
        }

        public override void SetValue(int row, DHTime value)
        {
            ColumnData.Data[row] = value?.GetLocalTime();
        }

        public override DHTime GetDHTime(int row)
        {
            return GetValue(row);
        }

        public override bool IsNull(int row)
        {
            return GetValue(row) == null;
        }

        protected sealed override string InternalGetColumnType()
        {
            return "java.time.LocalTime";
        }

        private static LocalTime[] ToLocalTimeArray(DHTime[] data)
        {
            var result = new LocalTime[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                result[i] = data[i]?.GetLocalTime();
            }
            return result;
        }
    }
}
