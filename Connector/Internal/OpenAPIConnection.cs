/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Threading;
using Deephaven.OpenAPI.Core.API;
using Deephaven.OpenAPI.Shared.Cmd;
using Deephaven.OpenAPI.Shared.Data;
using Deephaven.OpenAPI.Shared.Ide;
using Deephaven.OpenAPI.Shared.Primary;

namespace Deephaven.Connector.Internal
{
    internal class OpenAPIConnection : IDisposable
    {
        private const int ConnectionTimeoutMillis = 15 * 1000;

        private class MyClient : WebApiClient
        {
            private WebApiServer _server;

            public void ConsoleDeath(RequestId id)
            {
            }

            public WebApiServer GetServer()
            {
                return _server;
            }

            public void OnClose(ushort code, string reason)
            {
            }

            public void OnError(Exception ex)
            {
            }

            public void OnOpen()
            {
            }

            public void QueryAdded(QueryConfig config)
            {
            }

            public void QueryModified(QueryConfig config)
            {
            }

            public void QueryRemoved(QueryConfig config)
            {
            }

            public void Ids6976(char[] bugMitigation1, sbyte[] bugMitigation2)
            {
            }

            public void SetServer(WebApiServer server)
            {
                _server = server;
            }
        }

        private ServerBuilder<WebApiServer, WebApiClient> _serverBuilder;
        private WebApiServer _apiServer;
        private RefreshToken _refreshToken;
        private MyClient _client;
        private Timer _timer;

        public Action<RefreshToken> OnTokenRefresh;
        public Action<string> OnError;

        public static class RequestFactory
        {
            private static int _requestId = 1;

            public static RequestId GetRequest()
            {
                return new RequestId(_requestId++);
            }
        }

        public OpenAPIConnection(string url, string username, string pwd, string operateAs)
        {
            if (url == null)
            {
                throw new ArgumentException(nameof(url));
            }
            if(username == null)
            {
                throw new ArgumentException(nameof(username));
            }
            if(pwd == null)
            {
                throw new ArgumentException(nameof(pwd));
            }
            _serverBuilder = ServerBuilder<WebApiServer, WebApiClient>.Of(
                (factory, send, message, close) => new WebApiServer_Impl(factory, send, message, close)).SetUrl(url);
            _client = new MyClient();
            _apiServer = _serverBuilder.Start(_client, ConnectionTimeoutMillis);

            _refreshToken = _apiServer.Login(username, pwd, operateAs ?? username);

            _timer = new Timer(ScheduleAuthTokenRefresh, null, TimeSpan.FromSeconds(300), TimeSpan.FromSeconds(300));
        }

        private void ScheduleAuthTokenRefresh(object o)
        {
            void ErrorCallback(string e) => OnError?.Invoke(e);
            _apiServer.RefreshAsync(refreshToken =>
                {
                    _refreshToken = refreshToken;
                    OnTokenRefresh?.Invoke(refreshToken);
                },
                ErrorCallback,
                ErrorCallback);
        }

        public ConsoleConnection GetConsoleConnection(ConsoleSessionType sessionType, int remoteDebugPort,
            bool suspendWorker, int maxHeapMb, int timeoutMs, bool localDateAsString, bool localTimeAsString)
        {
            if (_apiServer == null)
                throw new Exception("Cannot get console connection, API server connection already closed.");
            return new ConsoleConnection(_apiServer, RequestFactory.GetRequest(),
                sessionType, remoteDebugPort, suspendWorker, maxHeapMb,
                timeoutMs, localDateAsString, localTimeAsString);
        }

        private bool _disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _timer.Dispose();
            _apiServer.Dispose();
            _timer = null;
            _apiServer = null;
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
    }
}
