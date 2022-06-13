/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Deephaven.OpenAPI.Core.API;
using Deephaven.OpenAPI.Core.API.Util;
using Deephaven.OpenAPI.Shared.Cmd;
using Deephaven.OpenAPI.Shared.Data;
using Deephaven.OpenAPI.Shared.Ide;
using Deephaven.OpenAPI.Shared.Primary;
using Deephaven.OpenAPI.Shared.Worker;
using Org.BouncyCastle.Security;

namespace Deephaven.OpenAPI.Client.Internal
{
    internal class OpenApiClient : IOpenApiClient
    {
        /// <summary>
        /// Used to ensure that we can actually talk to the server. Has a null if the initial round-trip test command
        /// completed successfully; otherwise an error message. The error message is set either if
        /// GetAuthConfigValuesAsync fails (unlikely), or if the connection is closed (e.g. because the hash didn't match)
        /// </summary>
        private readonly IOpenApiListener _listener;

        private readonly ClientInternal _clientInternal;
        private readonly ServerWrapper<WebApiServer> _serverWrapper;

        private Dictionary<long, WeakReference<WorkerSession>> _workerSessions =
            new Dictionary<long, WeakReference<WorkerSession>>();

        private Timer _timer;
        private bool _disposed = false;

        private static int _nextClientId = 1;

        // This is a factory method because OpenApiClient has a finalizer, C# finalizers will still run on
        // partially-constructed objects even if the constructor throws an exception, and so I want a constructor
        // that doesn't throw.
        public static OpenApiClient Create(string host, int port = 8123, IOpenApiListener listener = null,
            int connectionTimeoutMillis = ClientConstants.DefaultConnectionTimeoutMillis)
        {
            // Canonicalize
            host = System.Net.Dns.GetHostEntry(host).HostName;

            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            var client = new ClientInternal(tcs, listener);
            var sb = ServerBuilder<WebApiServer, WebApiClient>.Of((factory, send, message, close) =>
                    new WebApiServer_Impl(factory, send, message, close))
                .SetUrl($"wss://{host}:{port}/socket");
            var server = sb.Start(client, connectionTimeoutMillis);

            // Send an initial command to see if we have successfully connected
            server.GetAuthConfigValuesAsync(
                success => tcs.TrySetResult(null),
                error => tcs.TrySetResult(error),
                error => tcs.TrySetResult(error));

            if (!tcs.Task.Wait(connectionTimeoutMillis))
            {
                throw new Exception(
                    $"Timeout: Failed to connect to API Server after {connectionTimeoutMillis} milliseconds");
            }

            var reason = tcs.Task.Result;
            if (reason != null)
            {
                throw new Exception($"Error: Failed to connect to API Server: {reason}");
            }

            var serverWrapper = new ServerWrapper<WebApiServer>(server);
            var result = new OpenApiClient(listener, client, serverWrapper);
            // So callbacks can refer to a specific object
            client.SetOpenApiClient(result);
            return result;
        }

        private OpenApiClient(IOpenApiListener listener, ClientInternal clientInternal,
            ServerWrapper<WebApiServer> serverWrapper)
        {
            _listener = listener;
            _clientInternal = clientInternal;
            _serverWrapper = serverWrapper;
        }

        ~OpenApiClient()
        {
            // I don't want to block the finalizer queue, so I'm going to dispatch this on my own thread.
            ThreadPool.QueueUserWorkItem(SynchronousDisposeHelper);
        }

        public async Task LoginTask(TextReader privKeyReader)
        {
            const string expectedAlgorithm = "SHA1withDSA";
            var dict = MakeDict(privKeyReader);

            string MustGet(string key)
            {
                if (!dict.TryGetValue(key, out var value))
                {
                    throw new Exception($"Private key file is missing an entry for {key}");
                }

                return value;
            }

            var user = MustGet("user");
            var operateAs = MustGet("operateas");
            var pubKeyBase64 = MustGet("public");
            var privKeyBase64 = MustGet("private");

            var pubKeyBytes = Convert.FromBase64String(pubKeyBase64);
            var privKeyBytes = Convert.FromBase64String(privKeyBase64);
            var privKey = PrivateKeyFactory.CreateKey(privKeyBytes);

            var crdTask = _serverWrapper.InvokeServerTask<ChallengeResponseData>((srv, sa, fa) =>
                srv.GetChallengeNonceAsync(sa, fa, fa));
            var crd = await crdTask;
            if (crd.Algorithm != expectedAlgorithm)
            {
                throw new Exception(
                    $"Server wants nonce to be signed with {crd.Algorithm}, but this code only supports {expectedAlgorithm}");
            }

            var nonceAsBytes = ArrayUtil.SignedBytesToUnsignedBytes(crd.Nonce);
            var signer = SignerUtilities.GetSigner(expectedAlgorithm);
            signer.Init(true, privKey);
            signer.BlockUpdate(nonceAsBytes, 0, nonceAsBytes.Length);

            // "Signed" as in "signed by the private key"
            var signedNonce = signer.GenerateSignature();
            // "Signed" as in "the byte has a sign bit"
            var signedNonceAsSignedBytes = ArrayUtil.UnsignedBytesToSignedBytes(signedNonce);

            var signedPubKeyBytes = ArrayUtil.UnsignedBytesToSignedBytes(pubKeyBytes);

            var crTask = _serverWrapper.InvokeServerTask<RefreshToken>((srv, sa, fa) =>
                srv.ChallengeResponseAsync(signedNonceAsSignedBytes, signedPubKeyBytes, user, operateAs, sa, fa, fa));
            var rt = crTask.Result;
            FinishLogin(rt);
        }

        public async Task LoginTask(string username, string password, string operateAs = null)
        {
            var rtTask = _serverWrapper.InvokeServerTask<RefreshToken>((srv, sa, fa) =>
                srv.LoginAsync(username, password, operateAs ?? username, sa, fa, fa));
            var rt = await rtTask;
            FinishLogin(rt);
        }

        private void FinishLogin(RefreshToken rt)
        {
            // start a timer to refresh our auth token every 5 minutes
            var weakSelf = new WeakReference<OpenApiClient>(this);
            _timer = new Timer(ScheduleAuthTokenRefresh, weakSelf, TimeSpan.FromSeconds(300),
                TimeSpan.FromSeconds(300));

            // notify any listeners of the auth token
            _listener?.OnAuthTokenRefresh(this, rt);
        }

        private static void ScheduleAuthTokenRefresh(object o)
        {
            var weakSelf = (WeakReference<OpenApiClient>) o;
            if (!weakSelf.TryGetTarget(out var self))
            {
                return;
            }

            self._serverWrapper.InvokeServer(srv =>
            {
                srv.RefreshAsync(
                    refreshToken => self._listener?.OnAuthTokenRefresh(self, refreshToken),
                    error => self._listener?.OnAuthTokenError(self, error),
                    error => self._listener?.OnAuthTokenError(self, error)
                );
            });
        }

        public Task<string[]> GetWorkerProfilesTask()
        {
            return _serverWrapper.InvokeServerTask<string[]>((srv, sa, fa) => srv.GetJvmProfilesAsync(sa, fa, fa));
        }

        public async Task<IWorkerSession> AttachWorkerByNameTask(string name, IWorkerListener workerListener,
            int connectionTimeoutMillis, int watchdogTimeoutMillis)
        {
            var config = _clientInternal.GetByName(name);
            if (config == null)
            {
                // If the name isn't found, it's possible this is because we are in a race with some pending
                // PersistentQueryAdded messages and the name would have been found if we had just waited a short while
                // longer. To account for this, we do a simple command (effectively a ping), wait for a response, and
                // hope by then we have caught up with all in-flight messages.
                _ = await _serverWrapper.InvokeServerTask<string>((srv, sa, fa) =>
                    srv.GetDefaultCalendarAsync(sa, fa, fa));
                config = _clientInternal.GetByName(name);

                if (config == null)
                {
                    throw new IndexOutOfRangeException($"No query found with the name \"{name}\"");
                }
            }

            return await Attach(config, workerListener, connectionTimeoutMillis, watchdogTimeoutMillis);
        }

        public async Task<IWorkerSession> AttachWorkerBySerialTask(long serial, IWorkerListener workerListener,
            int connectionTimeoutMillis, int watchdogTimeoutMillis)
        {
            var config = _clientInternal.GetBySerial(serial);
            if (config == null)
            {
                // If the serial isn't found, it's possible this is because we are in a race with some pending
                // PersistentQueryAdded messages and the serial would have been found if we had just waited a short while
                // longer. To account for this, we do a simple command (effectively a ping), wait for a response, and
                // hope by then we have caught up with all in-flight messages.
                _ = await _serverWrapper.InvokeServerTask<string>((srv, sa, fa) =>
                    srv.GetDefaultCalendarAsync(sa, fa, fa));
                config = _clientInternal.GetBySerial(serial);

                if (config == null)
                {
                    throw new IndexOutOfRangeException($"No query found with the serial number \"{serial}\"");
                }
            }

            return await Attach(config, workerListener, connectionTimeoutMillis, watchdogTimeoutMillis);
        }

        public async Task<IWorkerSession> StartWorkerTask(WorkerOptions options, IWorkerListener workerListener,
            int connectionTimeoutMillis, int watchdogTimeoutMillis)
        {
            var requestId = new RequestId(Interlocked.Increment(ref _nextClientId));

            var ca = await _serverWrapper.InvokeServerTask<ConsoleAddress>((srv, sa, fa) =>
                srv.StartWorkerAsync(requestId, options.ConsoleConfig, sa, fa, fa));

            return await InitializeSession(ca.WebsocketUrl, connectionTimeoutMillis, requestId.ClientId, ca.ServiceId,
                watchdogTimeoutMillis, requestId, workerListener);
        }

        public IPersistentQueryConfig GetPersistentQueryConfig(long name)
        {
            return _clientInternal.GetQueryConfig(name);
        }

        public Dictionary<long, IPersistentQueryConfig> GetPersistentQueryConfigs()
        {
            return _clientInternal.GetQueryConfigs();
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

            SynchronousDisposeHelper(null);
            GC.SuppressFinalize(this);
        }

        private void SynchronousDisposeHelper(object _)
        {
            WorkerSession[] sessions;
            lock (this)
            {
                sessions = _workerSessions.Values
                    .Select(wr => wr.TryGetTarget(out var session) ? session : null)
                    .Where(s => s != null)
                    .ToArray();
                _workerSessions = new Dictionary<long, WeakReference<WorkerSession>>();
            }

            _timer?.Dispose();

            foreach (var session in sessions)
            {
                session.Dispose();
            }

            var tcs = new TaskCompletionSource<bool>();
            // Post a job to dispose the server
            _serverWrapper.InvokeServer(s => s.Dispose());
            // Post a job to set the task completion source
            _serverWrapper.InvokeServer(s => tcs.SetResult(true));
            tcs.Task.Wait();
        }

        private async Task<IWorkerSession> Attach(IPersistentQueryConfig config, IWorkerListener workerListener,
            int connectionTimeoutMillis, int watchdogTimeoutMillis)
        {
            if (config.Status != PersistentQueryStatus.Running)
            {
                throw new Exception($"Attach failed because Persistent Query is in state {config.Status}. " +
                                    $"Expected {nameof(PersistentQueryStatus.Running)}");
            }

            var clientId = Interlocked.Increment(ref _nextClientId);
            return await InitializeSession(config.Internal.WebsocketUrl, connectionTimeoutMillis,
                clientId, config.ServiceId, watchdogTimeoutMillis, null, workerListener);
        }

        private const string RemoteQueryProcessorService = "RemoteQueryProcessor";

        private async Task<IWorkerSession> InitializeSession(string websocketUrl, int connectionTimeoutMillis,
            int clientConnectionId, string serviceId, int watchdogTimeoutMillis,
            RequestId requestIdForStoppingWorkerOnDispose, IWorkerListener workerListener)
        {
            var connectToken = await _serverWrapper.InvokeServerTask<ConnectToken>((srv, sa, fa) =>
                srv.CreateAuthTokenAsync(RemoteQueryProcessorService, sa, fa, fa));

            // Once the worker has been created, we need to start the server and register it.
            var builder = ServerBuilder<WorkerServer, WorkerClient>.Of(
                    (factory, send, message, close) => new WorkerServer_Impl(factory, send, message, close))
                .SetUrl(websocketUrl);

            var tcs = new TaskCompletionSource<ClientTableIdFactory>(TaskCreationOptions
                .RunContinuationsAsynchronously);
            var workerClient = new WorkerClientImpl(workerListener);
            var workerServer = builder.Start(workerClient, connectionTimeoutMillis);

            // TODO(kosak): Now or UtcNow?
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            workerServer.RegisterAsync(clientConnectionId, serviceId, now, connectToken,
                success =>
                {
                    try
                    {
                        var factory = new ClientTableIdFactory(clientConnectionId, success.ConnectionId);
                        tcs.SetResult(factory);
                    }
                    catch (Exception e)
                    {
                        // Exceptions within this success handler should pass back up instead of being swallowed
                        tcs.SetException(e);
                    }
                },
                failure => tcs.SetException(new Exception($"Worker connection failure: {failure}")),
                failure => tcs.SetException(new Exception($"Worker connection failure: {failure}")));

            if (!tcs.Task.Wait(connectionTimeoutMillis))
            {
                throw new Exception(
                    $"Timeout: Failed to connect to worker after {connectionTimeoutMillis} milliseconds");
            }

            var result = tcs.Task.Result;
            var context = new ServerContext(workerServer, workerClient, result);
            Action<WorkerSession> onDisposed = sess =>
            {
                lock (this)
                {
                    if (!_workerSessions.Remove(sess.UniqueId))
                    {
                        return;
                    }
                }

                var disposeMessageSent = new TaskCompletionSource<bool>();
                _serverWrapper.InvokeServer(s => s.StopWorker(requestIdForStoppingWorkerOnDispose));
                _serverWrapper.InvokeServer(s => disposeMessageSent.SetResult(true));
                disposeMessageSent.Task.Wait();
            };
            var session = new WorkerSession(context, websocketUrl, serviceId, onDisposed);
            // so that callbacks will know which session they came from
            workerClient.SetWorkerSession(session);
            lock (this)
            {
                _workerSessions.Add(session.UniqueId, new WeakReference<WorkerSession>(session));
            }

            // Start the heartbeat loop.
            session.ConfigureWatchdog(watchdogTimeoutMillis);
            return session;
        }

        private static Dictionary<string, string> MakeDict(TextReader reader)
        {
            var result = new Dictionary<string, string>();
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }

                line = line.Trim();
                if (line.Length == 0 || line[0] == '#')
                {
                    continue;
                }

                var entry = line.Split(' ');
                if (entry.Length != 2)
                {
                    throw new Exception($"Unexpected line in key file: {line}");
                }

                result.Add(entry[0], entry[1]);
            }

            return result;
        }
    }

    /// <summary>
    /// This implementation receives incoming messages from the Open API
    /// associated with system level events.
    /// </summary>
    internal class ClientInternal : WebApiClient, IQueryConfigProvider
    {
        /// <summary>
        /// We use this object to report an early cancel to our creator
        /// </summary>
        private readonly TaskCompletionSource<string> _tcs;

        /// <summary>
        /// A map to keep track of running persistent queries
        /// </summary>
        private readonly ConcurrentDictionary<long, PersistentQueryConfig> _queryConfigMap =
            new ConcurrentDictionary<long, PersistentQueryConfig>();

        /// <summary>
        /// A reference to the server this client is paired with.
        /// </summary>
        private WebApiServer _server;

        private OpenApiClient _openApiClient;

        private readonly IOpenApiListener _openApiListener;

        public ClientInternal(TaskCompletionSource<string> tcs, IOpenApiListener openApiListener)
        {
            _tcs = tcs;
            _openApiListener = openApiListener;
        }

        public void SetOpenApiClient(OpenApiClient newClient)
        {
            _openApiClient = newClient;
        }

        public PersistentQueryConfig GetQueryConfig(long name)
        {
            return _queryConfigMap.TryGetValue(name, out var queryConfig) ? queryConfig : null;
        }

        public Dictionary<long, IPersistentQueryConfig> GetQueryConfigs()
        {
            return _queryConfigMap.ToDictionary(kv => kv.Key, kv => (IPersistentQueryConfig) kv.Value);
        }

        /// <summary>
        /// Called when a console dies
        /// </summary>
        /// <param name="id"></param>
        public void ConsoleDeath(RequestId id)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public WebApiServer GetServer()
        {
            return _server;
        }

        /// <summary>
        /// Called when the connection to the server is lost for any reason.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="reason"></param>
        public void OnClose(ushort code, string reason)
        {
            _tcs.TrySetResult(reason);
            _openApiListener?.OnClosed(_openApiClient, code, reason);
        }

        /// <summary>
        /// Called when an error occurs
        /// </summary>
        /// <param name="ex"></param>
        public void OnError(Exception ex)
        {
            _openApiListener?.OnError(_openApiClient, ex);
        }

        /// <summary>
        /// Called when the connection to the open API server is opened.
        /// </summary>
        public void OnOpen()
        {
            _openApiListener?.OnOpen(_openApiClient);
        }

        /// <summary>
        /// Called by the server when a new persistent query is added.
        /// </summary>
        /// <param name="config">Configuration information for the new persistent query</param>
        public void QueryAdded(QueryConfig config)
        {
            var workerInfo = new PersistentQueryConfig(config);
            _queryConfigMap[config.Serial] = workerInfo;
            _openApiListener?.OnPersistentQueryAdded(_openApiClient, workerInfo);
        }

        /// <summary>
        /// Called by the server when an existing persistent query is modified.
        /// </summary>
        /// <param name="config">Configuration information for the modified persistent query</param>
        public void QueryModified(QueryConfig config)
        {
            var workerInfo = new PersistentQueryConfig(config);
            _queryConfigMap[config.Serial] = workerInfo;
            _openApiListener?.OnPersistentQueryModified(_openApiClient, workerInfo);
        }

        /// <summary>
        /// Called by the server when a persistent query is removed.
        /// </summary>
        /// <param name="config">Configuration information for the removed persistent query</param>
        public void QueryRemoved(QueryConfig config)
        {
            // if for some reason we didn't have the config in the first place, we do nothing
            if (_queryConfigMap.TryRemove(config.Serial, out var workerInfo))
            {
                _openApiListener?.OnPersistentQueryRemoved(_openApiClient, workerInfo);
            }
        }

        public void Ids6976(char[] bugMitigation1, sbyte[] bugMitigation2)
        {
        }

        public void SetServer(WebApiServer server)
        {
            _server = server;
        }

        public IPersistentQueryConfig GetByName(string name)
        {
            foreach (var entry in _queryConfigMap)
            {
                if (entry.Value.Name.Equals(name))
                {
                    return entry.Value;
                }
            }

            return null;
        }

        public IPersistentQueryConfig GetBySerial(long serial)
        {
            return _queryConfigMap.TryGetValue(serial, out var config) ? config : null;
        }
    }
}