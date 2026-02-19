/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Threading.Tasks;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client.Internal
{
    public interface IWorkerClient
    {
        void AddItdWatcher(TableHandle tableHandle, Action<InitialTableDefinition> success,
            Action<string> failure);
        void RemoveItdWatcher(TableHandle tableHandle);
        void AddTableUpdateHandler(TableHandle tableHandle, Action<DeltaUpdates> tableUpdateHandler);
        void AddTableSnapshotHandler(TableHandle tableHandle, Action<TableSnapshot> tableSnapshotHandler);
        void RemoveTableUpdateHandler(TableHandle tableHandle, Action<DeltaUpdates> tableUpdateHandler);
        void RemoveTableSnapshotHandler(TableHandle tableHandle, Action<TableSnapshot> tableSnapshotHandler);
    }
}
