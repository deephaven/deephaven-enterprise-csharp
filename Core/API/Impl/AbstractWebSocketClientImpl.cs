/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using Deephaven.OpenAPI.Core.RPC.Serialization.API;
using Deephaven.OpenAPI.Core.RPC.Serialization.Stream.Binary;
using System;

namespace Deephaven.OpenAPI.Core.API.Impl
{
    /// <summary>
    /// A web socket-based implementation of the abstract client endpoint.
    /// Each interface is paired with concrete server and client types provided by
    /// the user (TS and TC respectively).
    /// </summary>
    /// <typeparam name="TS">The concrete server type</typeparam>
    /// <typeparam name="TC">The concrete client type</typeparam>
    public abstract class AbstractWebSocketClientImpl<TS, TC> : AbstractEndpointImpl
        where TS : IServer<TS,TC>
        where TC : IClient<TC,TS>
    {
        private TS _server;

        protected AbstractWebSocketClientImpl(
            Func<ITypeSerializer, BinarySerializationStreamWriter> writerFactory,
            Action<BinarySerializationStreamWriter> send,
            ITypeSerializer typeSerializer,
            Action<Action<ISerializationStreamReader>, ITypeSerializer> onMessage) : base(writerFactory, send, typeSerializer, onMessage)
        {

        }

        public virtual void OnOpen()
        {
            throw new NotSupportedException("This method cannot be called from server code");
        }

        public virtual void OnClose()
        {
            throw new NotSupportedException("This method cannot be called from server code");
        }

        public virtual void OnError(Exception ex)
        {
            throw new NotSupportedException("This method cannot be called from server code");
        }

        public void SetServer(TS server)
        {
            this._server = server;
        }

        public TS GetServer()
        {
            return _server;
        }
    }
}
