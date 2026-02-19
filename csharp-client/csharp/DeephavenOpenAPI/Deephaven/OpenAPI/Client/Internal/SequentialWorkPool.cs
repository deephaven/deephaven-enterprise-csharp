/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Deephaven.OpenAPI.Shared.Worker;

namespace Deephaven.OpenAPI.Client.Internal
{
    /// <summary>
    /// Manages a pool of work that is done on a separate thread, but in order. In other words, work
    /// items that are added to this object are performed on a separate thread in the order that they
    /// were added.
    /// </summary>
    public class SequentialWorkPool
    {
        /// <summary>
        /// The queue of pending items, or null if there is no queue and no thread draining that queue.
        /// Guarded by this.
        /// </summary>
        private Queue<Action> _workQueue = null;

        /// <summary>
        /// Adds an Action to the work queue. The action must be well-behaved: finish relatively quickly so as not to
        /// starve other actions on the queue, and also to return normally (i.e. not throw). It's the responsibility of
        /// the action to catch and handle its own exceptions. If an exception escapes from the action, we will just
        /// drop it on the floor.
        /// </summary>
        /// <param name="action"></param>
        public void Add(Action action)
        {
            var startTask = false;
            lock (this)
            {
                if (_workQueue == null)
                {
                    _workQueue = new Queue<Action>();
                    startTask = true;
                }

                _workQueue.Enqueue(action);
            }

            if (startTask)
            {
                ThreadPool.QueueUserWorkItem(ProcessWorkQueue);
            }
        }

        /// <summary>
        /// This thread runs until the _workQueue is empty, at which point it exits.
        /// </summary>
        private void ProcessWorkQueue(object _)
        {
            while (true)
            {
                Action next;
                lock (this)
                {
                    if (_workQueue.Count == 0)
                    {
                        _workQueue = null;
                        break;
                    }

                    next = _workQueue.Dequeue();
                }

                try
                {
                    next();
                }
                catch (Exception e)
                {
                    // The actions on the work queue are obligated to catch and handle their own exceptions. The
                    // SequentialWorkPool has no facility for handling exceptions thrown by actions. If, despite all
                    // this, an exception escapes from the Action anyway, we have no choice but to ignore it.
                    Console.Error.WriteLine($"ProcessWorkQueue: Ignoring thrown exception: {e}");
                }
            }
        }
    }
}
