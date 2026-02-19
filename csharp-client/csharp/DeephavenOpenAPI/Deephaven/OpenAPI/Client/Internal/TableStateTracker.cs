/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Threading;

namespace Deephaven.OpenAPI.Client.Internal
{
    public sealed class TableStateTracker : IDisposable
    {
        private static long _nextFreeId;

        public long UniqueId { get; }
        private TableStateScope TableStateScope { get; }
        public TableState TableState { get; }
        private bool _disposed = false;

        public static TableStateTracker Create(TableStateScope scope, TableState state)
        {
            var id = Interlocked.Increment(ref _nextFreeId);
            var result = new TableStateTracker(id, scope, state);
            scope.AddTracker(result);
            state.AddTracker(result);
            return result;
        }

        private TableStateTracker(long id, TableStateScope scope, TableState state)
        {
            UniqueId = id;
            TableStateScope = scope;
            TableState = state;
        }

        ~TableStateTracker()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            lock (this)
            {
                if (_disposed)
                {
                    return;
                }
                _disposed = true;
            }
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Basically the standard .NET Dispose pattern.
        /// </summary>
        /// <param name="disposing">unused</param>
        private void Dispose(bool disposing)
        {
            TableStateScope.RemoveTracker(this);
            TableState.RemoveTracker(this);
        }

        public override string ToString()
        {
            return $"TableStateTracker({TableStateScope}, {TableState})";
        }
    }
}
