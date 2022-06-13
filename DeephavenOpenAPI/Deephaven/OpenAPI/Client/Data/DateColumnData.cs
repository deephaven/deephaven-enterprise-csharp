/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Data;
using Deephaven.OpenAPI.Shared.Data.Columns;

namespace Deephaven.OpenAPI.Client.Data
{
    /// <summary>
    /// An object representing date column data. A Deephaven date is a "local
    /// date" containing only the year, month and day of month (no time of day
    /// or time zone information). Since there is no native .NET type of this
    /// kind, the Open API <see cref="DHDate"/> type is used.
    /// </summary>
    public class DateColumnData : AbstractColumnData<DHDate, LocalDate, LocalDateArrayColumnData>
    {
        internal DateColumnData(LocalDateArrayColumnData columnData) : base(columnData) { }

        internal DateColumnData(int size) : base(size)
        {
        }
        public DateColumnData(DHDate[] data) : base(ToLocalDateArray(data))
        {
        }

        protected sealed override string InternalGetColumnType()
        {
            return "java.time.LocalDate";
        }


        public override DHDate GetValue(int row)
        {
            var localDate = ColumnData.Data[row];
            return localDate == null ? null : new DHDate(localDate);
        }

        public override void SetValue(int row, DHDate value)
        {
            ColumnData.Data[row] = value?.GetLocalDate();
        }

        public override DHDate GetDHDate(int row)
        {
            return GetValue(row);
        }

        public override bool IsNull(int row)
        {
            return GetValue(row) == null;
        }

        private static LocalDate[] ToLocalDateArray(DHDate[] data)
        {
            var result = new LocalDate[data.Length];
            for(var i = 0; i < data.Length; i++)
            {
                result[i] = data[i]?.GetLocalDate();
            }
            return result;
        }
    }
}
