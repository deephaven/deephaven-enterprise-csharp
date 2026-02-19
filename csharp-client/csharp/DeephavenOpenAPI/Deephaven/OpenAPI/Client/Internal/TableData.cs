/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Client;
using Deephaven.OpenAPI.Client.Data;
using Deephaven.OpenAPI.Client.Internal;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client.Internal
{
    /// <summary>
    /// An object holding table data in response to a one-time request (i.e.
    /// a "snapshot" of the table).
    /// </summary>
    internal class TableData : ITableData
    {
        public long Rows { get; }
        public IColumnData[] ColumnData { get; }
        public long TableSize { get; }
        public RowRangeSet IncludedRows { get; }

        internal TableData(ColumnDefinition[] columnDefinitions, TableSnapshot tableSnapshot)
        {
            IncludedRows = new RowRangeSet(tableSnapshot.IncludedRows);
            Rows = tableSnapshot.IncludedRows.Size;
            TableSize = tableSnapshot.TableSize;
            ColumnData = new IColumnData[tableSnapshot.DataColumns.Length];
            for (var i = 0; i < tableSnapshot.DataColumns.Length; i++)
            {
                ColumnData[i] = ColumnDataFactory.WrapColumnData(columnDefinitions[i], tableSnapshot.DataColumns[i]);
            }
        }
    }
}
