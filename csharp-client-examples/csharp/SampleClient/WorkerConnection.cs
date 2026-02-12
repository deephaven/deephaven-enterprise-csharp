using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Deephaven.OpenAPI.Core.API;
using Deephaven.OpenAPI.Shared.Worker;
using Deephaven.OpenAPI.Shared.Batch;
using Deephaven.OpenAPI.Shared.Cmd;
using Deephaven.OpenAPI.Shared.Data;
using Deephaven.OpenAPI.Shared.Data.Columns;
using Deephaven.OpenAPI.Shared.Metrics;
using static Deephaven.OpenAPI.Shared.Batch.BatchTableRequest;

namespace SampleClient
{
    /// <summary>
    /// A simple class demonstrating how to connect to a worker process, fetch
    /// and subscribe to table data.
    /// </summary>
    public class WorkerConnection : IDisposable
    {
        /// <summary>
        /// This implementation receives calls from the worker when various
        /// events occur.
        /// </summary>
        private class MyClient : WorkerClient
        {
            /// <summary>
            /// Map of exported table definitions
            /// </summary>
            private Dictionary<TableHandle, InitialTableDefinition> _definitions;

            /// <summary>
            /// The worker server from which we will receive messages.
            /// </summary>
            private WorkerServer _workerServer;

            public MyClient(Dictionary<TableHandle, InitialTableDefinition> definitions)
            {
                _definitions = definitions;
            }

            /// <summary>
            /// Called by the worker when a new table is exported.
            /// </summary>
            /// <param name="tableDef"></param>
            public void ExportedTableCreationMessage(InitialTableDefinition tableDef)
            {
                Console.Out.WriteLine("Table resolved: " + tableDef);
            }

            /// <summary>
            /// Called by the worker when an exported table is updated.
            /// </summary>
            /// <param name="clientId"></param>
            /// <param name="size"></param>
            public void ExportedTableUpdateMessage(TableHandle clientId, long size)
            {
                Console.Out.WriteLine("Table updated: " + clientId);
            }

            /// <summary>
            /// Called by the worker when an error occurs with respect to an exported table update.
            /// </summary>
            /// <param name="clientId"></param>
            /// <param name="errorMessage"></param>
            public void ExportedTableUpdateMessageError(TableHandle clientId, string errorMessage)
            {
                Console.Out.WriteLine("Table update error: " + errorMessage);
            }

            public WorkerServer GetServer()
            {
                return _workerServer;
            }

            /// <summary>
            /// Called when a client handle has been resolved during a batch operation (the server-side object
            /// has been created and exported).
            /// </summary>
            /// <param name="handle"></param>
            public void HandleResolved(TableHandle handle)
            {

            }

            public void IncrementalUpdates(TableHandle table, DeltaUpdates updates)
            {
                Console.Out.WriteLine("Got incremental updates");
            }

            public void OnClose(ushort code, string reason)
            {
                Console.Out.WriteLine("WorkerClient OnClose: {0} ({1})", reason, code);
            }

            public void OnError(Exception ex)
            {
                Console.Out.WriteLine("Error: " + ex);
            }

            public void OnOpen()
            {
                _workerServer.SetClient(this);
            }

            /// <summary>
            /// Should receive this immediately if we call Pong on the server.
            /// </summary>
            public void Ping()
            {
                Console.Out.WriteLine("Got ping from server");
            }

            public void ReportMetrics(string type, ServerObjectHandle tableHandle, long nanos)
            {

            }

            public void ReportMetrics(MetricsLog[] metrics)
            {

            }

            /// <summary>
            /// Called when we are subscrubed to the worker logs.
            /// </summary>
            /// <param name="log">the log item</param>
            public void SendLog(LogItem log)
            {

            }

            public void SetServer(WorkerServer server)
            {
                _workerServer = server;
            }

            public void InitialSnapshot(TableHandle table, TableSnapshot snapshot)
            {

            }

            public void TableMapStringKeyAdded(TableMapHandle handle, string key)
            {

            }

            public void TableMapStringArrayKeyAdded(TableMapHandle handle, string[] key)
            {

            }
        }

        private WorkerServer _server;
        private MyClient _myClient;
        private ClientTableIdFactory _clientTableIdFactory;
        private static int _clientId = 0;

        private Dictionary<TableHandle, InitialTableDefinition> _definitions =
            new Dictionary<TableHandle, InitialTableDefinition>();

        public WorkerConnection(QueryStatusWrapper wrapper, ConnectToken connectToken)
        {
            _myClient = new MyClient(_definitions);
            _clientId++;

            var builder = ServerBuilder<WorkerServer, WorkerClient>
                .Of((factory, send, message, close) => new WorkerServer_Impl(factory, send, message, close))
                .SetUrl(wrapper.Designated.WebsocketUrl);
            _server = builder.Start(_myClient, 60 * 1000);

            ConnectionSuccess connectionSuccess = _server.Register(_clientId, wrapper.Designated.ServiceId, connectToken);
            _clientTableIdFactory = new ClientTableIdFactory(_clientId, connectionSuccess.ConnectionId);
            Console.Out.WriteLine("Connected to worker, connection id: " + connectionSuccess.ConnectionId);
        }

        public InitialTableDefinition GetTableDefinition(TableHandle tableHandle)
        {
            return _definitions[tableHandle];
        }

        /// <summary>
        /// Fetch a table from the worker by name. Note this is asynchronous,
        /// will return a handle immediately but we won't know the table details
        /// until the server sends back the table definition.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public TableHandle FetchTable(string tableName)
        {
            var handle = _clientTableIdFactory.NewHandle();
            _server.FetchTableAsync(handle, tableName,
                definition =>
                {
                    Console.WriteLine("Got initial table definition, columns:");
                    Console.WriteLine(definition.Columns.Select(c => c.Name).Aggregate((n1, n2) => n1 + ", " + n2));
                    _definitions[handle] = definition;
                },
                failure => Console.WriteLine($"Failure fetching table: {failure}"),
                err => Console.WriteLine($"Error fetching table: {err}"));
            return handle;
        }

        /// <summary>
        /// Subscribe to the given table. If the table is not preemptive, first
        /// create a preemptive version and then subscribe. The table must be
        /// preemptive in order to receive updates.
        /// </summary>
        /// <param name="handle"></param>
        public void Subscribe(TableHandle handle)
        {
            InitialTableDefinition definition;
            if (!_definitions.TryGetValue(handle, out definition))
            {
                Console.WriteLine("No definition found for handle: " + handle);
                return;
            }
            else if (!definition.IsPreemptive)
            {
                var request = new BatchTableRequest();
                var op = new BatchTableRequestSerializedTableOps();
                op.Handles = new HandleMapping(handle, _clientTableIdFactory.NewHandle());
                op.UpdateIntervalMs = 1000;
                request.Ops = new BatchTableRequestSerializedTableOps[] {op};
                Console.WriteLine("first fetching preemptive version of table...");
                _server.BatchAsync(request, successCallback => { Subscribe(successCallback.Success[0]); },
                  failure => Console.Error.WriteLine($"Failure making table preemptive: {failure}"),
                  err => Console.Error.WriteLine($"Error making table preemptive: {err}"));
            }
            else
            {
                SubscribeToPreemptiveTable(handle);
            }
        }

        /// <summary>
        /// Subscribe to the given table, assumes it is already preemptive.
        /// </summary>
        /// <param name="handle"></param>
        public void SubscribeToPreemptiveTable(TableHandle handle)
        {
            InitialTableDefinition definition;
            if (!_definitions.TryGetValue(handle, out definition))
            {
                Console.WriteLine("No definition found for handle: " + handle);
                return;
            }
            var columns = definition.Columns;
            var columnBitArray = new BitArray(columns.Length);
            columnBitArray.SetAll(true);

            Console.Out.WriteLine("Subscribing..." + handle);
            _server.Subscribe(handle, columnBitArray, false);
        }

        /// <summary>
        /// Unsubscribe from the given table.
        /// </summary>
        /// <param name="handle"></param>
        public void Unsubscribe(TableHandle handle)
        {
            _server.Unsubscribe(handle);
        }

        /// <summary>
        /// Request a table snapshot asynchronously, and print the results.
        /// </summary>
        /// <param name="handle">the table</param>
        /// <param name="rows">which rows to snapshot</param>
        /// <param name="columns">which columns to snapshot</param>
        public void ConstructTableSnapshot(TableHandle handle, RangeSet rows, BitArray columns)
        {
            _server.ConstructSnapshotQueryAsync(handle, rows, columns,
                snapshot =>
                {
                    Console.WriteLine("Got table snapshot!");
                    for(var i =0; i < snapshot.DataColumns.Length; i++)
                    {
                        if (snapshot.DataColumns[i] != null)
                        {
                            Console.Write("ColumnData {0}:", i);
                            ColumnData columnData = snapshot.DataColumns[i];
                            object data = columnData.GetData();
                            if (data != null && data.GetType().IsArray)
                            {
                                Array array = (Array) data;
                                for (var j = 0; j < array.Length; j++)
                                {
                                    if (j > 0)
                                        Console.Write(",");
                                    Console.Write(array.GetValue(j));
                                }

                                Console.WriteLine();
                            }
                            else
                            {
                                Console.WriteLine("Got column data that is not an array: " + data);
                            }
                        }
                    }
                },
                failure => Console.WriteLine($"Failure getting table snapshot: {failure}"),
                err => Console.WriteLine($"Error getting table snapshot: {err}"));
        }

        /// <summary>
        /// Dispose the server connection (must be called to disconnect cleanly).
        /// </summary>
        public void Dispose()
        {
            _server.Dispose();
        }
    }
}
