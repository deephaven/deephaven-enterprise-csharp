/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Client.Data;

namespace Deephaven.OpenAPI.Client
{
    /// <summary>
    /// An interface to table data provided in response to a one-time request (i.e.  a "snapshot" of the table).
    /// </summary>
    public interface ITableData
    {
        long Rows { get; }
        RowRangeSet IncludedRows { get; }
        IColumnData[] ColumnData { get; }
    }
}
