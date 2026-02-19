/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Deephaven.OpenAPI.Client;
using Deephaven.OpenAPI.Shared.Data;

namespace Examples
{
    public class Program : IWorkerListener, IOpenApiListener
    {
        public static int Main(string[] args)
        {
            if (args.Length != 4 && args.Length != 5)
            {
                Console.Error.WriteLine("Program arguments: exampleName server username password [operateAs]");
                Environment.Exit(1);
            }
            var argIdx = 0;
            var exampleName = args[argIdx++];
            var host = args[argIdx++];
            var username = args[argIdx++];
            var password = args[argIdx++];
            var operateAs = argIdx < args.Length ? args[argIdx++] : username;

            try
            {
                new Program(host, username, password, operateAs).Run(exampleName);
                Console.Error.WriteLine("Program exited normally");
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Caught exception: {e}");
                return 1;
            }
        }

        private static readonly Dictionary<string, Action<IOpenApiClient, IQueryScope>> _examples =
            new Dictionary<string, Action<IOpenApiClient, IQueryScope>>
        {
            { "validate", ValidationExample.Run },
            { "lastBy", LastByExample.Run },
            { "filter", FilterExample.Run },
            { "cleanup", TableCleanupExample.Run },
            { "sort", SortExample.Run },
            { "snapshot", SnapshotExample.Run },
            { "select", SelectExample.Run },
            { "view", ViewExample.Run },
            { "join", JoinExample.Run },
            { "ht", HeadAndTailExample.Run },
            { "tt", TimeTableExample.Run },
            { "tablesnapshot", TableSnapshotExample.Run },
            { "nt", NewTableExample.Run },
            { "agg", AggregatesExample.Run },
            { "ungroup", UngroupExample.Run },
            { "merge", MergeTablesExample.Run },
            { "drop", DropColumnsExample.Run },
            { "catalog", CatalogExample.Run },
            { "catalogTable", CatalogTableExample.Run },
            { "strFilter", StringFilterExample.Run },
            { "dtFilter", DateTimeFilterExample.Run },
            { "gc", GCExample.Run },
            { "gcsession", GcSessionExample.Run }
        };

        private readonly string _host;
        private readonly string _username;
        private readonly string _password;
        private readonly string _operateAs;


        private Program(string host, string username, string password, string operateAs)
        {
            _host = host;
            _username = username;
            _password = password;
            _operateAs = operateAs;
        }

        private void Run(string exampleName)
        {
            var runEverything = exampleName == "*";
            var examplesToRun = _examples.Where(kv => runEverything || kv.Key == exampleName).ToArray();
            if (examplesToRun.Length == 0)
            {
                Console.WriteLine($"No example found with the name {exampleName}");
                return;
            }
            using (var client = OpenApi.Connect(_host, openApiListener: this, connectionTimeoutMillis: 10000))
            {
                // block on login
                client.Login(_username, _password, _operateAs);

                Console.WriteLine("Starting worker...");
                var workerOptions = new WorkerOptions("Default");
                workerOptions.AddJvmArg("-ea");
                workerOptions.AddJvmArg("-agentlib:jdwp=transport=dt_socket,server=y,suspend=n,address=7172");

                // start a new worker (blocking)
                using (var workerSession = client.StartWorker(workerOptions, this, watchdogTimeoutMillis: 10000))
                {
                    Console.WriteLine("Worker started.");
                    foreach (var entry in examplesToRun)
                    {
                        Console.WriteLine($"Running example {entry.Key}");
                        try
                        {
                            entry.Value(client, workerSession.QueryScope);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Example {entry.Key} failed with the following exception: {e}");
                        }
                    }
                }
            }
        }

        public void OnError(IWorkerSession arg1, Exception ex)
        {
            Console.WriteLine("Worker error: " + ex);
        }

        public void OnPing(IWorkerSession obj)
        {
            Console.WriteLine("Worker ping");
        }

        public void OnLogMessage(IWorkerSession arg1, LogMessage logMessage)
        {
            Console.WriteLine("Worker log message: " + logMessage);
        }

        public void OnClosed(IWorkerSession arg1, ushort code, string msg)
        {
            Console.WriteLine($"Worker closed: {msg} ({code})");
        }

        public void OnOpen(IWorkerSession obj)
        {
            Console.WriteLine("Worker open");
        }

        void IOpenApiListener.OnPersistentQueryAdded(IOpenApiClient openApiClient, IPersistentQueryConfig persistentQueryConfig)
        {
            Console.WriteLine("PQ added: " + persistentQueryConfig.Name);
        }

        void IOpenApiListener.OnPersistentQueryModified(IOpenApiClient openApiClient, IPersistentQueryConfig persistentQueryConfig)
        {
            Console.WriteLine("PQ modified: " + persistentQueryConfig.Name);
        }

        void IOpenApiListener.OnPersistentQueryRemoved(IOpenApiClient openApiClient, IPersistentQueryConfig persistentQueryConfig)
        {
            Console.WriteLine("PQ removed: " + persistentQueryConfig.Name);
        }

        void IOpenApiListener.OnClosed(IOpenApiClient openApiClient, ushort code, string message)
        {
            Console.WriteLine("Client closed: {0} ({1})", message, code);
        }

        void IOpenApiListener.OnError(IOpenApiClient openApiClient, Exception ex)
        {
            Console.WriteLine("Client error: " + ex);
        }

        void IOpenApiListener.OnOpen(IOpenApiClient openApiClient)
        {
            Console.WriteLine("Client connected");
        }

        void IOpenApiListener.OnAuthTokenRefresh(IOpenApiClient openApiClient, RefreshToken refreshToken)
        {
            Console.WriteLine("Auth token refresh: " + refreshToken);
        }

        void IOpenApiListener.OnAuthTokenError(IOpenApiClient openApiClient, string error)
        {
            Console.WriteLine("Auth token error: " + error);
        }
    }
}
