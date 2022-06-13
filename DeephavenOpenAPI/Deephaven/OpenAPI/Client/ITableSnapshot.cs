/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using Deephaven.OpenAPI.Client.Data;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client
{
    public interface ITableSnapshot
    {
        TableSnapshot.TableSnapshotSnapshotType SnapshotType { get; }
        long DeltaSequence { get; }
        IColumnData[] DataColumns { get; }
        RowRangeSet IncludedRows { get; }
        long TableSize { get; }
    }
}
