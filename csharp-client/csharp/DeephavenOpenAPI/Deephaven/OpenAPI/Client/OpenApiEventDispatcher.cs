/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client
{
    /// <summary>
    /// An implementation of <see cref="IWorkerListener"/> that
    /// dispatches worker updates as events instead of requiring the user to
    /// implement listener methods.
    /// </summary>
    public class OpenApiEventDispatcher : IOpenApiListener
    {
        public event Action<IOpenApiClient, IPersistentQueryConfig> PersistentQueryAdded;
        public event Action<IOpenApiClient, IPersistentQueryConfig> PersistentQueryModified;
        public event Action<IOpenApiClient, IPersistentQueryConfig> PersistentQueryRemoved;
        public event Action<IOpenApiClient, ushort, string> Closed;
        public event Action<IOpenApiClient, Exception> Error;
        public event Action<IOpenApiClient> Open;
        public event Action<IOpenApiClient, RefreshToken> AuthTokenRefresh;
        public event Action<IOpenApiClient, string> AuthTokenError;

        public void OnAuthTokenError(IOpenApiClient openApiClient, string error)
        {
            AuthTokenError?.Invoke(openApiClient, error);
        }

        public void OnAuthTokenRefresh(IOpenApiClient openApiClient, RefreshToken authToken)
        {
            AuthTokenRefresh?.Invoke(openApiClient, authToken);
        }

        public void OnClosed(IOpenApiClient openApiClient, ushort code, string reason)
        {
            Closed?.Invoke(openApiClient, code, reason);
        }

        public void OnError(IOpenApiClient openApiClient, Exception exception)
        {
            Error?.Invoke(openApiClient, exception);
        }

        public void OnOpen(IOpenApiClient openApiClient)
        {
            Open?.Invoke(openApiClient);
        }

        public void OnPersistentQueryAdded(IOpenApiClient openApiClient, IPersistentQueryConfig persistentQueryConfig)
        {
            PersistentQueryAdded?.Invoke(openApiClient, persistentQueryConfig);
        }

        public void OnPersistentQueryModified(IOpenApiClient openApiClient, IPersistentQueryConfig persistentQueryConfig)
        {
            PersistentQueryModified?.Invoke(openApiClient, persistentQueryConfig);
        }

        public void OnPersistentQueryRemoved(IOpenApiClient openApiClient, IPersistentQueryConfig persistentQueryConfig)
        {
            PersistentQueryRemoved?.Invoke(openApiClient, persistentQueryConfig);
        }
    }
}
