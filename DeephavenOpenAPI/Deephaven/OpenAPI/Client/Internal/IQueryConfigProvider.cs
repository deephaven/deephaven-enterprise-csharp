/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Client;

namespace Deephaven.OpenAPI.Client.Internal
{
    internal interface IQueryConfigProvider
    {
        IPersistentQueryConfig GetByName(string name);
        IPersistentQueryConfig GetBySerial(long serial);
    }
}
