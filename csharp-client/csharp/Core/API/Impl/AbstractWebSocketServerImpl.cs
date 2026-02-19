/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Core.API;
using Deephaven.OpenAPI.Core.API.Impl;
using Deephaven.OpenAPI.Core.RPC.Serialization.API;
using Deephaven.OpenAPI.Core.RPC.Serialization.Stream.Binary;

namespace Deephaven.OpenAPI.Core.Api.Impl
{
    /// <summary>
    /// A web socket-based implementation of the abstract server endpoint.
    /// Each interface is paired with concrete server and client types provided by
    /// the user (TS and TC respectively).
    /// This implements IDisposable and will invoke the provided close behaviour
    /// when disposed.
    /// </summary>
    /// <typeparam name="TS">The concrete server type</typeparam>
    /// <typeparam name="TC">The concrete client type</typeparam>
    public abstract class AbstractWebSocketServerImpl<TS,TC> : AbstractEndpointImpl, IServer<TS,TC>
        where TS : IServer<TS,TC>
        where TC: IClient<TC,TS>
    {
        private TC _client;
        private readonly Action _onClose;

        protected AbstractWebSocketServerImpl(
            Func<ITypeSerializer, BinarySerializationStreamWriter> writerFactory,
            Action<BinarySerializationStreamWriter> send,
            ITypeSerializer typeSerializer,
            Action<Action<ISerializationStreamReader>, ITypeSerializer> onMessage,
            Action onClose) : base(writerFactory, send, typeSerializer, onMessage)
        {
            _onClose = onClose ?? throw new ArgumentException("Server factory must provide close() behavior!");
        }

        public void OnOpen(IConnection connection, TC client)
        {
            throw new NotSupportedException("Cannot be called from client code");
        }

        public void OnClose(IConnection connection, TC client)
        {
            throw new NotSupportedException("Cannot be called from client code");
        }

        public void OnError(Exception exception)
        {
            throw new NotSupportedException("Cannot be called from client code");
        }

        public TC GetClient()
        {
            return _client;
        }

        /// <summary>
        /// Set the client object that will receive messages from the server.
        /// </summary>
        /// <param name="client"></param>
        public void SetClient(TC client)
        {
            _client = client;
        }

        public void Dispose()
        {
            _onClose();
        }
    }
}
