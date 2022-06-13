/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace Deephaven.OpenAPI.Client
{
    /// <summary>
    /// This class provides a no-op implementation of the interface methods in <see cref="IWorkerListener"/>.
    /// It is provided as a convenience to callers who only want to implement specific methods.
    /// </summary>
    public abstract class AbstractWorkerListener : IWorkerListener
    {
        public virtual void OnClosed(IWorkerSession workerSession, ushort code, string err) { }
        public virtual void OnError(IWorkerSession workerSession, Exception ex) { }
        public virtual void OnLogMessage(IWorkerSession workerSession, LogMessage logMessage) { }
        public virtual void OnOpen(IWorkerSession workerSession) { }
        public virtual void OnPing(IWorkerSession workerSession) { }
    }
}
