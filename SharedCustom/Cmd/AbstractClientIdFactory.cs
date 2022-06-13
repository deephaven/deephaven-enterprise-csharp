/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Deephaven.OpenAPI.Shared.Cmd
{
    public abstract class AbstractClientIdFactory<T> : IdFactory<T> where T : ServerObjectHandle
    {
        private static int _nextId;

        protected readonly int _clientConnectionId;
        private int ServerConnectionId { get; }
        private readonly Dictionary<int, T> _pool = new Dictionary<int, T>();

        protected AbstractClientIdFactory(int clientConnectionId, int serverConnectionId)
        {
            _clientConnectionId = clientConnectionId;
            ServerConnectionId = serverConnectionId;
        }

        protected abstract T CreateHandle(int id);

        public override T GetOrCreateHandle(int id)
        {
            lock (this)
            {
                if (!_pool.TryGetValue(id, out var handle))
                {
                    handle = CreateHandle(id);
                    _pool[id] = handle;
                }
                return handle;
            }
        }

        public void DeleteHandle(T handle)
        {
            lock (this)
            {
                _pool.Remove(handle.GetClientId());
            }
        }

        public sealed override int GetReplyToId()
        {
            return ServerConnectionId;
        }

        public sealed override int NewId()
        {
            return Interlocked.Increment(ref _nextId);
        }

        public T[] GetPoolValues()
        {
            {
                lock (this)
                {
                    return _pool.Values.ToArray();
                }
            }
        }
    }
}
