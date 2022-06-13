/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client
{
    /// <summary>
    /// This class provides a no-op implementation of the interface methods in <see cref="IOpenApiListener"/>.
    /// It is provided as a convenience to callers who only want to implement specific methods.
    /// </summary>
    public abstract class AbstractOpenApiListener : IOpenApiListener
    {
        public virtual void OnAuthTokenError(IOpenApiClient openApiClient, string error) { }
        public virtual void OnAuthTokenRefresh(IOpenApiClient openApiClient, RefreshToken refreshToken) { }
        public virtual void OnClosed(IOpenApiClient openApiClient, ushort code, string message) { }
        public virtual void OnError(IOpenApiClient openApiClient, Exception exception) { }
        public virtual void OnOpen(IOpenApiClient openApiClient) { }
        public virtual void OnPersistentQueryAdded(IOpenApiClient openApiClient, IPersistentQueryConfig persistentQueryConfig) { }
        public virtual void OnPersistentQueryModified(IOpenApiClient openApiClient, IPersistentQueryConfig persistentQueryConfig) { }
        public virtual void OnPersistentQueryRemoved(IOpenApiClient openApiClient, IPersistentQueryConfig persistentQueryConfig) { }
    }
}
