/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace Deephaven.OpenAPI.Client
{
    /// <summary>
    /// An implementation of <see cref="IWorkerListener"/> that
    /// dispatches worker updates as events instead of requiring the user to
    /// implement listener methods.
    /// </summary>
    public class WorkerEventDispatcher : IWorkerListener
    {
        public event Action<IWorkerSession> Open;
        public event Action<IWorkerSession, ushort, string> Closed;
        public event Action<IWorkerSession, Exception> Error;
        public event Action<IWorkerSession> Ping;
        public event Action<IWorkerSession, LogMessage> LogMessage;

        public void OnClosed(IWorkerSession workerSession, ushort code, string reason)
        {
            Closed?.Invoke(workerSession, code, reason);
        }

        public void OnError(IWorkerSession workerSession, Exception ex)
        {
            Error?.Invoke(workerSession, ex);
        }

        public void OnLogMessage(IWorkerSession workerSession, LogMessage logMessage)
        {
            LogMessage?.Invoke(workerSession, logMessage);
        }

        public void OnOpen(IWorkerSession workerSession)
        {
            Open?.Invoke(workerSession);
        }

        public void OnPing(IWorkerSession workerSession)
        {
            Ping?.Invoke(workerSession);
        }
    }
}
