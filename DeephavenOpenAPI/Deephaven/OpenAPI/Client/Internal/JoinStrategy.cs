/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

namespace Deephaven.OpenAPI.Client.Internal
{
    internal enum JoinStrategy
    {
        Default,
        Linear,
        UseExistingGroups,
        CreateGroups
    }
}
