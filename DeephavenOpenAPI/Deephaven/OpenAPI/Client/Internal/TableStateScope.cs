/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;

namespace Deephaven.OpenAPI.Client.Internal
{
    public class TableStateScope : IDisposable
    {
        private static long _nextFreeId;
        private readonly long _uniqueId;
        private bool _isDisposed = false;

        private Dictionary<long, WeakReference<TableStateTracker>> _trackers =
            new Dictionary<long, WeakReference<TableStateTracker>>();

        public TableStateScope()
        {
            _uniqueId = Interlocked.Increment(ref _nextFreeId);
        }

        public void Dispose()
        {
            Dictionary<long, WeakReference<TableStateTracker>> localTrackers;
            lock (this)
            {
                if (_isDisposed)
                {
                    return;
                }
                _isDisposed = true;
                localTrackers = _trackers;
                _trackers = null;
            }

            foreach (var wrTrack in localTrackers.Values)
            {
                if (wrTrack.TryGetTarget(out var tracker))
                {
                    tracker.Dispose();
                }
            }
        }

        internal void AddTracker(TableStateTracker tracker)
        {
            lock (this)
            {
                if (_isDisposed)
                {
                    return;
                }
                _trackers.Add(tracker.UniqueId, new WeakReference<TableStateTracker>(tracker));
            }
        }

        internal void RemoveTracker(TableStateTracker tracker)
        {
            lock (this)
            {
                if (_isDisposed)
                {
                    return;
                }
                _trackers.Remove(tracker.UniqueId);
            }
        }

        public override string ToString()
        {
            return $"TableStateScope {_uniqueId}";
        }
    }
}
