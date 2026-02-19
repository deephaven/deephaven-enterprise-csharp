/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

namespace Deephaven.OpenAPI.Shared.Cmd
{
    public abstract class IdFactory<THandle>
    {
        public abstract int NewId();
        public abstract int GetReplyToId();
        public abstract THandle GetOrCreateHandle(int i);
        public THandle NewHandle()
        {
            return GetOrCreateHandle(NewId());
        }
    }
}
