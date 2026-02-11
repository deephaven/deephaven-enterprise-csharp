/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client.Internal
{
    /// <summary>
    /// Thin, immutable wrapper around QueryConfig
    /// </summary>
    internal class PersistentQueryConfig : IPersistentQueryConfig {
        private readonly QueryStatusWrapper _wrapper;

        public PersistentQueryConfig(QueryStatusWrapper wrapper) => _wrapper = wrapper;

        public long Serial => _wrapper.Designated.Serial;
        public string Name => _wrapper.Config.Name;
        public string ServiceId => _wrapper.Designated.ServiceId;

        public PersistentQueryStatus Status {
            get {
                var designated = _wrapper.Designated;
                return designated == null
                  ? PersistentQueryStatus.Uninitialized
                  : (PersistentQueryStatus)Enum.Parse(typeof(PersistentQueryStatus),
                    designated.Status);
            }
        }

        public IPersistentQueryConfigInternal Internal => new MyInternal(_wrapper.Designated.WebsocketUrl);

        private class MyInternal : IPersistentQueryConfigInternal
        {
            public MyInternal(string websocketUrl) => WebsocketUrl = websocketUrl;
            public string WebsocketUrl { get; }
        }
    }
}
