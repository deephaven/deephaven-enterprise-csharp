/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace Deephaven.OpenAPI.Shared.Cmd
{
    public abstract class ServerObjectHandle
    {
        public const int Uninitialized = -1;

        public abstract int GetClientId();
    }
}
