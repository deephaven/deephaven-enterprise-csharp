/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
namespace Deephaven.OpenAPI.Client.Internal
{
    /// <summary>
    /// Deephaven internal operations -- clients should not use.
    /// </summary>
    public interface IQueryScopeInternal
    {
        ServerContext Context { get; }
        TableStateScope TableStateScope { get; }
    }
}
