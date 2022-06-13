/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using Deephaven.OpenAPI.Core.RPC.Serialization.API;
using Deephaven.OpenAPI.Core.RPC.Serialization.Stream.Binary;
using System;
using System.IO;
using System.Net.WebSockets;
using Deephaven.OpenAPI.Core.API.Util;

namespace Deephaven.OpenAPI.Core.API
{
    /// <summary>
    /// A delegate for providing a concrete server endpoint. The endpoint
    /// should use the given functions for serialization.
    /// </summary>
    public delegate TS ServerImplFactory<out TS, TC>(
        Func<ITypeSerializer, BinarySerializationStreamWriter> writerFactory,
        Action<BinarySerializationStreamWriter> send,
        Action<Action<ISerializationStreamReader>, ITypeSerializer> onMessage,
        Action onClose) where TS : IServer<TS, TC> where TC : IClient<TC, TS>;

    /// <summary>
    /// Base class to be extended and given a concrete Server interface in a
    /// client project, causing code to be generated to connect to a websocket
    /// server.
    /// </summary>
    public class ServerBuilder<TS, TC> where TS : IServer<TS, TC> where TC : IClient<TC, TS>
    {
        private string _url;
        private readonly ServerImplFactory<TS,TC> _serverImplFactory;

        private class ServerImpl
        {
            private readonly WebSocketWrapper _webSocketWrapper;
            private readonly TS _endpoint;
            private Action<MemoryStream> _onMessage;

            public TS GetEndpoint()
            {
                return _endpoint;
            }

            private void HandleOpen()
            {
                _endpoint.GetClient().OnOpen();
            }

            private void HandleMessage(MemoryStream ms)
            {
                _onMessage(ms);
            }

            private void HandleClose(ushort code, string reason)
            {
                _endpoint.GetClient().OnClose(code, reason);
            }

            private void HandleError(Exception e)
            {
                _endpoint.GetClient().OnError(e);
            }

            public ServerImpl(string url, ServerImplFactory<TS,TC> serverImplFactory, TC client, int timeoutMillis)
            {
                _endpoint = serverImplFactory(serializer =>
                    {
                        var writer = new BinarySerializationStreamWriter(serializer);
                        writer.PrepareToWrite();
                        return writer;
                    },
                    writer =>
                    {
                        var msg = writer.GetFullPayload().ToArray();
                        _webSocketWrapper.Send(msg);
                    },
                    (receive, serializer) =>
                    {
                        _onMessage = buffer =>
                        {
                            receive(new BinarySerializationStreamReader(serializer, buffer));
                        };
                    },
                    () =>
                    {
                        _webSocketWrapper.Close();
                    }
                );
                _endpoint.SetClient(client);
                client.SetServer(_endpoint);

                if(_endpoint is IHasChecksum ep)
                {
                    url = url + "?checksum=" + ep.Checksum;
                }

                _webSocketWrapper = WebSocketWrapper.Connect(url, timeoutMillis, HandleMessage,
                    HandleClose, HandleError);
                HandleOpen();
            }
        }

        protected ServerBuilder(ServerImplFactory<TS,TC> serverImplFactory)
        {
            _serverImplFactory = serverImplFactory;
        }

        public ServerBuilder<TS, TC> SetUrl(string url)
        {
            _url = url;
            return this;
        }

        /// <summary>
        /// Creates a new instance of the specified server type, starts, and
        /// returns it. May be called more than once to create additional
        /// connections, such as after the first is closed.
        /// </summary>
        /// <returns>The server endpoint.</returns>
        /// <param name="client">The Client that will receive incoming messages.</param>
        /// <param name="timeoutMillis">Number of milliseconds to wait before throwing a timeout exception</param>
        public TS Start(TC client, int timeoutMillis)
        {
            var serverImpl = new ServerImpl(_url, _serverImplFactory, client, timeoutMillis);
            return serverImpl.GetEndpoint();
        }

        /// <summary>
        /// Simple create method that takes the generated server endpoint's
        /// constructor and returns a functioning server builder.
        /// </summary>
        /// <returns>The of.</returns>
        /// <param name="serverImplFactory">Server impl factory.</param>
        public static ServerBuilder<TS,TC> Of(ServerImplFactory<TS, TC> serverImplFactory)
        {
            return new ServerBuilder<TS, TC>(serverImplFactory);
        }
    }
}
