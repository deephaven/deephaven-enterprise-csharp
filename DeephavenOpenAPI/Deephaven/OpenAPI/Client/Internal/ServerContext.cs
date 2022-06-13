/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Linq;
using System.Threading.Tasks;
using Deephaven.OpenAPI.Shared.Batch;
using Deephaven.OpenAPI.Shared.Cmd;
using Deephaven.OpenAPI.Shared.Data;
using Deephaven.OpenAPI.Shared.Worker;

namespace Deephaven.OpenAPI.Client.Internal
{
    public class ServerContext : IDisposable
    {
        private readonly ServerWrapper<WorkerServer> _serverWrapper;
        public IWorkerClient WorkerClient { get; }
        private readonly ClientTableIdFactory _clientTableIdFactory;
        private bool _isDisposed = false;

        public ServerContext(WorkerServer workerServer, IWorkerClient workerClient,
            ClientTableIdFactory clientTableIdFactory)
        {
            _serverWrapper = new ServerWrapper<WorkerServer>(workerServer);
            WorkerClient = workerClient;
            _clientTableIdFactory = clientTableIdFactory;
        }

        public void Dispose()
        {
            lock (this)
            {
                if (_isDisposed)
                {
                    return;
                }
                _isDisposed = true;
            }

            var liveHandles = _clientTableIdFactory.GetPoolValues().Where(th => th.ServerIdIsAssigned).ToArray();

            var tcs = new TaskCompletionSource<bool>();

            // Don't call our helper method InvokeServer because it asserts !_isDisposed, which we have just set.
            _serverWrapper.InvokeServer(ws =>
            {
                foreach (var th in liveHandles)
                {
                    ws.Release(th);
                }
            });

            // Post a second job which shuts down the server
            _serverWrapper.InvokeServer(ws => ws.Dispose());
            // Post a third job which sets the TaskCompletionSource.
            _serverWrapper.InvokeServer(ws => tcs.SetResult(true));

            tcs.Task.Wait();
        }

        internal TableHandle NewTableHandle()
        {
            AssertNotDisposed();
            return _clientTableIdFactory.NewHandle();
        }

        internal void ReleaseTableHandle(TableHandle tableHandle)
        {
            if (IsDisposed())
            {
                return;
            }

            _clientTableIdFactory.DeleteHandle(tableHandle);
            if (tableHandle.ServerIdIsAssigned)
            {
                _serverWrapper.InvokeServer(ws => ws.Release(tableHandle));
            }
        }

        internal void InvokeServer(Action<WorkerServer> action)
        {
            _serverWrapper.InvokeServer(action);
        }
        
        internal Task<T> InvokeServerTask<T>(Action<WorkerServer, Action<T>, Action<string>> callback)
        {
            AssertNotDisposed();
            return _serverWrapper.InvokeServerTask(callback);
        }

        private bool IsDisposed()
        {
            lock (this)
            {
                return _isDisposed;
            }
        }

        private void AssertNotDisposed()
        {
            if (IsDisposed())
            {
                throw new Exception("Can't perform request... ServerContext is disposed");
            }
        }

        internal void StartWatchdog(long expiryMillis)
        {
            _serverWrapper.Server.StartWatchdog(expiryMillis);
        }

        // We would like to prevent the 'dependentStates' and the result 'childState' from being either disposed
        // or finalized until our callbacks are done (success or failure). To accomplish this, we do two things:
        // 1. Make our own private TableStateScope, then make new TableStateTrackers that bind these TableStates
        //    to that new TableStateScope. In the event that the caller asynchronously disposes whatever
        //    TableStateScopes these TableStates otherwise belong to, their membership in our own private temporary
        //    TableStateScope would prevent them from being disposed.
        // 2. We use GC.KeepAlive(these TableStateTrackers we just made) in our callbacks to make sure that
        //    the trackers cannot possibly be finalized until the callbacks have come back (and our owner has
        //    called Cleanup).
        private class CleanupState
        {
            private readonly TableStateScope _scope;
            private readonly TableStateTracker[] _trackers;

            public CleanupState(TableState[] dependentStates, TableState childState)
            {
                _scope = new TableStateScope();
                _trackers = dependentStates.Concat(new[] {childState})
                    .Select(ts => TableStateTracker.Create(_scope, ts)).ToArray();
            }

            public void Cleanup()
            {
                GC.KeepAlive(_trackers);
                _scope.Dispose();
            }
        }

        private class ItdCallbackState
        {
            private readonly TableStateBuilder _childBuilder;
            private readonly CleanupState _cleanup;

            public ItdCallbackState(TableStateBuilder childBuilder, CleanupState cleanup)
            {
                _childBuilder = childBuilder;
                _cleanup = cleanup;
            }

            public void OnSuccess(InitialTableDefinition itd)
            {
                _cleanup.Cleanup();
                _childBuilder.SuccessAction(itd);
            }

            public void OnFailure(string error)
            {
                _cleanup.Cleanup();
                _childBuilder.FailureAction(error);
            }
        }

        /// <summary>
        /// Invoke a server operation.
        /// </summary>
        /// <param name="dependentStates">The TableStates that the asynchronous call depends on.
        ///   We put some effort into making sure these TableStates stay alive until the async
        ///   call is done</param>
        /// <param name="childBuilder">The TableStateBuilder that holds information for the result TableState
        ///   that is being built.</param>
        /// <param name="invoker">This is a little lambda that we use in order to capture the server
        ///   call and put run it on its own thread. The caller is expected to pass in boilerplate like
        ///       (workerServer, successAction, failureAction) =>
        ///           workerServer->whateverRequestAsync(arg1, args2, successAction, failureAction)
        ///  Basically the caller is wrapping a call to 'whateverRequestAsync' and forwarding the successAction
        ///  and failureAction that we provide here.</param>
        internal void InvokeServerForItd(TableState[] dependentStates, TableStateBuilder childBuilder,
            Action<WorkerServer, Action<InitialTableDefinition>, Action<string>> invoker)
        {
            var cleanupState = new CleanupState(dependentStates, childBuilder.TableState);
            var cbs = new ItdCallbackState(childBuilder, cleanupState);
            _serverWrapper.InvokeServerAsync(invoker, cbs.OnSuccess, cbs.OnFailure);
        }

        /// <summary>
        /// The rationale for this class is as follows. When we make a BatchTableRequest, the information we want
        /// doesn't come back directly in the BatchTableResponse, but rather it comes as an asynchronous
        /// InitialTableDefinition that follows. So this class acts as a sort of state machine:
        /// 1. Send Batch Table Request. If there's an error in sending, fire the failure callback and you're done.
        /// 2. Wait for Batch Table Response
        /// 3. Upon Batch Table Response, if this contains an error indication, fire the failure callback and you're done.
        /// 4. Wait for Initial Table Definition.
        /// 5. Upon Initial Table Definition, fire the success callback and you're done.
        /// </summary>
        private class BtrCallbackState
        {
            private readonly TableStateBuilder _childBuilder;
            private readonly CleanupState _cleanup;
            private readonly IWorkerClient _workerClient;
            private bool _actionFired = false;

            public BtrCallbackState(TableStateBuilder childBuilder, CleanupState cleanup,
                IWorkerClient workerClient)
            {
                _childBuilder = childBuilder;
                _workerClient = workerClient;
                _cleanup = cleanup;
                _workerClient.AddItdWatcher(childBuilder.TableHandle, OnInitialTableDefinition, OnFailure);
            }

            public void OnBatchTableResponse(BatchTableResponse btr)
            {
                if (btr.FailureMessages != null && btr.FailureMessages.Length > 0)
                {
                    OnFailure(btr.FailureMessages[0]);
                }
                // Otherwise, let the situation play out, waiting for the ITD notification.
            }

            public void OnFailure(string error)
            {
                if (!OkToFinish())
                {
                    return;
                }
                _cleanup.Cleanup();
                _childBuilder.FailureAction(error);
            }

            private void OnInitialTableDefinition(InitialTableDefinition itd)
            {
                if (!OkToFinish())
                {
                    return;
                }
                _cleanup.Cleanup();
                _childBuilder.SuccessAction(itd);
            }

            private bool OkToFinish()
            {
                lock (this)
                {
                    if (_actionFired)
                    {
                        return false;
                    }
                    _actionFired = true;
                }
                _workerClient.RemoveItdWatcher(_childBuilder.TableHandle);
                return true;
            }
        }

        /// <summary>
        /// Invoke a BatchTableRequest operation.
        /// </summary>
        /// <param name="dependentStates">The TableStates that the asynchronous call depends on.
        ///   We put some effort into making sure these TableStates stay alive until the async
        ///   call is done</param>
        /// <param name="childBuilder">The TableStateBuilder that holds information for the result TableState
        ///   that is being built.</param>
        /// <param name="op">The batch operation being invoked.</param>
        internal void InvokeServerForBtr(TableState[] dependentStates, TableStateBuilder childBuilder,
            BatchTableRequest.BatchTableRequestSerializedTableOps op)
        {
            var cleanupState = new CleanupState(dependentStates, childBuilder.TableState);
            var cbs = new BtrCallbackState(childBuilder, cleanupState, WorkerClient);
            var btr = new BatchTableRequest
            {
                Ops = new[] {op}
            };
            Action<WorkerServer, Action<BatchTableResponse>, Action<string>> invoker =
                (ws, sa, fa) => ws.BatchAsync(btr, sa, fa, fa);
            _serverWrapper.InvokeServerAsync(invoker, cbs.OnBatchTableResponse, cbs.OnFailure);
        }
    }
}
