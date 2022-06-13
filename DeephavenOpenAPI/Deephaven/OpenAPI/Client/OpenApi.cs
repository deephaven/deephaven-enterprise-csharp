/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using Deephaven.OpenAPI.Client.Internal;

namespace Deephaven.OpenAPI.Client
{
    /// <summary>
    /// A factory interface for initiating a connection to the Open API.
    /// </summary>
    public static class OpenApi
    {
        /// <summary>
        /// Connect to the Open API server and return a client that can be used
        /// to login and create/attach to workers.
        /// </summary>
        /// <param name="host">The Open API host/IP address</param>
        /// <param name="port">The Open API port</param>
        /// <param name="openApiListener">An optional listener implementation</param>
        /// <param name="connectionTimeoutMillis">Connection timeout in milliseconds</param>
        /// <returns>A new client object</returns>
        public static IOpenApiClient Connect(string host, int port = 8123, IOpenApiListener openApiListener = null,
            int connectionTimeoutMillis = ClientConstants.DefaultConnectionTimeoutMillis)
        {
            return OpenApiClient.Create(host, port, openApiListener, connectionTimeoutMillis);
        }
    }
}
