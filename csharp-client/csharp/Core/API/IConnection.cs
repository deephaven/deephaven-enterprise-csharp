/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;

namespace Deephaven.OpenAPI.Core.API
{
    public interface IConnection : IDisposable
    {
        void Data(string key, object value);
        object Data(string key);
    }
}
