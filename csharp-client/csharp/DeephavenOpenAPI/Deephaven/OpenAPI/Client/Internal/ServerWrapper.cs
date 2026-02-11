/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Threading.Tasks;
using Deephaven.OpenAPI.Core.API;

namespace Deephaven.OpenAPI.Client.Internal
{
    public class ServerWrapper<TServer>
    {
        private readonly TServer _server;
        private readonly SequentialWorkPool _workPool;

        public TServer Server => _server;

        public ServerWrapper(TServer server)
        {
            _server = server;
            _workPool = new SequentialWorkPool();
        }

        public void InvokeServer(Action<TServer> deferred)
        {
            _workPool.Add(() => deferred(_server));
        }

        public void InvokeServerAsync<T>(Action<TServer, Action<T>, Action<string>> callback,
            Action<T> successAction, Action<string> failureAction)
        {
            Action deferred = () =>
            {
                // If callback itself throws, divert the exception to failureAction.
                try
                {
                    callback(_server, successAction, failureAction);
                } catch (Exception e)
                {
                    failureAction(e.Message);
                }
            };
            _workPool.Add(deferred);
        }

        public Task<T> InvokeServerTask<T>(Action<TServer, Action<T>, Action<string>> callback)
        {
            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

            Action<T> successAction = result => tcs.SetResult(result);
            Action<string> failureAction = err => tcs.SetException(new Exception(err));
            InvokeServerAsync(callback, successAction, failureAction);
            return tcs.Task;
        }
    }
}
