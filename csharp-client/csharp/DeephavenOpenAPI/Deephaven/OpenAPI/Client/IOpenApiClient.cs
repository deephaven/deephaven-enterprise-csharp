/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Deephaven.OpenAPI.Core.API.Util;

namespace Deephaven.OpenAPI.Client
{
    public static class ClientConstants
    {
        /// <summary>
        /// Connection timeout for both servers and workers.
        /// </summary>
        public const int DefaultConnectionTimeoutMillis = 64 * 1000;

        /// <summary>
        /// Timeout in millisecond after which the connection watchdog expires.
        /// </summary>
        public const int DefaultWatchdogTimeoutMillis = 30 * 1000;
    }

    /// <summary>
    /// An interface representing an Open API session, from which you can login,
    /// access workers and persistent queries.
    /// </summary>
    public interface IOpenApiClient : IDisposable
    {
        Task LoginTask(string username, string password, string operateAs = null);
        Task LoginTask(TextReader privKeyReader);

        /// <summary>
        /// Provides a list of Worker JVM profiles, that can be used in the
        /// <see cref="WorkerOptions.JvmProfile"/> argument when starting a
        /// new worker.
        /// </summary>
        /// <returns></returns>
        Task<string[]> GetWorkerProfilesTask();

        /// <summary>
        /// Attach a new worker session to the given Persistent Query
        /// by name.
        /// </summary>
        /// <param name="name">Worker name</param>
        /// <param name="workerListener">Optional listener for connection-level events</param>
        /// <param name="connectionTimeoutMillis">Worker connection timeout in milliseconds</param>
        /// <param name="watchdogTimeoutMillis">The Connection watchdog timeout in milliseconds</param>
        /// <returns></returns>
        Task<IWorkerSession> AttachWorkerByNameTask(string name,
            IWorkerListener workerListener = null,
            int connectionTimeoutMillis = ClientConstants.DefaultConnectionTimeoutMillis,
            int watchdogTimeoutMillis = ClientConstants.DefaultWatchdogTimeoutMillis);

        /// <summary>
        /// Attach a new worker session to the given Persistent Query
        /// by serial number.
        /// </summary>
        /// <param name="serial">Worker serial</param>
        /// <param name="workerListener">Optional listener for connection-level events</param>
        /// <param name="connectionTimeoutMillis">Worker connection timeout in milliseconds</param>
        /// <param name="watchdogTimeoutMillis">The Connection watchdog timeout in milliseconds</param>
        /// <returns></returns>
        Task<IWorkerSession> AttachWorkerBySerialTask(long serial,
            IWorkerListener workerListener = null,
            int connectionTimeoutMillis = ClientConstants.DefaultConnectionTimeoutMillis,
            int watchdogTimeoutMillis = ClientConstants.DefaultWatchdogTimeoutMillis);

        /// <summary>
        /// Start a new worker session.
        /// </summary>
        /// <param name="options">Worker options</param>
        /// <param name="workerListener">Optional listener for connection-level events</param>
        /// <param name="timeoutMillis">Worker connection timeout in milliseconds</param>
        /// <param name="watchdogTimeoutMillis">The Connection watchdog timeout in milliseconds</param>
        /// <returns></returns>
        Task<IWorkerSession> StartWorkerTask(WorkerOptions options,
            IWorkerListener workerListener = null, int timeoutMillis = ClientConstants.DefaultConnectionTimeoutMillis,
            int watchdogTimeoutMillis = ClientConstants.DefaultWatchdogTimeoutMillis);

        Dictionary<long, IPersistentQueryConfig> GetPersistentQueryConfigs();
        IPersistentQueryConfig GetPersistentQueryConfig(long name);
    }


    public static class OpenApiClient_Extensions
    {
        /// <summary>
        /// Login with key-based credentials. The
        /// </summary>
        /// <param name="self">This object (needed because this method is a C# extension method)</param>
        /// <param name="privKeyFilename">The filename of the Deephaven key-based credential file,
        /// e.g. a file like "priv-myuser-iris.base64.txt"</param>
        public static Task LoginTask(this IOpenApiClient self, string privKeyFilename)
        {
            using (var textReader = File.OpenText(privKeyFilename))
            {
                return self.LoginTask(textReader);
            }
        }

        public static void Login(this IOpenApiClient self, string username, string password, string operateAs = null)
        {
            ExceptionUtil.WaitOrUnwrappedException(self.LoginTask(username, password, operateAs));
        }

        /// <summary>
        /// Login with key-based credentials.
        /// </summary>
        /// <param name="self">This object (needed because this method is a C# extension method)</param>
        /// <param name="privKeyReader">A TextReader providing the contents of the Deephaven private key file,
        /// e.g. a file like "priv-myuser-iris.base64.txt"</param>
        public static void Login(this IOpenApiClient self, TextReader privKeyReader)
        {
            ExceptionUtil.WaitOrUnwrappedException(self.LoginTask(privKeyReader));
        }

        /// <summary>
        /// Login with key-based credentials.
        /// </summary>
        /// <param name="self">This object (needed because this method is a C# extension method)</param>
        /// <param name="privKeyFilename">The filename of the Deephaven key-based credential file,
        /// e.g. a file like "priv-myuser-iris.base64.txt"</param>
        public static void Login(this IOpenApiClient self, string privKeyFilename)
        {
            ExceptionUtil.WaitOrUnwrappedException(LoginTask(self, privKeyFilename));
        }

        public static IWorkerSession StartWorker(this IOpenApiClient self, WorkerOptions options,
            IWorkerListener workerListener = null,
            int connectionTimeoutMillis = ClientConstants.DefaultConnectionTimeoutMillis,
            int watchdogTimeoutMillis = ClientConstants.DefaultWatchdogTimeoutMillis)
        {
            return ExceptionUtil.ResultOrUnwrappedException(self.StartWorkerTask(options, workerListener,
                connectionTimeoutMillis, watchdogTimeoutMillis));
        }

        public static string[] GetWorkerProfiles(this IOpenApiClient self)
        {
            return ExceptionUtil.ResultOrUnwrappedException(self.GetWorkerProfilesTask());
        }

        public static IWorkerSession AttachWorkerByName(this IOpenApiClient self, string name,
            IWorkerListener workerListener = null,
            int connectionTimeoutMillis = ClientConstants.DefaultConnectionTimeoutMillis)
        {
            return ExceptionUtil.ResultOrUnwrappedException(self.AttachWorkerByNameTask(name, workerListener,
                connectionTimeoutMillis));
        }

        public static IWorkerSession AttachWorkerBySerial(this IOpenApiClient self, long serial,
            IWorkerListener workerListener = null,
            int connectionTimeoutMillis = ClientConstants.DefaultConnectionTimeoutMillis)
        {
            return ExceptionUtil.ResultOrUnwrappedException(self.AttachWorkerBySerialTask(serial, workerListener,
                connectionTimeoutMillis));
        }
    }
}