using System;
using System.Collections.Generic;
using System.Threading;
using Deephaven.OpenAPI.Core.API;
using Deephaven.OpenAPI.Shared.Primary;
using Deephaven.OpenAPI.Shared.Cmd;
using Deephaven.OpenAPI.Shared.Data;

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
            private Dictionary<string, QueryConfig> _queryConfigMap = new Dictionary<string, QueryConfig>();

            /// <summary>
            /// A reference to the server this client is paired with.
            /// </summary>
            private WebApiServer _server;

            public QueryConfig GetQueryConfig(string name)
            {
                QueryConfig queryConfig;
                if (_queryConfigMap.TryGetValue(name, out queryConfig))
                {
                    return queryConfig;
                }
                return null;
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
                Console.WriteLine("Open");
            }

            /// <summary>
            /// Called by the server when a new persistent query is added.
            /// </summary>
            /// <param name="config">Configuration information for the new persistent query</param>
            public void QueryAdded(QueryConfig config)
            {
                _queryConfigMap[config.Name] = config;
                Console.WriteLine("Query added: " + config.Name + " [" + config.Status + "] " +
                                      config.WebsocketUrl);
                if (config.Status.Equals("Running"))
                {
                    Console.WriteLine("Objects:");
                    foreach (var configObject in config.Objects)
                    {
                        Console.WriteLine("\t" + configObject[0] + " " + configObject[1]);
                    }
                }
            }

            /// <summary>
            /// Called by the server when an existing persistent query is modified.
            /// </summary>
            /// <param name="config">Configuration information for the modified persistent query</param>
            public void QueryModified(QueryConfig config)
            {
                _queryConfigMap[config.Name] = config;
                Console.Out.WriteLine("Query modified: " + config.Name);
            }

            /// <summary>
            /// Called by the server when a persistent query is removed.
            /// </summary>
            /// <param name="config">Configuration information for the removed persistent query</param>
            public void QueryRemoved(QueryConfig config)
            {
                _queryConfigMap.Remove(config.Name);
                Console.Out.WriteLine("Query removed: " + config.Name);
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
            _server = _serverBuilder.Start(_client);

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
            _server.RefreshAsync(refreshToken => { _refreshToken = refreshToken; Console.WriteLine("Auth token refreshed.");},
                error => { Console.WriteLine("Error refreshing token: " + error); });
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
                err => Console.WriteLine("Error getting connect token: " + err));
        }

        public QueryConfig GetQueryConfig(string name)
        {
            return _client.GetQueryConfig(name);
        }

        public void Dispose()
        {
            _timer.Dispose();
            _server.Dispose();
        }
    }
}
