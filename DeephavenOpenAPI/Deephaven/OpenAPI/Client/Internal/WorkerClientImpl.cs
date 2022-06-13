/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using System;
using System.Collections.Generic;
using Deephaven.OpenAPI.Shared.Cmd;
using Deephaven.OpenAPI.Shared.Data;
using Deephaven.OpenAPI.Shared.Metrics;
using Deephaven.OpenAPI.Shared.Worker;

namespace Deephaven.OpenAPI.Client.Internal
{
    public class WorkerClientImpl : WorkerClient, IWorkerClient
    {
        private readonly IWorkerListener _workerListener;

        /// <summary>
        /// Pending tasks for incoming table definitions.
        /// </summary>
        private Dictionary<TableHandle, SuccessFailureAction> _itdWatchers =
            new Dictionary<TableHandle, SuccessFailureAction>();

        /// <summary>
        /// Handlers for table updates
        /// </summary>
        private readonly Dictionary<TableHandle, Action<DeltaUpdates>> _updateHandlers =
            new Dictionary<TableHandle, Action<DeltaUpdates>>();

        /// <summary>
        /// Handlers for table snapshots
        /// </summary>
        private readonly Dictionary<TableHandle, Action<TableSnapshot>> _snapshotHandlers =
            new Dictionary<TableHandle, Action<TableSnapshot>>();

        /// <summary>
        /// I do this in order to finish my callbacks on a separate thread, rather than
        /// blocking or slowing down the websocket code.
        /// </summary>
        private readonly SequentialWorkPool _workPool = new SequentialWorkPool();

        /// <summary>
        /// The worker server from which we will receive messages.
        /// </summary>
        private WorkerServer _workerServer;

        private WeakReference<IWorkerSession> _workerSession;

        public WorkerClientImpl(IWorkerListener workerListener)
        {
            _workerListener = workerListener;
            _workerSession = new WeakReference<IWorkerSession>(null);
        }

        public void SetServer(WorkerServer server)
        {
            _workerServer = server;
        }

        public void SetWorkerSession(IWorkerSession workerSession)
        {
            _workerSession = new WeakReference<IWorkerSession>(workerSession);
        }

        public void AddTableUpdateHandler(TableHandle tableHandle, Action<DeltaUpdates> tableUpdateHandler)
        {
            lock (this)
            {
                // delegate chaining
                _ = _updateHandlers.TryGetValue(tableHandle, out var existing);
                _updateHandlers[tableHandle] = existing + tableUpdateHandler;
            }
        }

        public void AddTableSnapshotHandler(TableHandle tableHandle, Action<TableSnapshot> tableSnapshotHandler)
        {
            lock (this)
            {
                // delegate chaining
                _ = _snapshotHandlers.TryGetValue(tableHandle, out var existing);
                _snapshotHandlers[tableHandle] = existing + tableSnapshotHandler;
            }
        }

        public void RemoveTableUpdateHandler(TableHandle tableHandle, Action<DeltaUpdates> tableUpdateHandler)
        {
            lock (this)
            {
                if (_updateHandlers.TryGetValue(tableHandle, out var existing))
                {
                    ReplaceOrRemove(_updateHandlers, tableHandle, existing - tableUpdateHandler);
                }
            }
        }

        public void RemoveTableSnapshotHandler(TableHandle tableHandle, Action<TableSnapshot> tableSnapshotHandler)
        {
            lock (this)
            {
                // delegate chaining
                if (_snapshotHandlers.TryGetValue(tableHandle, out var existing))
                {
                    ReplaceOrRemove(_snapshotHandlers, tableHandle, existing - tableSnapshotHandler);
                }
            }
        }

        private static void ReplaceOrRemove<K, V>(Dictionary<K, V> dict, K key, V value) where V : class
        {
            if (value == null)
            {
                dict.Remove(key);
            }
            else
            {
                dict[key] = value;
            }
        }

        /// <summary>
        /// Called by the worker when a new table is exported.
        /// </summary>
        /// <param name="tableDef"></param>
        public void ExportedTableCreationMessage(InitialTableDefinition tableDef)
        {
            SuccessFailureAction callback;
            lock (this)
            {
                if (!_itdWatchers.TryGetValue(tableDef.Id, out callback))
                {
                    return;
                }

                _itdWatchers.Remove(tableDef.Id);
            }

            callback.Success(tableDef);
        }

        public void AddItdWatcher(TableHandle tableHandle, Action<InitialTableDefinition> success,
            Action<string> failure)
        {
            lock (this)
            {
                _itdWatchers.Add(tableHandle, new SuccessFailureAction(success, failure));
            }
        }

        public void RemoveItdWatcher(TableHandle tableHandle)
        {
            lock (this)
            {
                _itdWatchers.Remove(tableHandle);
            }
        }

        /// <summary>
        /// Called by the worker when am exported table is updated.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="size"></param>
        public void ExportedTableUpdateMessage(TableHandle table, long size)
        {
        }

        /// <summary>
        /// Called by the worker when an error occurs with respect to an exported table update.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="errorMessage"></param>
        public void ExportedTableUpdateMessageError(TableHandle table, string errorMessage)
        {
        }

        public WorkerServer GetServer()
        {
            return _workerServer;
        }

        /// <summary>
        /// Called when a client handle has been resolved during a batch operation
        /// </summary>
        /// <param name="handle"></param>
        public void HandleResolved(TableHandle handle)
        {
        }

        public void IncrementalUpdates(TableHandle table, DeltaUpdates updates)
        {
            Action<DeltaUpdates> tableUpdateHandler;
            lock (this)
            {
                if (!_updateHandlers.TryGetValue(table, out tableUpdateHandler))
                {
                    return;
                }
            }

            _workPool.Add(() => tableUpdateHandler(updates));
        }

        public void OnClose(ushort code, string reason)
        {
            Dictionary<TableHandle, SuccessFailureAction> temp;
            lock (this)
            {
                temp = _itdWatchers;
                // TODO(kosak): should probably prevent new watchers from being added
                _itdWatchers = new Dictionary<TableHandle, SuccessFailureAction>();
            }

            var message = $"Connection closed: {reason}";
            foreach (var sfa in temp.Values)
            {
                sfa.Failure(message);
            }

            if (_workerSession.TryGetTarget(out var session))
            {
                _workPool.Add(() => _workerListener?.OnClosed(session, code, reason));
            }
        }

        public void OnError(Exception ex)
        {
            if (_workerSession.TryGetTarget(out var session))
            {
                _workPool.Add(() => _workerListener?.OnError(session, ex));
            }
        }

        public void OnOpen()
        {
            _workerServer.SetClient(this);
            if (_workerSession.TryGetTarget(out var session))
            {
                _workPool.Add(() => _workerListener?.OnOpen(session));
            }
        }

        /// <summary>
        /// Should receive this immediately if we call Pong on the server.
        /// </summary>
        public void Ping()
        {
            if (_workerSession.TryGetTarget(out var session))
            {
                _workPool.Add(() =>
                {
                    _workerServer.FeedWatchdog();
                    _workerListener?.OnPing(session);
                });
            }
        }

        public void ReportMetrics(string type, ServerObjectHandle tableHandle, long nanos)
        {
            //_worker.OnReportMetrics?.Invoke(type, tableHandle.GetClientId(), nanos);
        }

        public void ReportMetrics(MetricsLog[] metrics)
        {
            /*foreach (MetricsLog log in metrics)
            {
                _worker.OnReportMetrics?.Invoke(log.Type, log.ClientId.GetC, log.Nano);
            }*/
        }

        /// <summary>
        /// Called when we are subscribed to the worker logs.
        /// </summary>
        /// <param name="log">the log item</param>
        public void SendLog(LogItem log)
        {
            if (_workerSession.TryGetTarget(out var session))
            {
                _workPool.Add(() => _workerListener?.OnLogMessage(session, new LogMessage(log)));
            }
        }

        public void InitialSnapshot(TableHandle table, TableSnapshot snapshot)
        {
            Action<TableSnapshot> tableSnapshotHandler;
            lock (this)
            {
                if (!_snapshotHandlers.TryGetValue(table, out tableSnapshotHandler))
                {
                    return;
                }
            }

            _workPool.Add(() => tableSnapshotHandler(snapshot));
        }

        public void TableMapStringKeyAdded(TableMapHandle handle, string key)
        {
        }

        public void TableMapStringArrayKeyAdded(TableMapHandle handle, string[] key)
        {
        }

        private class SuccessFailureAction
        {
            public Action<InitialTableDefinition> Success;
            public Action<string> Failure;

            public SuccessFailureAction(Action<InitialTableDefinition> success, Action<string> failure)
            {
                Success = success;
                Failure = failure;
            }
        }
    }
}