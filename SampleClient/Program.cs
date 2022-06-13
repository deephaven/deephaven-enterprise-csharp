/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Deephaven;
using Deephaven.OpenAPI.Client;
using Deephaven.OpenAPI.Core.API.Util;
using Deephaven.OpenAPI.Shared.Data;

namespace SampleClient
{
    class Program : IOpenApiListener, IWorkerListener
    {
        public static void Main(string[] args)
        {
            if (args.Length != 3 && args.Length != 4)
            {
                Console.Error.WriteLine($"Program arguments: host username password [operateAs]");
                Environment.Exit(1);
            }
            var host = args[0];
            var username = args[1];
            var password = args[2];
            var operateAs = args.Length == 4 ? args[3] : username;

            try
            {
                new Program(username, password, operateAs).Run(host);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Caught exception: {e}");
            }
        }

        private class Command
        {
            public string Shortcut { get; }
            public string Description { get; }
            public Action Action { get; }

            public Command(string shortcut, string description, Action action) =>
                (Shortcut, Description, Action) = (shortcut, description, action);
        }

        private readonly Dictionary<string, Command> _actionMap;

        private bool _quit = false;

        private readonly string _username;
        private readonly string _password;
        private readonly string _operateAs;

        private readonly Command loginCommand;
        private readonly Command startWorkerCommand;
        private readonly Command attachWorkerCommand;
        private readonly Command attachWorkerBySerialCommand;
        private readonly Command fetchBoundTableCommand;
        private readonly Command fetchIntradayTableCommand;
        private readonly Command fetchHistoricalTableCommand;
        private readonly Command helpCommand;


        private IOpenApiClient _openApiClient;
        private bool _loggedIn = false;
        private IWorkerSession _workerSession;
        private IQueryTable _table;

        private Program(string username, string password, string operateAs)
        {
            _username = username;
            _password = password;
            _operateAs = operateAs;

            var commands = new []
            {
                new Command("q", "quit", Quit),
                loginCommand = new Command("l", "login", Login),
                startWorkerCommand = new Command("start", "start worker", StartWorker),
                attachWorkerCommand = new Command("a", "attach to worker by name", AttachWorkerByName),
                attachWorkerBySerialCommand = new Command("as", "attach to worker by serial", AttachWorkerBySerial),
                new Command("stop", "stop/release worker", StopWorker),
                fetchBoundTableCommand = new Command("fb", "fetch bound table", FetchBoundTable),
                fetchIntradayTableCommand = new Command("fi", "fetch intraday table", FetchIntradayTable),
                fetchHistoricalTableCommand = new Command("fh", "fetch historical table", FetchHistoricalTable),
                new Command("r", "release table", ReleaseTable),
                new Command("td", "table data", GetTableData),
                new Command("sort", "apply sort", ApplySort),
                new Command("filter", "apply filter", ApplyFilter),
                new Command("lb", "last by", LastBy),
                new Command("uv", "update view", UpdateView),
                new Command("sub", "subscribe", Subscribe),
                new Command("unsub","unsubscribe", Unsubscribe),
                helpCommand = new Command("?", "Show this menu", ShowMenu)
            };
            _actionMap = commands.ToDictionary(c => c.Shortcut, c => c);
        }

        private void Run(String host)
        {
            using (_openApiClient = OpenApi.Connect(host, openApiListener: this))
            {
                Console.WriteLine("Connected to " + host);
                ShowMenu();
                while (!_quit)
                {
                    var s = GetInput($"Enter a command ({helpCommand.Shortcut} for menu)", "");
                    if (_actionMap.TryGetValue(s, out var c))
                    {
                        try
                        {
                            c.Action.Invoke();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error running command \"{c.Description}\": {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Unknown command: {s}");
                    }
                }

                SetWorkerSession(null);
            }
        }

        private void ShowMenu()
        {
            foreach (var cmd in _actionMap.Values)
            {
                Console.WriteLine($"{cmd.Shortcut} - {cmd.Description}");
            }
        }


        private void Quit()
        {
            _quit = true;
        }

        private void Login()
        {
            if (_loggedIn)
            {
                Console.WriteLine("Already logged in!");
                return;
            }
            _openApiClient.Login(_username, _password, _operateAs);
            _loggedIn = true;
            Console.WriteLine("Logged in successfully");
        }

        private void AttachWorkerByName()
        {
            if (!ConfirmLoggedIn())
            {
                return;
            }
            var pqName = GetInput("Enter a PQ name to attach", "");
            var session = _openApiClient.AttachWorkerByName(pqName);
            SetWorkerSession(session);
            Console.WriteLine("Worker attached");
        }

        private void AttachWorkerBySerial()
        {
            if (!ConfirmLoggedIn())
            {
                return;
            }
            var serialText = GetInput("Enter a PQ serial to attach", "");
            var serial = long.Parse(serialText);
            var session = _openApiClient.AttachWorkerBySerial(serial);
            SetWorkerSession(session);
            Console.WriteLine("Worker attached");
        }

        private void StartWorker()
        {
            if (!ConfirmLoggedIn())
            {
                return;
            }
            var workerOptions = new WorkerOptions("Default");
            var maxHeapText = GetInput("Enter max heap in MB", "4000");
            workerOptions.MaxHeapMB = int.Parse(maxHeapText);
            Console.WriteLine("Starting worker...");
            var session = _openApiClient.StartWorker(workerOptions);
            SetWorkerSession(session);
            Console.WriteLine("Worker started.");
        }

        private void StopWorker()
        {
            if (_workerSession == null)
            {
                Console.WriteLine("No worker to stop/release");
                return;
            }
            SetWorkerSession(null);
        }

        private void FetchBoundTable()
        {
            if (!ConfirmSessionExists())
            {
                return;
            }
            var name = GetInput("Enter a bound table/query name", "");
            var t = _workerSession.QueryScope.BoundTable(name);
            FinishResolving("Bound table", t, true);
        }

        private void FetchIntradayTable()
        {
            if (!ConfirmSessionExists())
            {
                return;
            }
            var (ns, tableName) = GetNamespaceAndTableName(DemoConstants.IntradayNamespace, DemoConstants.IntradayTable);
            var t = _workerSession.QueryScope.IntradayTable(ns, tableName);
            FinishResolving("Intraday table", t, true);
        }

        private void FetchHistoricalTable()
        {
            if (!ConfirmSessionExists())
            {
                return;
            }

            var (ns, tableName) = GetNamespaceAndTableName(DemoConstants.HistoricalNamespace, DemoConstants.HistoricalTable);
            var t = _workerSession.QueryScope.HistoricalTable(ns, tableName);
            FinishResolving("Historical table", t, true);
        }

        private static (string, string) GetNamespaceAndTableName(string defaultNs, string defaultTable)
        {
            var ns = GetInput("Enter namespace", defaultNs);
            var tableName = GetInput("Enter table name", defaultTable);
            return (ns, tableName);
        }

        private void ReleaseTable()
        {
            if (_table == null)
            {
                Console.WriteLine("No active table to dispose");
                return;
            }

            _table.Dispose();
            _table = null;
            Console.WriteLine("Table disposed");
        }

        private void GetTableData()
        {
            if (!ConfirmTableBound())
            {
                return;
            }
            PrintUtils.PrintTableData(_table);
        }

        private void ApplySort()
        {
            if (!ConfirmTableBound())
            {
                return;
            }
            var sortList = new List<SortPair>();
            while (true)
            {
                var sortStr = GetInput("Enter sort column and order (eg \"Sym [asc|desc] [abs]\") or blank line to quit", "");
                if (sortStr.Length == 0)
                {
                    break;
                }
                var tokens = sortStr.Split(' ');
                var sortDirection = SortDirection.Ascending;
                if (tokens.Length > 1)
                {
                    sortDirection = tokens[1].StartsWith("a", StringComparison.InvariantCulture)
                        ? SortDirection.Ascending
                        : SortDirection.Descending;
                }
                var abs = tokens.Length > 2;
                sortList.Add(new SortPair(tokens[0], sortDirection, abs));
            }

            var t = _table.Sort(sortList.ToArray());
            FinishResolving("Sort", t, false);
        }

        private void ApplyFilter()
        {
            if (!ConfirmTableBound())
            {
                return;
            }
            var condition = GetInput("Condition", "Ticker = `AAPL` && Close <= 120");
            var t = _table.Where(condition);
            FinishResolving("Filter", t, false);
        }

        private void LastBy()
        {
            if (!ConfirmTableBound())
            {
                return;
            }
            var columnList = new List<string>();
            while (true)
            {
                var customColumn = GetInput("Enter next grouping column (empty line to stop)", "");
                if (customColumn.Length == 0)
                {
                    break;
                }
                columnList.Add(customColumn);
            }
            var t = _table.LastBy(columnList.ToArray());
            FinishResolving("LastBy", t, false);
        }

        private void UpdateView()
        {
            if (!ConfirmTableBound())
            {
                return;
            }
            var customColumnList = new List<string>();
            while (true)
            {
                var customColumn = GetInput("Enter next column expression (empty line to stop)", "");
                if (customColumn.Length == 0)
                {
                    break;
                }
                customColumnList.Add(customColumn);
            }

            var t = _table.UpdateView(customColumnList.ToArray());
            FinishResolving("Update", t, false);
        }

        private void Subscribe()
        {
            if (!ConfirmTableBound())
            {
                return;
            }
            _table = _table.Preemptive(1000);

            _table.OnTableUpdate += OnTableUpdate;

            _table.SubscribeAll();
            Console.WriteLine("Subscribed");
        }

        private void Unsubscribe()
        {
            if (!ConfirmTableBound())
            {
                return;
            }
            _table.Unsubscribe();
            Console.WriteLine("Unsubscribed");
        }

        private void OnTableUpdate(IQueryTable table, ITableUpdate deltaUpdate)
        {
            Console.WriteLine("Got table update");
        }

        private void FinishResolving(String what, IQueryTable newTable, bool disposeOld)
        {
            if (newTable != null)
            {
                try
                {
                    newTable.TableState.Resolve();
                    Console.WriteLine($"Finished resolve of {what}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Caught exception while attemping to resolve {what}: {e}");
                    // Leave table state alone
                    Console.WriteLine("Going back to previous table...");
                    return;
                }
            }

            var oldTable = _table;
            _table = newTable;
            if (oldTable != null && disposeOld)
            {
                oldTable.Dispose();
                Console.WriteLine("Diposed previous table");
            }
        }

        private void SetWorkerSession(IWorkerSession newSession)
        {
            if (_workerSession != null)
            {
                FinishResolving("Table reset", null, true);
                Console.WriteLine("Disposing session.");
                _workerSession.Dispose();
            }
            _workerSession = newSession;
        }

        private bool ConfirmLoggedIn()
        {
            if (_loggedIn)
            {
                return true;
            }

            ShowHelp("Need to log in", loginCommand);
            return false;
        }


        private bool ConfirmSessionExists()
        {
            if (_workerSession != null)
            {
                return true;
            }

            _ = ConfirmLoggedIn();
            ShowHelp("Need to start or attach to worker",
                startWorkerCommand, attachWorkerCommand, attachWorkerBySerialCommand);
            return false;
        }

        private bool ConfirmTableBound()
        {
            if (_table != null)
            {
                return true;
            }

            _ = ConfirmSessionExists();
            ShowHelp("Need to fetch table", fetchBoundTableCommand, fetchIntradayTableCommand,
                fetchHistoricalTableCommand);
            return false;
        }

        private void ShowHelp(String what, params Command[] solutions)
        {
            Console.WriteLine($"{what}. Suggested actions: {solutions.MakeSeparatedList(", ", c => c.Shortcut)}");
        }

        void IOpenApiListener.OnPersistentQueryAdded(IOpenApiClient openApiClient, IPersistentQueryConfig queryConfig)
        {
            Console.WriteLine($"PQ Added: {queryConfig.Serial} [{queryConfig.Name}], Status={queryConfig.Status}");
        }

        void IOpenApiListener.OnPersistentQueryModified(IOpenApiClient openApiClient, IPersistentQueryConfig queryConfig)
        {
            Console.WriteLine($"PQ Modified: {queryConfig.Serial} [{queryConfig.Name}], Status={queryConfig.Status}");
        }

        void IOpenApiListener.OnPersistentQueryRemoved(IOpenApiClient openApiClient, IPersistentQueryConfig queryConfig)
        {
            Console.WriteLine($"PQ Removed: {queryConfig.Serial} [{queryConfig.Name}], Status={queryConfig.Status}");
        }

        void IOpenApiListener.OnClosed(IOpenApiClient openApiClient, ushort code, string message)
        {
            Console.WriteLine("Client closed: {0} ({1})", message, code);
        }

        void IOpenApiListener.OnError(IOpenApiClient openApiClient, Exception exception)
        {
            Console.WriteLine($"Error: {exception}");
        }

        void IOpenApiListener.OnOpen(IOpenApiClient openApiClient)
        {

        }

        void IOpenApiListener.OnAuthTokenRefresh(IOpenApiClient openApiClient, RefreshToken refreshToken)
        {

        }

        void IOpenApiListener.OnAuthTokenError(IOpenApiClient openApiClient, string error)
        {

        }

        public void OnOpen(IWorkerSession workerSession)
        {

        }

        public void OnClosed(IWorkerSession workerSession, ushort code, string err)
        {

        }

        public void OnError(IWorkerSession workerSession, Exception ex)
        {

        }

        public void OnPing(IWorkerSession workerSession)
        {

        }

        public void OnLogMessage(IWorkerSession workerSession, LogMessage logMessage)
        {

        }

        private static string GetInput(string prompt, string defaultValue)
        {
            var defaultPrompt = defaultValue.Length == 0 ? "" : $" [{defaultValue}]";
            Console.Write($"{prompt}{defaultPrompt}: ");
            var result = Console.ReadLine();
            if (result == null)
            {
                throw new Exception("Unexpected end of input");
            }
            if (result.Length == 0)
            {
                return defaultValue;
            }
            return result;
        }
    }
}
