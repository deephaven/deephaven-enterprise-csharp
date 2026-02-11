/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace Deephaven.OpenAPI.Client
{
    /// <summary>
    /// An interface to a persistent query configuration. Persistent Queries
    /// are uniquely identified by the <see cref="Serial"/> identifier and the
    /// current status reflected by the <see cref="Status"/> property.
    /// </summary>
    public interface IPersistentQueryConfig
    {
        long Serial { get; }
        string Name { get; }
        PersistentQueryStatus Status { get; }
        string ServiceId { get; }

        IPersistentQueryConfigInternal Internal { get; }
    }

    public interface IPersistentQueryConfigInternal
    {
        string WebsocketUrl { get; }
    }
}
