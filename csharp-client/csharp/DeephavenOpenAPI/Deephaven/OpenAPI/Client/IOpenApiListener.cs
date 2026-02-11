/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client
{
    /// <summary>
    /// An interface that clients can use to listen for callbacks from the server.
    /// </summary>
    public interface IOpenApiListener
    {
        /// <summary>
        /// Callback invoked when a persistent query has been added.
        /// </summary>
        /// <param name="openApiClient">The OpenAPI client</param>
        /// <param name="persistentQueryConfig">The Persistent Query Config</param>
        void OnPersistentQueryAdded(IOpenApiClient openApiClient, IPersistentQueryConfig persistentQueryConfig);
        /// <summary>
        /// Callback invoked when a persistent query has been modified.
        /// </summary>
        /// <param name="openApiClient">The OpenAPI client</param>
        /// <param name="persistentQueryConfig">The Persistent Query Config</param>
        void OnPersistentQueryModified(IOpenApiClient openApiClient, IPersistentQueryConfig persistentQueryConfig);
        /// <summary>
        /// Callback invoked when a persistent query has been removed.
        /// </summary>
        /// <param name="openApiClient">The OpenAPI client</param>
        /// <param name="persistentQueryConfig">The Persistent Query Config</param>
        void OnPersistentQueryRemoved(IOpenApiClient openApiClient, IPersistentQueryConfig persistentQueryConfig);
        /// <summary>
        /// Callback invoked when the server connection is closed.
        /// </summary>
        /// <param name="openApiClient">The OpenAPI client</param>
        /// <param name="code">Numeric shutdown reason code (TODO(kosak))</param>
        /// <param name="reason">Human-readable shutdown reason</param>
        void OnClosed(IOpenApiClient openApiClient, ushort code, string reason);
        /// <summary>
        /// Callback invoked when the server connection has an error.
        /// </summary>
        /// <param name="openApiClient">The OpenAPI client</param>
        /// <param name="exception">Connection exception</param>
        void OnError(IOpenApiClient openApiClient, Exception exception);
        /// <summary>
        /// Callback invoked when the server connection has been successfully opened.
        /// </summary>
        /// <param name="openApiClient">The OpenAPI client</param>
        void OnOpen(IOpenApiClient openApiClient);
        /// <summary>
        /// Callback invoked when the server sends a refresh token.
        /// </summary>
        /// <param name="openApiClient">The OpenAPI client</param>
        /// <param name="authToken">The refresh token</param>
        void OnAuthTokenRefresh(IOpenApiClient openApiClient, RefreshToken authToken);
        /// <summary>
        /// Callback invoked when the server has a token refresh error.
        /// </summary>
        /// <param name="openApiClient">The OpenAPI client</param>
        /// <param name="error">Human-readable error</param>
        void OnAuthTokenError(IOpenApiClient openApiClient, string error);
    }
}
