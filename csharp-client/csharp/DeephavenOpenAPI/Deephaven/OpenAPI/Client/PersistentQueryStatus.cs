/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
namespace Deephaven.OpenAPI.Client
{
    /// <summary>
    /// This enumeration represents the possible states of a persistent query.
    /// </summary>
    public enum PersistentQueryStatus
    {
		Uninitialized,
		Connecting,
		Authenticating,
		AcquiringWorker,
		Initializing,
		Running,
		Failed,
		Error,
		Disconnected,
		Stopped,
		Completed,
		Executing
	}
}
