/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Deephaven.OpenAPI.Core.API.Util;
using Deephaven.OpenAPI.Core.RPC.Serialization.API;
using Deephaven.OpenAPI.Core.RPC.Serialization.Stream.Binary;

namespace Deephaven.OpenAPI.Core.API.Impl
{
    /// <summary>
    /// Base class for implementing any kind of endpoint, simplifying the code
    /// required for a generated, concrete implementation.
    /// </summary>
    public abstract class AbstractEndpointImpl
    {
        private const int MinCallbackId = 1;
        private const int MaxCallbackId = int.MaxValue;

        private readonly Func<ITypeSerializer, BinarySerializationStreamWriter> _writerFactory;
        private readonly Action<BinarySerializationStreamWriter> _send;
        private readonly ITypeSerializer _typeSerializer;

        private static int _nextCallbackId = 1;
        private readonly Dictionary<int, IResponseCallback> _callbacks =
            new Dictionary<int, IResponseCallback>();

        // These support the watchdog feature.
        private bool _watchdogExpired;
        private long _watchdogTimeoutMillis;
        private readonly Timer _watchdogTimer;

        protected AbstractEndpointImpl(
            Func<ITypeSerializer, BinarySerializationStreamWriter> writerFactory,
            Action<BinarySerializationStreamWriter> send,
            ITypeSerializer typeSerializer,
            Action<Action<ISerializationStreamReader>, ITypeSerializer> onMessage)
        {
            _writerFactory = writerFactory;
            _send = send;
            _typeSerializer = typeSerializer;
            _watchdogTimer = new Timer(WatchdogExpired);
            onMessage(__OnMessage, typeSerializer);
        }

        /// <summary>
        /// Push any errors to the local (non-generated) endpoint implementation.
        /// </summary>
        /// <param name="ex">The error exception</param>
        protected abstract void __OnError(Exception ex);

        /// <summary>
        /// Invoke the remove invoked method on the local (non-generated) endpoint implementation.
        /// </summary>
        /// <param name="recipient">Identifies the method being invoked</param>
        /// <param name="reader">Stream from which to read method parameters</param>
        protected abstract void __Invoke(int recipient, ISerializationStreamReader reader);

        private void __OnMessage(ISerializationStreamReader reader)
        {
            try
            {
                var recipient = reader.ReadInt32();
                if (recipient >= 0)
                {
                    __Invoke(recipient, reader);
                }
                else
                {
                    IResponseCallback callback;
                    lock (this)
                    {
                        if (_callbacks.TryGetValue(-recipient, out callback))
                        {
                            _callbacks.Remove(-recipient);
                        }
                    }

                    callback?.OnResponse(reader);
                }
            }
            catch (Exception e)
            {
                __OnError(e);
            }
        }

        private BinarySerializationStreamWriter __StartCall()
        {
            return _writerFactory(_typeSerializer);
        }

        private void __EndCall(BinarySerializationStreamWriter writer)
        {
            _send(writer);
        }

        protected void __Send(int recipient, Action<BinarySerializationStreamWriter> s)
        {
            CheckWatchdog("__Send");
            BinarySerializationStreamWriter writer = __StartCall();
            try
            {
                writer.Write(recipient);
                s(writer);
                __EndCall(writer);
            }
            catch (Exception e)
            {
                __OnError(e);
                throw;
            }
        }

        protected void __Send(int recipient, Action<BinarySerializationStreamWriter> s, IResponseCallback callback)
        {
            CheckWatchdog("__Send");
            int callbackId;
            lock (this)
            {
                callbackId = _nextCallbackId;
                _nextCallbackId = _nextCallbackId < MaxCallbackId ? _nextCallbackId + 1 : MinCallbackId;
                _callbacks[callbackId] = callback;
            }

            try
            {
                var writer = __StartCall();
                writer.Write(recipient);
                writer.Write(callbackId);
                s(writer);
                __EndCall(writer);
            }
            catch (SerializationException e)
            {
                lock (this)
                {
                    // if the send fails, remove the callback again and propagate the exception
                    _callbacks.Remove(callbackId);
                }
                callback.OnError(e.Message);
                __OnError(e);
            }
        }

        /// <summary>
        /// Start the connection watchdog with the specified timeout in milliseconds.  The watchdog
        /// must be fed at least once within each <paramref name="timeoutMillis"/> or existing operations
        /// will be aborted and errors thrown.
        /// </summary>
        /// <param name="timeoutMillis">The timeout in milliseconds for the watchdog</param>
        /// <exception cref="ArgumentException">If timeoutMillis is not positive</exception>
        public void StartWatchdog(long timeoutMillis)
        {
            if (timeoutMillis <= 0)
            {
                throw new ArgumentException("timeoutMillis must be positive.");
            }

            lock (this)
            {
                _watchdogTimeoutMillis = timeoutMillis;
            }
            FeedWatchdog();
        }

        /// <summary>
        /// Feed the watchdog.  If the watchdog has not been started via <see cref="StartWatchdog"/>
        /// then this has no effect.
        /// </summary>
        public void FeedWatchdog()
        {
            lock (this)
            {
                if (_watchdogTimeoutMillis <= 0) return;

                // Reset the watchdog timer
                _watchdogExpired = false;
                _watchdogTimer.Change(_watchdogTimeoutMillis, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Invoked when the watchdog expires.  This terminates all existing ops with an error
        /// and propagates the timeout error upwards for the implementor to handle.
        /// </summary>
        private void WatchdogExpired(object unusedState)
        {
            List<IResponseCallback> localCallbacks;
            lock (this)
            {
                _watchdogExpired = true;
                localCallbacks = _callbacks.Values.ToList();
                _callbacks.Clear();
            }

            foreach (var callback in localCallbacks)
            {
                callback.OnError("Watchdog Timed Out.");
            }

            try
            {
                __OnError(new WatchdogException("Watchdog Timed out."));
            }
            catch (Exception ex)
            {
                Console.WriteLine("__OnError Exception: " + ex);
            }
        }

        /// <summary>
        /// Ensure that the watchdog timer has not expired.  Throw an exception if it has.
        /// </summary>
        /// <param name="operation">A string to include with the exception</param>
        /// <exception cref="Exception">If the watchdog has expired</exception>
        private void CheckWatchdog(string operation)
        {
            lock (this)
            {
                if (_watchdogExpired)
                {
                    throw new WatchdogException($"cannot invoke '{operation}', watchdog has expired.");
                }
            }
        }

        /// <summary>
        /// A callback object that explicitly handles Responses,  which may be succesful, or not,
        /// as well as an error, which can occur if the watchdog times out.
        /// </summary>
        protected interface IResponseCallback
        {
            /// <summary>
            /// Invoked whenever the server responds to an async operation.  Note that it is the
            /// responsibility of the caller to determine if the operation succeeded or failed.
            /// </summary>
            /// <param name="reader">a reader to deserialize the server response</param>
            void OnResponse(ISerializationStreamReader reader);

            /// <summary>
            /// Invoked whenever there was an error in communication with the server.  This could be
            /// a broken socket, or a Watchdog timeout.
            /// </summary>
            /// <param name="message">a message describing the error.</param>
            void OnError(string message);
        }

        /// <summary>
        /// A basic implementation of the boilerplate required for an <see cref="IResponseCallback"/>
        /// </summary>
        /// <typeparam name="TS">The type of object expected on success</typeparam>
        /// <typeparam name="TF">The type of object expected on failure</typeparam>
        public abstract class AbstractResponseCallback<TS, TF> : IResponseCallback
        {
            protected readonly Func<ISerializationStreamReader, TS> SuccessReader;
            protected readonly Func<ISerializationStreamReader, TF> FailureReader;

            public abstract void OnResponse(ISerializationStreamReader reader);
            public abstract void OnError(string message);

            protected AbstractResponseCallback(
                Func<ISerializationStreamReader, TS> successReader,
                Func<ISerializationStreamReader, TF> failureReader)
            {
                SuccessReader = successReader;
                FailureReader = failureReader;
            }
        }

        public class AsyncCallback<TS, TF> : AbstractResponseCallback<TS, TF>
        {
            protected readonly Action<TS> SuccessCallback;
            protected readonly Action<TF> FailureCallback;
            protected readonly Action<string> ErrorCallback;

            public AsyncCallback(
                Func<ISerializationStreamReader, TS> successReader,
                Func<ISerializationStreamReader, TF> failureReader,
                Action<TS> successCallback,
                Action<TF> failureCallback,
                Action<string> errorCallback) : base(successReader, failureReader)
            {
                SuccessCallback = successCallback;
                FailureCallback = failureCallback;
                ErrorCallback = errorCallback;
            }

            public override void OnResponse(ISerializationStreamReader reader)
            {
                var success = reader.ReadBoolean();
                try
                {
                    if (success)
                    {
                        SuccessCallback(SuccessReader(reader));
                    }
                    else
                    {
                        FailureCallback(FailureReader(reader));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("OnResponse Exception: " + ex);
                }
            }

            public override void OnError(string message)
            {
                try
                {
                    ErrorCallback(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("OnError Exception: " + ex);
                }
            }
        }

        public class BlockingCallback<TS, TF> : AbstractResponseCallback<TS, TF>
        {
            private TS _success;
            private TF _failure;
            private bool _failed;
            private Exception _readException;
            private readonly AutoResetEvent _callbackReceiveEvent = new AutoResetEvent(false);
            private readonly int _timeoutMs;

            public BlockingCallback(
                Func<ISerializationStreamReader, TS> successReader,
                Func<ISerializationStreamReader, TF> failureReader,
                int timeoutMs) :
                base(successReader, failureReader)
            {
                _timeoutMs = timeoutMs;
            }

            public override void OnResponse(ISerializationStreamReader reader)
            {
                try
                {
                    if (reader.ReadBoolean())
                    {
                        _success = SuccessReader(reader);
                    }
                    else
                    {
                        _failed = true;
                        _failure = FailureReader(reader);
                    }
                }
                catch (Exception ex)
                {
                    _failed = true;
                    _readException = ex;
                }

                try
                {
                    _callbackReceiveEvent.Set();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("OnResponse.Set() Exception: " + ex);
                }
            }

            public override void OnError(string message)
            {
                _failed = true;
                _readException = new Exception($"An error occured: {message}");
                try
                {
                    _callbackReceiveEvent.Set();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("OnError.Set() Exception: " + ex);
                }
            }

            public TS Wait()
            {
                if (!_callbackReceiveEvent.WaitOne(_timeoutMs))
                {
                    throw new Exception($"RPC call timeout, no response received within {_timeoutMs}");
                }

                if (_failed)
                {
                    if (_readException != null)
                    {
                        throw _readException;
                    }

                    // note this assumes the error object is a string or has a sane ToString
                    throw new Exception(_failure == null
                        ? "Unknown server error"
                        : $"Server returned an error: {_failure}");
                }

                return _success;
            }
        }

        public abstract class MessageSender<TS>
        {
            protected TS _s;

            protected MessageSender(TS s)
            {
                _s = s;
            }

            public abstract void Send(ISerializationStreamWriter activeWriter);
        }

        public abstract class MessageSenderWithCallback<TS, TSuccess, TFailure> : MessageSender<TS>
        {
            protected MessageSenderWithCallback(TS s) : base(s)
            {
            }

            public abstract TSuccess ReadSuccess(ISerializationStreamReader reader);
            public abstract TFailure ReadFailure(ISerializationStreamReader reader);
        }
    }
}