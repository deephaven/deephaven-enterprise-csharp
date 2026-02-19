/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using Deephaven.OpenAPI.Client.Data;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client.Internal
{
    public class ClientTableSnapshot : ITableSnapshot
    {
        public TableSnapshot.TableSnapshotSnapshotType SnapshotType { get; }
        public long DeltaSequence { get; }
        public IColumnData[] DataColumns { get; }
        public RowRangeSet IncludedRows { get; }
        public long TableSize { get; }

        public ClientTableSnapshot(ColumnDefinition[] columnDefinitions, TableSnapshot snapshot)
        {
            SnapshotType = snapshot.SnapshotType;
            DeltaSequence = snapshot.DeltaSequence;
            DataColumns = ColumnDataFactory.WrapColumnData(columnDefinitions, snapshot.DataColumns);
            IncludedRows = new RowRangeSet(snapshot.IncludedRows);
            TableSize = snapshot.TableSize;
        }
    }
}
