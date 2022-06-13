using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading;

namespace Deephaven.OpenAPI.Core.API.Util
{
    /// <summary>
    /// This class exists to work around a bug we believe exists in older releases of Windows like Server 2012R2 that
    /// causes websocket connections to spuriously fail on connection.
    /// See https://github.com/dotnet/runtime/issues/17005#issuecomment-305848835
    /// Unfortunately we have not been able to repro this in our testing, although our customer is seeing it.
    ///
    /// It also provides a fire-and-forget API for sending and a callback functionality for receiving.
    ///
    /// Errors at connection time are communicated via an exception on the Connect method.
    /// Later, errors are communicated via the onError callback, which will be called exactly once, and then this object
    /// will permanently enter the 'Error' state. In this state, all future Send operations will throw an exception
    /// and no more messages will be received.
    ///
    /// If the connection is closed (say, by the server), the onClosed callback will be called exactly once, and then
    /// this object will permanently enter the 'Closed' state. In this state, all future Send operations will throw an
    /// exception and no more messages will be received.
    /// </summary>
    public class WebSocketWrapper
    {
        /// <summary>
        /// In order to work around the Windows bug above, We try the connection multiple times, with a delay of
        /// {1, 2, 4, 8, 16, ...} seconds between each time, on the assumption that spurious errors sometimes happen.
        /// Note that there is no particular upper limit on this sequence; however the loop will end when
        /// 'timeoutMillis' time has elapsed.
        /// </summary>
        /// <param name="url">WebSocketURL</param>
        /// <param name="timeoutMillis">Total time to wait for a successful conection</param>
        /// <param name="onMessage">Callback for successfully received messages</param>
        /// <param name="onClose">Callback if the WebSocket is closed (deliberately or unexpectedly)</param>
        /// <param name="onError">Callback if the WebSocket has an error</param>
        /// <returns>The WebSocketWrapper object</returns>
        public static WebSocketWrapper Connect(string url, int timeoutMillis,
            Action<MemoryStream> onMessage, Action<ushort, string> onClose, Action<Exception> onError)
        {
            var uri = new Uri(url);
            var expiration = DateTime.Now + TimeSpan.FromMilliseconds(timeoutMillis);
            var nextDelaySecs = 1;
            var innerExceptions = new List<Exception>();
            while (true)
            {
                var now = DateTime.Now;
                if (expiration < now)
                {
                    expiration = now;
                }

                var delay = expiration - now;
                var ws = new ClientWebSocket();
                var cts = new CancellationTokenSource();
                var connTask = ws.ConnectAsync(uri, cts.Token);
                try
                {
                    if (connTask.Wait(delay))
                    {
                        return new WebSocketWrapper(ws, onMessage, onClose, onError);
                    }
                    cts.Cancel();
                    break;
                }
                catch (AggregateException ae)
                {
                    innerExceptions.AddRange(ae.InnerExceptions);
                    Thread.Sleep(nextDelaySecs * 1000);
                    nextDelaySecs *= 2;
                }
            }
            if (innerExceptions.Count == 0)
            {
                throw new Exception($"Timed out after {timeoutMillis} ms: failed to connect to {url}");
            }
            throw new AggregateException($"Retry logic failed after {timeoutMillis} ms: failed to connect to {url}",
                innerExceptions);
        }

        private const int RX_BUFFER_SIZE = 65536;

        private readonly ClientWebSocket _ws;
        private readonly Action<MemoryStream> _onMessage;
        private readonly Action<ushort, string> _onClose;
        private readonly Action<Exception> _onError;
        private readonly CancellationTokenSource _cts;
        private bool _isOpen;
        private int _numActiveThreads;

        /// <summary>
        /// The queue of messages we want to transmit. As a special case, a null item on the queue means
        /// the client wants to Close the WebSocket.
        /// </summary>
        private readonly Queue<byte[]> _txQueue;

        /// <summary>
        /// The reusable receive buffer that we repeatedly give to the ReceiveAsync call.
        /// </summary>
        private readonly byte[] _rxBuffer;

        /// <summary>
        /// The partial receive message being built.
        /// </summary>
        private readonly BufferBuilder _rxBuilder;

        /// <summary>
        /// The private constructor.
        /// </summary>
        /// <param name="ws">The connected WebSocket, created by our static factory method</param>
        /// <param name="onMessage">Callback for successfully received messages</param>
        /// <param name="onClose">Callback if the WebSocket is closed (deliberately or unexpectedly)</param>
        /// <param name="onError">Callback if the WebSocket has an error</param>
        /// <returns>The WebSocketWrapper object</returns>
        private WebSocketWrapper(ClientWebSocket ws, Action<MemoryStream> onMessage,
            Action<ushort, string> onClose, Action<Exception> onError)
        {
            _ws = ws;
            _onMessage = onMessage;
            _onClose = onClose;
            _onError = onError;
            _cts = new CancellationTokenSource();
            _isOpen = true;
            _txQueue = new Queue<byte[]>();
            _rxBuffer = new byte[RX_BUFFER_SIZE];
            _rxBuilder = new BufferBuilder();
            var rxThread = new Thread(() => MainThreadLoop(RxMessageHandler)) {IsBackground = true};
            var txThread = new Thread(() => MainThreadLoop(TxMessageHandler)) {IsBackground = true};
            rxThread.Start();
            txThread.Start();
        }

        public void Send(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentException("data was null");
            }

            SendHelper(data);
        }

        public void Close()
        {
            SendHelper(null);
        }

        private void SendHelper(byte[] data)
        {
            lock (this)
            {
                if (!_isOpen)
                {
                    throw new Exception("Can't Send because socket is no longer open");
                }

                _txQueue.Enqueue(data);
                if (_txQueue.Count == 1)
                {
                    Monitor.Pulse(this);
                }
            }
        }

        private void MainThreadLoop(Func<bool> handler)
        {
            Interlocked.Increment(ref _numActiveThreads);
            try
            {
                while (true)
                {
                    if (!handler())
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionHappened(e);
            }
            // Last one out disposes!
            var result = Interlocked.Decrement(ref _numActiveThreads);
            if (result == 0)
            {
                _ws.Dispose();
            }
        }

        private bool TxMessageHandler()
        {
            byte[] nextItem;
            while (true)
            {
                lock (this)
                {
                    if (!_isOpen)
                    {
                        // Shut down TX thread
                        return false;
                    }

                    if (_txQueue.Count != 0)
                    {
                        nextItem = _txQueue.Dequeue();
                        break;
                    }

                    Monitor.Wait(this);
                }
            }

            if (nextItem == null)
            {
                lock (this)
                {
                    _isOpen = false;
                }

                // Special flag for "Close" message
                var closeTask = _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client",
                    CancellationToken.None);
                closeTask.Wait();
                return false;
            }

            var aseg = new ArraySegment<byte>(nextItem);
            var sendTask = _ws.SendAsync(aseg, WebSocketMessageType.Binary, true, CancellationToken.None);
            sendTask.Wait();
            return true;
        }

        private bool RxMessageHandler()
        {
            var aseg = new ArraySegment<byte>(_rxBuffer);
            var rxTask = _ws.ReceiveAsync(aseg, _cts.Token);
            var result = rxTask.Result;
            if (result.CloseStatus.HasValue)
            {
                CloseHappened(result.CloseStatus.Value, result.CloseStatusDescription);
                return false;
            }
            _rxBuilder.Append(_rxBuffer, result.Count, result.EndOfMessage);

            if (!result.EndOfMessage)
            {
                return true;
            }

            var ms = _rxBuilder.ReleaseBuffer();
            try
            {
                _onMessage(ms);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Shouldn't happen: ignoring exception from callback: {e}");
            }
            return true;
        }

        private void ExceptionHappened(Exception e)
        {
            lock (this)
            {
                if (!_isOpen)
                {
                    return;
                }

                _isOpen = false;
            }
            _cts.Cancel();
            _onError(e);
        }

        private void CloseHappened(WebSocketCloseStatus cs, string description)
        {
            lock (this)
            {
                if (!_isOpen)
                {
                    return;
                }

                _isOpen = false;
            }
            _cts.Cancel();
            _onClose((ushort)cs, description);
        }
    }

    /// <summary>
    /// This class is similar to every other capacity-doubling *Builder or List&lt;T&gt; class in the
    /// world. The main differences are that you can ask it to release its underlying buffer back to you
    /// (saving a copy), and that you can give it a hint that you are doing the last Append of a set,
    /// so that it doesn't double the capacity of the underlying storage in that case.
    /// </summary>
    internal class BufferBuilder
    {
        private byte[] _buffer;
        private int _bufferSize;

        public void Append(byte[] src, int srcSize, bool lastAppend)
        {
            if (srcSize == 0)
            {
                return;
            }

            EnsureCapacity(_bufferSize + srcSize, lastAppend);
            Array.Copy(src, 0, _buffer, _bufferSize, srcSize);
            _bufferSize += srcSize;
        }

        public MemoryStream ReleaseBuffer()
        {
            var result = new MemoryStream(_buffer, 0, _bufferSize);
            _buffer = null;
            _bufferSize = 0;
            return result;
        }

        private void EnsureCapacity(int desiredCapacity, bool lastAppend)
        {
            var actualCapacity = _buffer?.Length ?? 0;
            if (actualCapacity >= desiredCapacity)
            {
                return;
            }

            var finalCapacity = lastAppend ? desiredCapacity : 1 << (1 + (int) Math.Log(desiredCapacity, 2));

            var newBuffer = new byte[finalCapacity];
            if (_buffer != null)
            {
                Array.Copy(_buffer, 0, newBuffer, 0, _bufferSize);
            }
            _buffer = newBuffer;
        }
    }
}
