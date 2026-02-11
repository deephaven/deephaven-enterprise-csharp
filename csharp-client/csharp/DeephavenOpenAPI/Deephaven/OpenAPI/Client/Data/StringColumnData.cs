/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Data.Columns;

namespace Deephaven.OpenAPI.Client.Data
{
    /// <summary>
    /// An object representing string column data.
    /// </summary>
    public class StringColumnData : AbstractColumnData<string, StringArrayColumnData>
    {
        internal StringColumnData(StringArrayColumnData columnData) : base(columnData)
        {
        }

        internal StringColumnData(int size) : base(size)
        {
        }

        public StringColumnData(string[] data) : base(data)
        {
        }

        public override bool IsNull(int row)
        {
            return GetValue(row) == null;
        }


        protected sealed override string InternalGetColumnType()
        {
            return "java.lang.String";
        }
    }
}
