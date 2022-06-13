/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;

namespace Deephaven.OpenAPI.Core.API
{
    public interface IServer<TS,TC> : IDisposable where TS : IServer<TS,TC> where TC : IClient<TC,TS>
    {
        /// <summary>
        /// Called when a client has initiated a connection to the server. Not
        /// to be invoked directly by the client.
        /// </summary>
        /// <param name="connection">Connection.</param>
        /// <param name="client">Client.</param>
        void OnOpen(IConnection connection, TC client);

        /// <summary>
        /// Called as a client connection is lost. Not to be invoked directly by
        /// the client.
        /// </summary>
        /// <param name="connection">Connection.</param>
        /// <param name="client">Client.</param>
        void OnClose(IConnection connection, TC client);

        void OnError(Exception exception);

        /// <summary>
        /// Gets the client for the currently running request, if any. Should
        /// return the value last passed to the <see cref="SetClient(TC)"/>
        /// method (if on the server, within the current thread).
        /// </summary>
        /// <returns>The current client object in use.</returns>
        TC GetClient();

        /// <summary>
        /// Sets the current client to call back to. Called on the server to
        /// indicate that the given client instance is about to make a call.
        /// Called by the client to specify which client instance should have
        /// messages forwarded to it.
        /// </summary>
        /// <param name="client">the Client object to use on this server.</param>
        void SetClient(TC client);

        /// <summary>
        /// Start a watchdog process.  If the watchdog is not fed within the timeout window (specified in millis)
        /// all existing operations are terminated with an error.
        /// </summary>
        /// <param name="timeoutMillis">the period in milliseconds where the watchdog must be fed</param>
        void StartWatchdog(long timeoutMillis);

        /// <summary>
        /// Feed the watchdog.  This must be called within each timeout period, when <see cref="StartWatchdog"/>
        /// has been used.
        /// </summary>
        void FeedWatchdog();
    }
}
