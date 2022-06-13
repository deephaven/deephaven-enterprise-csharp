/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using Deephaven.OpenAPI.Shared.Ide;

namespace Deephaven.Connector.Internal.Parameters
{
    internal interface BindParameter
    {
        string GetLiteral(ConsoleSessionType sessionType);
    }
}
