/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Threading.Tasks;
using Deephaven.OpenAPI.Client.Data;

namespace Deephaven.OpenAPI.Client
{
    /// <summary>
    /// An interface to a Deephaven worker. From a worker session, new queries
    /// can be originated by specifying a root table in a number of ways.
    /// </summary>
    public interface IWorkerSession : IDisposable
    {
        /// <summary>
        /// This worker session's query scope.
        /// </summary>
        IQueryScope QueryScope { get; }

        /// <summary>
        /// The Websocket URL of the worker, for informational purposes.
        /// </summary>
        string WebsocketUrl { get; }

        /// <summary>
        /// The worker's service ID, for informational purposes.
        /// </summary>
        string ServiceId { get; }
    }
}
