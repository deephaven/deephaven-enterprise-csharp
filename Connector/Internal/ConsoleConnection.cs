/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using Deephaven.Connector.Internal.Parameters;
using Deephaven.OpenAPI.Core.API;
using Deephaven.OpenAPI.Core.RPC.Serialization.Java.Math;
using Deephaven.OpenAPI.Shared.Cmd;
using Deephaven.OpenAPI.Shared.Data;
using Deephaven.OpenAPI.Shared.Data.Columns;
using Deephaven.OpenAPI.Shared.Ide;
using Deephaven.OpenAPI.Shared.Ide.Cmd;
using Deephaven.OpenAPI.Shared.Metrics;
using Deephaven.OpenAPI.Shared.Primary;
using Deephaven.OpenAPI.Shared.Worker;

namespace Deephaven.Connector.Internal
{
    internal class ConsoleConnection : IDisposable
    {
        private const int connectionTimeoutMillis = 15 * 1000;

        private class MyWorkerClient : WorkerClient
        {
            private WorkerServer _workerServer;

            public void ExportedTableCreationMessage(InitialTableDefinition tableDef)
            {
            }

            public void ExportedTableUpdateMessage(TableHandle clientId, long size)
            {
            }

            public void ExportedTableUpdateMessageError(TableHandle clientId, string errorMessage)
            {
            }

            public WorkerServer GetServer()
            {
                return _workerServer;
            }

            public void HandleResolved(TableHandle handle)
            {
            }

            public void IncrementalUpdates(TableHandle table, DeltaUpdates updates)
            {
            }

            public void OnClose(ushort code, string reason)
            {
            }

            public void OnError(Exception ex)
            {
            }

            public void OnOpen()
            {
                _workerServer.SetClient(this);
            }

            public void Ping()
            {
            }

            public void ReportMetrics(string type, ServerObjectHandle tableHandle, long nanos)
            {
            }

            public void ReportMetrics(MetricsLog[] metrics)
            {
            }

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

        private const string RemoteQueryProcessorService = "RemoteQueryProcessor";

        // There's a regular set of JVM args that should improve performance, so we want to normally use those.
        private static readonly string[] DefaultJvmArgs =
        {
            "-XX:+UseConcMarkSweepGC",
            "-XX:+UseParNewGC",
            "-XX:SurvivorRatio=8",
            "-XX:TargetSurvivorRatio=90",
            "-XX:+CMSParallelRemarkEnabled",
            "-XX:CMSInitiatingOccupancyFraction=90",
            "-XX:+CMSClassUnloadingEnabled",
            "-XX:ParallelGCThreads=4"
        };

        private WebApiServer _apiServer;

        private WorkerServer _workerServer;
        private MyWorkerClient _workerClient;
        private ClientScriptIdFactory _scriptFactory;
        private ClientTableIdFactory _tableFactory;
        private ClientExecutionIdFactory _exFactory;
        private ScriptHandle _serverHandleForConsole;

        private RequestId _requestId;
        private ConsoleSessionType _sessionType;
        private int _remoteDebugPort;
        private bool _suspendWorker;
        private int _maxHeapMb;
        private int _timeoutMs;
        private List<string> _whiteListClasses = new List<string>();

        public ConsoleConnection(WebApiServer apiServer, RequestId requestId,
            ConsoleSessionType sessionType, int remoteDebugPort,
            bool suspendWorker, int maxHeapMb, int timeoutMs,
            bool localDateAsString, bool localTimeAsString)
        {
            _apiServer = apiServer;
            _requestId = requestId;
            _sessionType = sessionType;
            _remoteDebugPort = remoteDebugPort;
            _suspendWorker = suspendWorker;
            _maxHeapMb = maxHeapMb;
            _timeoutMs = timeoutMs;
            if (!localTimeAsString)
            {
                _whiteListClasses.Add("java.time.LocalTime");
            }
            if (!localDateAsString)
            {
                _whiteListClasses.Add("java.time.LocalDate");
            }

            StartConsole(requestId);
        }

        private ConsoleAddress CreateWorker(RequestId requestId)
        {
            List<string> jvmArgs = new List<string>();
            jvmArgs.AddRange(DefaultJvmArgs);

            // if we have whitelisted classes, add a JVM arg for those
            if (_whiteListClasses.Count > 0)
            {
                jvmArgs.Add("-DColumnPreviewManager.whiteListClasses=" +
                    string.Join(",", _whiteListClasses));
            }

            // If the user has specified that they want to enable server-side debugging, pass those parameters along.
            if (_remoteDebugPort > 0)
            {
                string doSuspend = (_suspendWorker ? "y" : "n");
                jvmArgs.Add("-agentlib:jdwp=transport=dt_socket,server=y,suspend=" + doSuspend + ",address=" + _remoteDebugPort);
            }

            // Now create the ConsoleConfig (which is really a WorkerConfig) with the various properties.
            ConsoleConfig config = new ConsoleConfig
            {
                Classpath = Array.Empty<string>(),
                Debug = false,
                DetailedGCLogging = false,
                EnvVars = Array.Empty<string[]>(),
                MaxHeapMb = _maxHeapMb,
                OmitDefaultGcParameters = false,
                QueryDescription = "ADO.NET Open API client: " + System.Environment.UserDomainName + "\"" + System.Environment.UserName + " - " + System.Environment.MachineName,
                JvmArgs = jvmArgs.ToArray()
            };

            return _apiServer.StartWorker(requestId, config, _timeoutMs);
        }

        private void StartConsole(RequestId id)
        {
            // Create a new worker if one does not already exist.
            ConsoleAddress workerAddress = CreateWorker(id);

            // Once the worker has been created, we need to start the server and register it.
            var builder = ServerBuilder<WorkerServer, WorkerClient>.Of(
                (factory, send, message, close) => new WorkerServer_Impl(factory, send, message, close)).SetUrl(workerAddress.WebsocketUrl);
            _workerClient = new MyWorkerClient();
            _workerServer = builder.Start(_workerClient, connectionTimeoutMillis);

            // we must get an auth token for the remote query processor specifically
            ConnectToken connectToken = _apiServer.CreateAuthToken(RemoteQueryProcessorService);
            ConnectionSuccess success = _workerServer.Register(id.ClientId, workerAddress.ServiceId, connectToken, _timeoutMs);
            _scriptFactory = new ClientScriptIdFactory(id.ClientId, success.ConnectionId);
            _serverHandleForConsole = _scriptFactory.NewHandle();
            ConsoleConnectionResult consoleConnection = _workerServer.StartConsole(_serverHandleForConsole, _sessionType, _timeoutMs);
            _exFactory = new ClientExecutionIdFactory(id.ClientId, success.ConnectionId);
            _tableFactory = new ClientTableIdFactory(id.ClientId, success.ConnectionId);

            // we need to define a few types in Python
            if (_sessionType == ConsoleSessionType.Python)
            {
                RunPythonPreamble();
            }
        }

        private static readonly Random _random = new Random();

        private static string UniqueVarName()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return "__result" + new string(Enumerable.Repeat(chars, 5)
              .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public class SnapshotQueryResult
        {
            private WorkerServer _workerServer;

            public TableHandle TableHandle { get; private set; }
            public InitialTableDefinition InitialTableDefinition { get; private set; }

            private const string ColumnTypeBoolean = "java.lang.Boolean";
            private const string ColumnTypeByte = "byte";
            private const string ColumnTypeChar = "char";
            private const string ColumnTypeInt16 = "short";
            private const string ColumnTypeInt32 = "int";
            private const string ColumnTypeInt64 = "long";
            private const string ColumnTypeFloat = "float";
            private const string ColumnTypeDouble = "double";
            private const string ColumnTypeDateTime = "com.illumon.iris.db.tables.utils.DBDateTime";
            private const string ColumnTypeString = "java.lang.String";
            private const string ColumnTypeStringArray = "java.lang.String[]";
            private const string ColumnTypeBigDecimal = "java.math.BigDecimal";
            private const string ColumnTypeBigInteger = "java.math.BigInteger";
            private const string ColumnTypeLocalDate = "java.time.LocalDate";
            private const string ColumnTypeLocalTime = "java.time.LocalTime";

            public SnapshotQueryResult(WorkerServer workerServer,
                TableHandle tableHandle, InitialTableDefinition initialTableDefinition)
            {
                _workerServer = workerServer;
                TableHandle = tableHandle;
                InitialTableDefinition = initialTableDefinition;
            }

            public int ColumnCount => InitialTableDefinition.Columns.Length;

            public string GetColumnName(int index)
            {
                return InitialTableDefinition.Columns[index].Name;
            }

            public int GetColumnIndex(string name)
            {
                for (int index = 0; index < InitialTableDefinition.Columns.Length; index++)
                {
                    if (InitialTableDefinition.Columns[index].Name.Equals(name))
                    {
                        return index;
                    }
                }
                throw new IndexOutOfRangeException();
            }

            public SnapshotColumnData[] GetTableData(long first, long last, out long size)
            {
                RangeSet rows = new RangeSet();
                rows.AddRange(new Range(first, last));
                BitArray columns = new BitArray(InitialTableDefinition.Columns.Length);
                columns.SetAll(true);
                TableSnapshot tableSnapshot = _workerServer.ConstructSnapshotQuery(TableHandle, rows, columns);
                SnapshotColumnData[] columnDatas = new SnapshotColumnData[tableSnapshot.DataColumns.Length];
                size = 0;
                for (int i = 0; i < columnDatas.Length; i++)
                {
                    string name = InitialTableDefinition.Columns[i].Name;
                    if (i == 0)
                    {
                        size = tableSnapshot.IncludedRows.Size;
                    }
                    else if (tableSnapshot.IncludedRows.Size != size)
                    {
                        throw new InvalidOperationException("Internal state error" +
                            " - received column data with inconsistent number of rows for column " + name);
                    }
                    string type = InitialTableDefinition.Columns[i].Type;
                    object data = tableSnapshot.DataColumns[i].GetData();
                    ColumnData columnData = tableSnapshot.DataColumns[i];
                    switch (type)
                    {
                        case ColumnTypeBoolean:
                            columnDatas[i] = new SnapshotBoolColumnData((byte[])data);
                            break;
                        case ColumnTypeByte:
                            columnDatas[i] = new SnapshotByteColumnData((sbyte[])data);
                            break;
                        case ColumnTypeChar:
                            columnDatas[i] = new SnapshotCharColumnData((char[])data);
                            break;
                        case ColumnTypeInt16:
                            columnDatas[i] = new SnapshotInt16ColumnData((short[])data);
                            break;
                        case ColumnTypeInt32:
                            columnDatas[i] = new SnapshotInt32ColumnData((int[])data);
                            break;
                        case ColumnTypeInt64:
                            columnDatas[i] = new SnapshotInt64ColumnData((long[])data);
                            break;
                        case ColumnTypeFloat:
                            columnDatas[i] = new SnapshotFloatColumnData((float[])data);
                            break;
                        case ColumnTypeDouble:
                            columnDatas[i] = new SnapshotDoubleColumnData((double[])data);
                            break;
                        case ColumnTypeDateTime:
                            columnDatas[i] = new SnapshotDateTimeColumnData((long[])data);
                            break;
                        case ColumnTypeString:
                            columnDatas[i] = new SnapshotStringColumnData((string[])data);
                            break;
                        case ColumnTypeStringArray:
                            columnDatas[i] = new SnapshotStringArrayColumnData((string[][])data);
                            break;
                        case ColumnTypeBigDecimal:
                            columnDatas[i] = new SnapshotDecimalColumnData((BigDecimal?[])data);
                            break;
                        case ColumnTypeBigInteger:
                            columnDatas[i] = new SnapshotBigIntegerColumnData((BigInteger?[])data);
                            break;
                        case ColumnTypeLocalDate:
                            columnDatas[i] = new SnapshotLocalDateColumnData((LocalDate[])data);
                            break;
                        case ColumnTypeLocalTime:
                            columnDatas[i] = new SnapshotLocalTimeColumnData((LocalTime[])data);
                            break;
                        default:
                            if (columnData is StringArrayColumnData)
                            {
                                columnDatas[i] = new SnapshotStringColumnData((string[])data);
                            }
                            else
                            {
                                throw new NotSupportedException("Unexpected column data type \"" + type + "\" for column " + name);
                            }
                            break;
                    }
                }
                return columnDatas;
            }
        }

        private const string pythonPreamble =
            "ADO_BigDecimal = jpy.get_type('java.math.BigDecimal')\n"
            + "ADO_BigInteger = jpy.get_type('java.math.BigInteger')\n"
            + "ADO_LocalDate = jpy.get_type('java.time.LocalDate')\n"
            + "ADO_LocalTime = jpy.get_type('java.time.LocalTime')\n"
            + "ADO_QueryConstants = jpy.get_type('com.illumon.util.QueryConstants')\n"
            + "ADO_DBDateTime = jpy.get_type('com.illumon.iris.db.tables.utils.DBDateTime')\n";

        private void RunPythonPreamble()
        {
            ExecutionHandle exHandle = _exFactory.NewHandle();
            exHandle.ScriptId = _serverHandleForConsole.ScriptId;
            CommandResult result = _workerServer.ExecuteCommand(exHandle, pythonPreamble);
            if (!string.IsNullOrEmpty(result.Error))
            {
                throw new Exception($"Error configuring Python session: {result.Error}");
            }
        }

        public void BindVariables(Dictionary<string, BindParameter> bindParameters)
        {
            foreach(KeyValuePair<string, BindParameter> keyValuePair in bindParameters)
            {
                string assignment = keyValuePair.Key + "=" + keyValuePair.Value.GetLiteral(_sessionType);
                ExecutionHandle exHandle = _exFactory.NewHandle();
                exHandle.ScriptId = _serverHandleForConsole.ScriptId;
                CommandResult result = _workerServer.ExecuteCommand(exHandle, assignment);
                if(!string.IsNullOrEmpty(result.Error))
                {
                    throw new Exception($"Error binding variable {keyValuePair.Key}: {result.Error}");
                }
            }
        }

        private string ReplaceVariableNames(string query, Dictionary<string,string> parameterNameMap)
        {
            // Replace each expression starting with a "@" with the matching
            // bound variable name.
            // Literal "@" characters are supported by doubling up (a "@@" in
            // the query will result in a single "@" in the final query and we
            // will not attempt to treat is as a bound parameter)
            StringBuilder exprBuilder = new StringBuilder();
            for (int i = 0; i < query.Length; i++)
            {
                if (query[i] == '@')
                {
                    if (query[i + 1] != '@')
                    {
                        bool foundParam = false;
                        foreach (KeyValuePair<string, string> paramEntry in parameterNameMap)
                        {
                            if (query.Length >= (i+paramEntry.Key.Length)
                                && query.Substring(i, paramEntry.Key.Length).Equals(paramEntry.Key))
                            {
                                exprBuilder.Append(paramEntry.Value);
                                i += paramEntry.Key.Length-1;
                                foundParam = true;
                                break;
                            }
                        }
                        if (!foundParam)
                        {
                            throw new InvalidOperationException("No matching parameter found for bound variable starting at index " + i);
                        }
                    }
                    else
                    {
                        exprBuilder.Append('@');
                        i++; // this is an escape, we want just one '@' in the final query
                    }
                }
                else
                {
                    exprBuilder.Append(query[i]);
                }
            }
            return exprBuilder.ToString();
        }

        // this may be overly restrictive but simpler than trying to match precisely each session language
        private static readonly Regex VarNameRegex = new Regex("[a-zA-Z_][a-zA-Z0-9_]*");

        /// <summary>
        /// Execute the given query expression(s) and return an array of the table names created as a result.
        /// </summary>
        /// <param name="multiQueryExpr">A newline delimited list of query expressions</param>
        /// <returns></returns>
        public SnapshotQueryResult[] ExecuteSnapshotQuery(string multiQueryExpr, Dictionary<string, string> parameterNameMap,
            int timeoutMs)
        {
            // check that the var names are legal java/groovy/python
            foreach(string varName in parameterNameMap.Keys)
            {
                if (!VarNameRegex.IsMatch(varName))
                {
                    throw new ArgumentException("Invalid name for bound variable: " + varName);
                }
            }

            // build the final script that will assign each query table to a unique variable
            StringBuilder finalScriptBuilder = new StringBuilder();
            List<string> tableNames = new List<string>();
            foreach (string rawQueryExpr in multiQueryExpr.Split('\n'))
            {
                string queryExpr = ReplaceVariableNames(rawQueryExpr, parameterNameMap);
                string tableName = UniqueVarName();
                tableNames.Add(tableName);
                finalScriptBuilder.Append(tableName).Append("=").Append(queryExpr).Append('\n');
            }

            // execute the script in one shot
            ExecutionHandle exHandle = _exFactory.NewHandle();
            exHandle.ScriptId = _serverHandleForConsole.ScriptId;
            CommandResult result = _workerServer.ExecuteCommand(exHandle, finalScriptBuilder.ToString(), timeoutMs);

            // check that we got all our expected tables before the heavy lifting starts
            foreach (string tableName in tableNames)
            {
                if (!Array.Exists(result.Changes.Created, def => def.Name == tableName) || !string.IsNullOrEmpty(result.Error))
                {
                    throw new Exception("Error executing query: " + result.Error);
                }
            }

            // snapshot each live table and collect the results
            SnapshotQueryResult[] results = new SnapshotQueryResult[tableNames.Count];
            for (int i = 0; i < tableNames.Count; i++)
            {
                // fetch the live tables, take snapshot
                TableHandle tableHandle = _tableFactory.NewHandle();
                _workerServer.FetchScriptTable(tableHandle, _serverHandleForConsole, tableNames[i]);
                try
                {
                    // take a one-time snapshot
                    TableHandle snapshotTableHandle = _tableFactory.NewHandle();
                    InitialTableDefinition snapshotTableDef = _workerServer.Snapshot(null, tableHandle, snapshotTableHandle, true, new string[0]);
                    results[i] = new SnapshotQueryResult(_workerServer, snapshotTableHandle, snapshotTableDef);
                }
                finally
                {
                    // release original table
                    _workerServer.Release(tableHandle);
                }
            }

            return results;
        }

        private bool _disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _workerServer.Dispose(); // first close our connection to the worker
            _apiServer.StopWorker(_requestId); // stop the worker at the server
            _workerServer = null;
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
    }
}
