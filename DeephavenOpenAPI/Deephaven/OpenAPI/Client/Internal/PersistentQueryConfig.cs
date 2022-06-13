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
    internal class PersistentQueryConfig : IPersistentQueryConfig
    {
        private readonly QueryConfig _queryConfig;

        public PersistentQueryConfig(QueryConfig queryConfig) => _queryConfig = queryConfig;

        public long Serial => _queryConfig.Serial;
        public string ScriptLanguage => _queryConfig.ScriptLanguage;
        public string ConfigurationType => _queryConfig.ConfigurationType;
        public string Name => _queryConfig.Name;
        public string Owner => _queryConfig.Owner;

        public PersistentQueryStatus Status => (PersistentQueryStatus)Enum.Parse(typeof(PersistentQueryStatus), _queryConfig.Status);
        public string[][] Objects => _queryConfig.Objects;
        public string[] Scheduling => _queryConfig.Scheduling;
        public string FullStackTrace => _queryConfig.FullStackTrace;
        public string WebsocketUrl => _queryConfig.WebsocketUrl;
        public string ServiceId => _queryConfig.ServiceId;

        public IPersistentQueryConfigInternal Internal => new MyInternal(_queryConfig.WebsocketUrl);

        private class MyInternal : IPersistentQueryConfigInternal
        {
            public MyInternal(string websocketUrl) => WebsocketUrl = websocketUrl;
            public string WebsocketUrl { get; }
        }
    }
}
