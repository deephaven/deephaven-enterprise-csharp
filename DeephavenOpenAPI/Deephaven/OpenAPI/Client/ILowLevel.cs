/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client
{
    public interface ILowLevel
    {
        InitialTableDefinition InitialTableDefinition
        {
            get;
        }
    }
}
