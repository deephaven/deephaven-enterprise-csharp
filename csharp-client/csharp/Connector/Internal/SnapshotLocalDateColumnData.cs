/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.Connector.Internal
{
    internal class SnapshotLocalDateColumnData : AbstractSnapshotColumnData<LocalDate>
    {

        public SnapshotLocalDateColumnData(LocalDate[] data) : base(data)
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

        public override DateTime GetDateTime(int index)
        {
            if(_data[index] == null)
            {
                return DateTime.MinValue;
            }
            LocalDate localDate = _data[index];
            return new DateTime(localDate.Year, localDate.MonthValue,
                localDate.DayOfMonth, 0, 0, 0, DateTimeKind.Utc);
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
