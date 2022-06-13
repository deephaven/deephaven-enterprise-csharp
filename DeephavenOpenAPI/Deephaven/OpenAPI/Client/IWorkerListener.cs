/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace Deephaven.OpenAPI.Client
{
    /// <summary>
    /// An interface that clients can use to listen for callbacks from the Worker.
    /// </summary>
    public interface IWorkerListener
    {
        /// <summary>
        /// Callback invoked when a Worker has been opened.
        /// </summary>
        /// <param name="workerSession">The Worker Session</param>
        void OnOpen(IWorkerSession workerSession);
        /// <summary>
        /// Callback invoked when a Worker has been closed.
        /// </summary>
        /// <param name="workerSession">The Worker Session</param>
        /// <param name="code">Numeric error code (TODO(kosak))</param>
        /// <param name="err">Human-readable error message</param>
        void OnClosed(IWorkerSession workerSession, ushort code, string err);
        /// <summary>
        /// Callback invoked when there is a Worker exception
        /// </summary>
        /// <param name="workerSession">The Worker Session</param>
        /// <param name="ex">The exception</param>
        void OnError(IWorkerSession workerSession, Exception ex);
        /// <summary>
        /// Callback invoked when a Worker responds to a ping
        /// </summary>
        /// <param name="workerSession">The Worker Session</param>
        void OnPing(IWorkerSession workerSession);
        /// <summary>
        /// Callback invoked when a Worker records a log message
        /// </summary>
        /// <param name="workerSession">The Worker Session</param>
        /// <param name="logMessage">The log message</param>
        void OnLogMessage(IWorkerSession workerSession, LogMessage logMessage);
    }
}
