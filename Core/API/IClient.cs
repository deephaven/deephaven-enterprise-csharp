/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;

namespace Deephaven.OpenAPI.Core.API
{
    /// <summary>
    /// Starting interface for building the methods the server may call on the
    /// client. User code may provide an instance of this type to a Server
    /// instance, where Start is invoked, and Client instances will be created
    /// automatically on the server as connections are created.
    /// </summary>
    public interface IClient<TC,TS> where TC : IClient<TC,TS> where TS : IServer<TS,TC>
    {
        /// <summary>
        /// Callback called when the connection to the server has been established. Should not be called directly from the server.
        /// </summary>
        void OnOpen();

        /// <summary>
        /// Callback called when the connection to the server has been closed. Should not be called directly from the server.
        /// </summary>
        void OnClose(ushort code, string reason);

        /// <summary>
        /// Called when an error occurs while handling a message in one of the other client methods. If a ConnectionErrorHandler
        /// is provided to the ServerBuilder, that will be used in handling serialization/deserialization and connection errors.
        /// </summary>
        /// <param name="ex">The error that occurred</param>
        void OnError(Exception ex);

        void SetServer(TS server);

        TS GetServer();
    }
}
