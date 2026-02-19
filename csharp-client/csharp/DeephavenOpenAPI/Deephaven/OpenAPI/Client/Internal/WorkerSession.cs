/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Threading;
using Deephaven.OpenAPI.Core.API.Util;

namespace Deephaven.OpenAPI.Client.Internal
{
    internal class WorkerSession : IWorkerSession
    {
        private static long _nextFreeId;

        public long UniqueId { get; }
        private readonly ServerContext _serverContext;
        public IQueryScope QueryScope { get; }
        public string WebsocketUrl { get; }
        public string ServiceId { get; }
        private readonly Action<WorkerSession> _onDisposed;
        private bool _isDisposed = false;
        private readonly Timer _heartbeatTimer;

        internal WorkerSession(ServerContext serverContext, string websocketUrl, string serviceId,
            Action<WorkerSession> onDisposed)
        {
            UniqueId = Interlocked.Increment(ref _nextFreeId);
            _serverContext = serverContext;
            QueryScope = new QueryScope(serverContext);
            WebsocketUrl = websocketUrl;
            ServiceId = serviceId;
            _onDisposed = onDisposed;
            _heartbeatTimer = new Timer(FireHeartbeat);
        }

        ~WorkerSession()
        {
            // I don't want to block the finalizer queue, so I'm going to dispatch this on my own thread.
            ThreadPool.QueueUserWorkItem(SynchronousDisposeHelper);
        }

        /// <summary>
        /// Configure and start a heartbeat process to proactively monitor the connection state.
        /// </summary>
        /// <param name="expiryMillis">The amount of time in milliseconds that must pass without a response before
        ///                            an error results</param>
        public void ConfigureWatchdog(long expiryMillis)
        {
            if (expiryMillis <= 0)
            {
                throw new ArgumentException(
                    $"Watchdog Expiry ({expiryMillis}) must be positive");
            }
            _serverContext.StartWatchdog(expiryMillis);

            long heartbeatPeriod = expiryMillis / 2;
            _heartbeatTimer.Change(heartbeatPeriod, heartbeatPeriod);
        }

        /// <summary>
        /// Send a heartbeat to the server
        /// </summary>
        private void FireHeartbeat(object unused)
        {
            lock (this)
            {
                if (_isDisposed)
                {
                    return;
                }
                
                _serverContext.InvokeServer(s =>
                {
                    try
                    {
                        lock (this)
                        {
                            if (_isDisposed)
                            {
                                return;
                            }
                            
                            s.Pong();
                        }
                    }
                    catch (WatchdogException)
                    {
                        // This is fine;  You could argue that we should begin exponential walkout for
                        // firing further heartbeats.
                    }
                });
            }
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
            SynchronousDisposeHelper(null);
            GC.SuppressFinalize(this);
        }

        private void SynchronousDisposeHelper(object _)
        {
            QueryScope.Dispose();
            _serverContext.Dispose();
            _onDisposed(this);
        }
    }
}
