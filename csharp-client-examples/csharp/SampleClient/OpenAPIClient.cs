using Deephaven.OpenAPI.Core.API;
using Deephaven.OpenAPI.Shared.Cmd;
using Deephaven.OpenAPI.Shared.Data;
using Deephaven.OpenAPI.Shared.Primary;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SampleClient
{
    /// <summary>
    /// A sample class that demonstrates some basic wasy to use the Deephaven Open API.
    /// </summary>
    class OpenAPIClient : IDisposable
    {
        /// <summary>
        /// This implementation receives incoming messages from the Open API
        /// associated with system level events.
        /// </summary>
        private class MyClient : WebApiClient
        {
            /// <summary>
            /// A map to keep track of running persistent queries
            /// </summary>
            private readonly Dictionary<string, QueryStatusWrapper> _queryConfigMap = new();

            /// <summary>
            /// A reference to the server this client is paired with.
            /// </summary>
            private WebApiServer _server;

            public QueryStatusWrapper GetQueryStatusWrapper(string name) {
              return _queryConfigMap.GetValueOrDefault(name);
            }

            /// <summary>
            /// Called when a console dies.
            /// </summary>
            /// <param name="id"></param>
            public void ConsoleDeath(RequestId id)
            {
                Console.WriteLine("Console death, client id: " + id.ClientId);
            }

            /// <summary>
            /// Get a reference to the server associated with this client.
            /// </summary>
            /// <returns></returns>
            public WebApiServer GetServer()
            {
                return _server;
            }

            /// <summary>
            /// Called when the connection to the server is lost for any reason.
            /// </summary>
            /// <param name="code">Numeric code indicating the reason, provided by the WebSocket</param>
            /// <param name="reason">Text string indicating reason for the close, provided by the WebSocket</param>
            public void OnClose(ushort code, string reason)
            {
                Console.WriteLine("OnClose: {0} ({1})", reason, code);
            }

            /// <summary>
            /// Called when an error occurs.
            /// </summary>
            /// <param name="ex"></param>
            public void OnError(Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }

            /// <summary>
            /// Called when the connection to the open API server is opened.
            /// </summary>
            public void OnOpen()
            {
                Console.WriteLine("Connection is Open");
            }

            /// <summary>
            /// Called by the server when a new persistent query is added.
            /// </summary>
            /// <param name="wrapper">Configuration information for the new persistent query</param>
            public void QueryAdded(QueryStatusWrapper wrapper)
            {
                var config = wrapper.Config;
                _queryConfigMap[config.Name] = wrapper;
                if (wrapper.Designated == null)
                {
                  Console.WriteLine($"Query not started: {config.Name}");
                  return;
                }
                Console.WriteLine("Query added: " + config.Name + " [" + wrapper.Designated.Status + "] " +
                                      wrapper.Designated.WebsocketUrl);
                if (wrapper.Designated.Status.Equals("Running"))
                {
                    Console.WriteLine("Objects:");
                    foreach (var configObject in wrapper.Designated.Objects)
                    {
                        Console.WriteLine("\t" + configObject[0] + " " + configObject[1]);
                    }
                }
            }

            /// <summary>
            /// Called by the server when an existing persistent query is modified.
            /// </summary>
            /// <param name="config">Configuration information for the modified persistent query</param>
            public void QueryModified(QueryStatusWrapper wrapper)
            {
                _queryConfigMap[wrapper.Config.Name] = wrapper;
                Console.Out.WriteLine("Query modified: " + wrapper.Config.Name);
            }

            /// <summary>
            /// Called by the server when a persistent query is removed.
            /// </summary>
            /// <param name="config">Configuration information for the removed persistent query</param>
            public void QueryRemoved(QueryStatusWrapper wrapper)
            {
                _queryConfigMap.Remove(wrapper.Config.Name);
                Console.Out.WriteLine("Query removed: " + wrapper.Config.Name);
            }

            public void PreauthenticationKeepAlive()
            {
              // do nothing
            }

            public void Ids6976(char[] bugMitigation1, sbyte[] bugMitigation2)
            {
              // do nothing
            }

            public void SetServer(WebApiServer server)
            {
                _server = server;
            }
        }

        private ServerBuilder<WebApiServer,WebApiClient> _serverBuilder;
        private WebApiServer _server;
        private RefreshToken _refreshToken;
        private MyClient _client;
        private Timer _timer;

        public OpenAPIClient(string url, string username, string pwd)
        {
            _serverBuilder = ServerBuilder<WebApiServer, WebApiClient>.Of(
                (factory, send, message, close) => new WebApiServer_Impl(factory, send, message, close)).SetUrl(url);
            _client = new MyClient();
            _server = _serverBuilder.Start(_client, 60 * 1000);

            // login using a blocking function method
            // Like all Open API methods, this can also be called asynchronously if desired
            try
            {
                _refreshToken = _server.Login(username, pwd, username);
                Console.WriteLine("Yay, connected!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error logging in: " + e.Message);
            }

            // start a timer to refresh our auth token every 5 minutes
            _timer = new Timer(ScheduleAuthTokenRefresh, null, TimeSpan.FromSeconds(300), TimeSpan.FromSeconds(300));
        }

        private void ScheduleAuthTokenRefresh(object o)
        {
            _server.RefreshAsync(
              refreshToken => { _refreshToken = refreshToken; Console.WriteLine("Auth token refreshed.");},
              message => Console.WriteLine($"Failure refreshing token: {message}"),
                error => Console.WriteLine($"Error refreshing token: {error}"));
        }

        public ConnectToken LastConnectToken { get; set; }

        /// <summary>
        /// Ask the server for a new token we can use to create a worker connection.
        /// </summary>
        public void RefreshConnectToken()
        {
            _server.CreateAuthTokenAsync("RemoteQueryProcessor", token =>
                {
                    LastConnectToken = token;
                    Console.WriteLine("Got ConnectToken");
                },
                message => Console.WriteLine($"Failure getting connect token: {message}"),
                error => Console.WriteLine($"Error getting connect token: {error}"));
        }

        public QueryStatusWrapper GetQueryStatusWrapper(string name)
        {
            return _client.GetQueryStatusWrapper(name);
        }

        public void Dispose()
        {
            _timer.Dispose();
            _server.Dispose();
        }
    }
}
