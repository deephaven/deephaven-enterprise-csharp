/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client.Internal
{
    public class TableStateBuilder
    {
        public TableStateTracker TableStateTracker { get; }
        public Action<InitialTableDefinition> SuccessAction { get; }
        public Action<string> FailureAction { get; }

        public TableState TableState => TableStateTracker.TableState;
        public TableHandle TableHandle => TableState.TableHandle;

        public static TableStateBuilder Create(TableStateScope scope, ServerContext context)
        {
            var tcs = new TaskCompletionSource<InitialTableDefinition>();
            Action<InitialTableDefinition> successAction = itd => tcs.SetResult(itd);
            Action<string> failureAction = err => tcs.SetException(new Exception(err));
            var ts = new TableState(context, tcs.Task);
            var tracker = TableStateTracker.Create(scope, ts);
            return new TableStateBuilder(tracker, successAction, failureAction);
        }

        private TableStateBuilder(TableStateTracker tableStateTracker,
            Action<InitialTableDefinition> successAction, Action<string> failureAction)
        {
            TableStateTracker = tableStateTracker;
            SuccessAction = successAction;
            FailureAction = failureAction;
        }
    }

    /// <summary>
    /// Here is an explanation of the various classes interacting in this system.
    ///
    /// === LOWER LEVEL OBJECTS ===
    ///
    /// TableHandle: The underlying protocol object representing a server TableHandle. I have tried to remove
    /// all the logic from this class, so to the extent possible it just holds ids.
    ///
    /// TableState: Owns a TableHandle, tracks its state, and controls its lifetime. Also keeps track of all all
    /// the TableStateTrackers pointing to it (TableStateTracker is defined below). When the last TableStateTracker
    /// unbinds from it, the TableState will post a request to the server to be released. Also provides higher-level
    /// features like events on DeltaUpdates and Snapshot actions. You can think of the TableHandle being the low-level
    /// protocol object, the TableState being the feature-rich C# object, and the TableStateTracker being the
    /// "thing with the finalizer": when you let go of a TableStateTracker, that's when you (personally) stop caring
    /// about the TableHandle. And by the way, just because you (personally) have stopped caring about a TableHandle,
    /// this doesn't mean other people don't still care about it. Put another way, the TableState sticks around until
    /// the last person with a claim on it goes away. Which brings us to:
    ///
    /// TableStateTracker: Basically the pair (TableState, TableStateScope) along with a Dispose method and a finalizer.
    /// TableStateScope is defined below. Basically this pair means: I care about keeping this TableState alive (aka I
    /// have a claim on it), and my claim will last until:
    /// 1. My finalizer runs, *OR*
    /// 2. The Dispose method on the corresponding TableStateScope runs.
    ///
    /// TableStateScope: manages the set of TableStates that refer to it. The idea is that calling Dispose on a
    /// TableStateScope is a way of releasing a given set of TableStateTrackers *deterministically* under control of the
    /// program; meanwhile the TableStateTracker finalizer is a way of releasing them at garbage collection time. In an
    /// ideal world, the TableStateScope disposal would be the only way TableStateTrackers are disposed, but it seems
    /// sometimes client code misses this and a garbage-collection-based backstop is needed.
    ///
    /// Example:
    /// Given two low-level TableHandles t1 and t2, wrapped by higher-level TableStates T1 and T2.
    /// And given TableStateScopes S1, S2, and S3.
    /// And let's imagine that we have four TableStateTrackers
    /// tst1 = (T1, S1)
    /// tst2 = (T1, S2)
    /// tst3 = (T2, S2)
    /// tst4 = (T2, S3)
    ///
    /// If the program called S2.Dispose(), this would dispose trackers tst2 and tst3 and put them in the disposed state.
    /// This will cause tst2 and tst3 to unregister from TableStates T1 and T2. However this would have no effect on the
    /// liveness of the underlying TableHandles because T1 and T2 are still claimed by other TableStates (namely tst1
    /// and tst4 respectively).
    ///
    /// Now let's assume that tst4 became unreachable due to garbage collection, even though scope S3 is still live and
    /// undisposed. (This GC was able to happen because the TableStateScope only holds its TableStateTrackers by weak
    /// reference). The finalizer for tst4 would remove tst4 from scope S3, and also remove tst4 from state T2. Then,
    /// T2, realizing that no other trackers have a claim on it, would send a Release to the server for underlying
    /// low-level TableHandle t2.
    ///
    /// === TEARDOWN LOGIC ===
    ///
    /// When a TableStateScope is disposed (not finalized... so this is always done either by an explicit call to Dispose
    /// or implicitly in a using block), it asks all of the TableStateTrackers associated with it to dispose themselves.
    ///
    /// When a TableStateTracker is disposed *or finalized*, it unbinds itself from the TableState it is tracking.
    ///
    /// When a TableState has been unbound from the last TableStateTracker pointing to it, it sends a Release to the
    /// server.
    ///
    /// === DISCUSSION ===
    ///
    /// This is the strategy that ServerContext.InvokeServerHelper uses to keep handles alive:
    /// 1. It makes a new, private TableStateScope
    /// 2. It adds all the dependant TableStates it is working with to that temporary scope
    /// 3. It adds the result TableState (namely, the TableState that will hold the *result* of the server operation)
    ///    to this temporary scope as well.
    /// 4. It uses GC.KeepAlive to keep the TableStates alive until the callbacks are done.
    /// 5. After the callback is complete (success or error), the temporary TableStateScope is disposed.
    ///
    /// Q: Why does this work? How does this keep TableStates alive for the duration of the callback?
    /// A: 1. The TableStates cannot be finalized, because it is using GC.KeepAlive to keep them alive.
    ///    2. The TableStates cannot be torn down by the caller via some Dispose on some TableStateScope, because thanks
    ///       to having our own private TableStateScope that we created in step 1, they always belong to at least one
    ///       more scope.
    ///
    /// === HIGHER LEVEL OBJECTS ===
    /// A QueryScope is basically a wrapper around a TableStateScope, along with a bunch of higher-level methods.
    /// The QueryScope Dispose() method will just call the TableStateScope Dispose() method. QueryScope has no finalizer.
    ///
    /// A QueryTable is basically a wrapper around a TableState, along with a bunch of higher-level methods.
    /// The QueryTable Dispose() method will just call the TableState Dispose() method. QueryTable has no finalizer.
    /// </summary>
    public class TableState
    {
        private static long _nextFreeId;

        private readonly long _uniqueId;
        internal ServerContext Context { get; }
        public TableHandle TableHandle { get; }
        private readonly Task<Table> _tableTask;

        public event Action<ITableUpdate> OnTableUpdate;
        public event Action<ITableSnapshot> OnTableSnapshot;

        /// <summary>
        /// The trackers that are tracking me.
        /// </summary>
        private readonly HashSet<long> _trackers = new HashSet<long>();

        internal TableState(ServerContext context, Task<InitialTableDefinition> itdTask)
        {
            _uniqueId = Interlocked.Increment(ref _nextFreeId);
            Context = context;
            TableHandle = Context.NewTableHandle();
            _tableTask = MakeTableTask(itdTask);
        }

        private async Task<Table> MakeTableTask(Task<InitialTableDefinition> itdTask)
        {
            var itd = await itdTask;
            return new Table(TableHandle, Context, itd, DispatchTableUpdate, DispatchTableSnapshot);
        }

        /// <summary>
        /// Convenience method
        /// </summary>
        public Table Resolve()
        {
            return ResolveTask().Result;
        }

        public Task<Table> ResolveTask()
        {
            return _tableTask;
        }

        internal void AddTracker(TableStateTracker tracker)
        {
            lock (this)
            {
                _trackers.Add(tracker.UniqueId);
            }
        }

        internal void RemoveTracker(TableStateTracker tracker)
        {
            lock (this)
            {
                _trackers.Remove(tracker.UniqueId);
                if (_trackers.Count != 0)
                {
                    return;
                }
            }

            Context.ReleaseTableHandle(TableHandle);
        }

        private void DispatchTableUpdate(DeltaUpdates deltaUpdates)
        {
            if (!_tableTask.IsCompleted)
            {
                // We shouldn't be getting callbacks if we're not resolved yet, so just don't bother.
                return;
            }

            var tableDef = _tableTask.Result._tableDefinition;
            OnTableUpdate?.Invoke(new TableUpdate(tableDef.Definition.Columns, deltaUpdates));
        }

        private void DispatchTableSnapshot(TableSnapshot snapshot)
        {
            if (!_tableTask.IsCompleted)
            {
                // We shouldn't be getting callbacks if we're not resolved yet, so just don't bother.
                return;
            }

            var tableDef = _tableTask.Result._tableDefinition;
            OnTableSnapshot?.Invoke(new ClientTableSnapshot(tableDef.Definition.Columns, snapshot));
        }

        public override string ToString()
        {
            return $"TableState(id {_uniqueId}, Handle {TableHandle.ClientId})";
        }
    }
}
