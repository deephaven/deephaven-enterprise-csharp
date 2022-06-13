using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Deephaven;
using Deephaven.Connector;
using Deephaven.OpenAPI.Client;
using Deephaven.OpenAPI.Client.Data;
using Deephaven.OpenAPI.Client.Fluent;
using Deephaven.OpenAPI.Core.API.Util;
using static Deephaven.OpenAPI.Client.DeephavenImports;

namespace DocumentationExamples
{
    public class Program
    {
        public static int Main(string[] args)
        {

            try
            {
                string exampleName = null;
                string privKeyPath = null;
                string host = null;
                string username = null;
                string password = null;
                string operateAs = null;
                if (args.Length == 2)
                {
                    exampleName = args[0];
                    privKeyPath = args[1];
                }
                else if (args.Length == 4 || args.Length == 5)
                {
                    var argIdx = 0;
                    exampleName = args[argIdx++];
                    host = args[argIdx++];
                    username = args[argIdx++];
                    password = args[argIdx++];
                    operateAs = argIdx < args.Length ? args[argIdx] : username;
                }
                else
                {
                    Console.Error.WriteLine("Program arguments:");
                    Console.Error.WriteLine("  exampleName server username password [operateAs]");
                    Console.Error.WriteLine("  - OR -");
                    Console.Error.WriteLine("  exampleName privKeyFile");
                    Environment.Exit(1);
                }

                new Program(host, username, password, operateAs, privKeyPath).Run(exampleName);
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Caught exception: {e}");
                return 1;
            }
        }

        private readonly string _host;
        private readonly string _username;
        private readonly string _password;
        private readonly string _operateAs;
        private readonly string _privKeyPath;
        private readonly Dictionary<string, Action> _actions;

        private Program(string host, string username, string password, string operateAs, string privKeyPath)
        {
            _host = host;
            _username = username;
            _password = password;
            _operateAs = operateAs;
            _privKeyPath = privKeyPath;
            _actions = new Dictionary<string, Action>
            {
                { "helloworld", HelloWorld },
                { "hellopk", HelloPrivateKey },
                { "attach", AttachToPersistentQuery },
                { "pqmd", PrintPersistentQueryMetadata },
                { "ex1", ExampleQuery1 },
                { "ex1-nbp", ExampleQuery1_NotBestPractice },
                { "ex2", ExampleQuery2_InnerScope },
                { "scopes", ScopesAreIndependent },
                { "twoways", TwoWaysToDoASelect },
                { "select", SelectExpressions },
                { "view", ViewExpressions },
                { "drop", DropColumnsExample },
                { "join", JoinExample },
                { "match", MatchWithExample },
                { "matchAs", MatchWithAndAsExample },
                { "sum", SumByExample },
                { "ado", ADOExample },
                { "nvsf", NormalVsFluentExpressions },
                { "comb", Combinators },
                { "smethods", StringMethods },
                { "dtex", DateTimeExamples },
                { "bool", BooleanExamples },
                { "where", WhereExample },
                { "task", TaskExample },
                { "sort1", SortExample1 },
                { "sort2", SortExample2 },
                { "agg", AggregateExample },
                { "cd", AccessingColumnData },
                { "mix", MixWorkersAndPQs },
            };
        }

        private void Run(string exampleName)
        {
            if (!_actions.TryGetValue(exampleName, out var action))
            {
                throw new Exception($"Can't find example {exampleName}");
            }
            action();
        }

        private void HelloWorld()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var scope = workerSession.QueryScope;
                    var table = scope.HistoricalTable("LearnDeephaven", "EODTrades");
                    var (importDate, ticker) = table.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
                    var filtered = table.Where(importDate == "2017-11-01" && ticker == "AAPL");
                    PrintUtils.PrintTableData(filtered);
                }
            }
        }

        private void HelloPrivateKey()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_privKeyPath);

                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var scope = workerSession.QueryScope;
                    var table = scope.HistoricalTable("LearnDeephaven", "EODTrades");
                    var (importDate, ticker) = table.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
                    var filtered = table.Where(importDate == "2017-11-01" && ticker == "AAPL");
                    PrintUtils.PrintTableData(filtered);
                }
            }
        }

        private void AttachToPersistentQuery()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                using (var workerSession = client.AttachWorkerByName("GetSomeDataQuery"))
                {
                    var scope = workerSession.QueryScope;
                    var table = scope.BoundTable("query");
                    var t2 = table.Head(100);
                    PrintUtils.PrintTableData(t2);
                }
            }
        }

        private void PrintPersistentQueryMetadata()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                using (var workerSession = client.AttachWorkerByName("GetSomeDataQuery"))
                {
                    var scope = workerSession.QueryScope;
                    var table = scope.BoundTable("query");

                    var def = table.LowLevel.InitialTableDefinition;
                    Console.WriteLine($"There are {def.Columns.Length} columns");
                    foreach (var col in def.Columns)
                    {
                        Console.WriteLine($"{col.Name} {col.Type}");
                    }
                }
            }
        }


        private void ExampleQuery1()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var scope = workerSession.QueryScope;
                    var table = scope.HistoricalTable("LearnDeephaven", "EODTrades");
                    var (importDate, ticker, close, volume) =
                        table.GetColumns<StrCol, StrCol, NumCol, NumCol>(
                        "ImportDate", "Ticker", "Close", "Volume");
                    var filtered = table
                        .Where(importDate == "2017-11-01" && ticker.StartsWith("K"))
                        .Select(ticker, close, volume)
                        .Sort(volume.Descending())
                        .Head(10);
                    PrintUtils.PrintTableData(filtered);
                }
            }
        }

       private void ExampleQuery1_NotBestPractice()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var scope = workerSession.QueryScope;
                    var table = scope.HistoricalTable("LearnDeephaven", "EODTrades");
                    var (importDate, ticker, close, volume) = table.GetColumns<StrCol, StrCol, NumCol, NumCol>(
                        "ImportDate", "Ticker", "Close", "Volume");
                    using (var temp1 = table.Where(importDate == "2017-11-01" && ticker.StartsWith("K")))
                    using (var temp2 = temp1.Select(ticker, close, volume))
                    using (var temp3 = temp2.Sort(volume.Descending()))
                    using (var filtered = temp3.Head(10))
                    {
                        PrintUtils.PrintTableData(filtered);
                    }
                }
            }
        }

        private void ExampleQuery2_InnerScope()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var scope = workerSession.QueryScope;
                    var table = scope.HistoricalTable("LearnDeephaven", "EODTrades");
                    var (importDate, ticker) =
                        table.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
                    var nov1Data = table.Where(importDate == "2017-11-01");
                    for (var ch = 'A'; ch <= 'Z'; ++ch)
                    {
                        using (nov1Data.NewScope(out var innerTable))
                        {
                            var chAsString = new string(ch, 1);
                            Console.WriteLine($"Tickers starting with {chAsString}");
                            var filtered = innerTable.Where(ticker.StartsWith(chAsString))
                                .Head(10);
                            PrintUtils.PrintTableData(filtered);

                        }
                    }
                }
            }
        }

        private void ScopesAreIndependent()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var table = workerSession.QueryScope.HistoricalTable("LearnDeephaven", "EODTrades");
                    var (importDate, ticker) =
                        table.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
                    var t0 = table.Where(importDate == "2017-11-01" && ticker == "AAPL");
                    var scope1 = t0.NewScope(out var t1);
                    var scope2 = t1.NewScope(out var t2);
                    var scope3 = t2.NewScope(out var t3);
                    // The variables t0, t1, t2, and t3 all refer to the same TableHandle at the server.
                    scope2.Dispose();
                    // t2 and scope2 are invalid now, but the TableHandle is still live and can be
                    // accessed via t0, t1, or t3.
                    PrintUtils.PrintTableData(t3);
                    scope3.Dispose();
                    // t3 and scope3 are invalid now, but the TableHandle is still live and can be
                    // accessed via t0 or t1.
                    PrintUtils.PrintTableData(t1);
                    scope1.Dispose();
                    // t1 and scope1 are invalid now, but the TableHandle is still live and can be
                    // accessed via t0.
                }
            }
        }

        private void TwoWaysToDoASelect()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var table = workerSession.QueryScope.HistoricalTable("LearnDeephaven", "EODTrades");
                    var filtered1 = table.Where("ImportDate == `2017-11-01` && Ticker == `AAPL`");

                    var (importDate, ticker) =
                        table.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
                    var filtered2 = table.Where(importDate == "2017-11-01" && ticker == "AAPL");

                    PrintUtils.PrintTableData(filtered1);
                    PrintUtils.PrintTableData(filtered2);
                }
            }
        }

        private void SelectExpressions()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var table = workerSession.QueryScope.HistoricalTable("LearnDeephaven", "EODTrades");
                    var (importDate, ticker, close, volume) =
                        table.GetColumns<StrCol, StrCol, NumCol, NumCol>("ImportDate", "Ticker", "Close", "Volume");
                    var t0 = table.Where(importDate == "2017-11-01" && ticker == "AAPL");
                    var t1 = t0.Select(ticker, (close * volume).As("Result"));
                    // string literal equivalent
                    var t1_literal = t0.Select("Ticker", "Result = Close * Volume");

                    PrintUtils.PrintTableData(t1);
                    PrintUtils.PrintTableData(t1_literal);

                    var t2 = t0.Select(ticker, (ticker + "XYZ").As("Result"));
                    var t2_literal = t0.Select("Ticker", "Result = Ticker + `XYZ`");
                    PrintUtils.PrintTableData(t2);
                    PrintUtils.PrintTableData(t2_literal);
                }
            }
        }

        private void ViewExpressions()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var table = workerSession.QueryScope.HistoricalTable("LearnDeephaven", "EODTrades");
                    var (importDate, ticker, open, close) =
                        table.GetColumns<StrCol, StrCol, NumCol, NumCol>("ImportDate", "Ticker", "Open", "Close");
                    var t0 = table.Where(importDate == "2017-11-01" && ticker == "AAPL");
                    var t1_literal = t0.Select("Ticker", "AvgPrice = (Open + Close) / 2.0");
                    var t1_fluent = t0.Select(ticker, ((open + close) / 2.0).As("AvgPrice"));
                    PrintUtils.PrintTableData(t1_literal);
                    PrintUtils.PrintTableData(t1_fluent);
                }
            }
        }

        private void DropColumnsExample()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var table = workerSession.QueryScope.HistoricalTable("LearnDeephaven", "EODTrades");
                    var (importDate, ticker, open, close) =
                        table.GetColumns<StrCol, StrCol, NumCol, NumCol>("ImportDate", "Ticker", "Open", "Close");
                    var t0 = table
                        .Where(importDate == "2017-11-01" && ticker == "AAPL")
                        .Select(ticker, open, close);
                    var t1_literal = t0.DropColumns("Open");
                    var t1_fluent = t0.DropColumns(open);
                    PrintUtils.PrintTableData(t1_literal);
                    PrintUtils.PrintTableData(t1_fluent);
                }
            }
        }

        private void JoinExample()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var scope = workerSession.QueryScope;
                    var tickerValues = new StringColumnData(new[] {"AAPL", "IBM"});
                    var phoneValues = new StringColumnData(new[] {"1-800-AAA-AAPL", "1-888-IBM-XXXX"});
                    var phones = scope.TempTable(new[]
                    {
                        new ColumnDataHolder("Ticker", tickerValues),
                        new ColumnDataHolder("Phone", phoneValues)
                    });
                    var (phonesTicker, phonesNumber) =
                        phones.GetColumns<StrCol, StrCol>("Ticker", "Phone");

                    var trades = scope.HistoricalTable("LearnDeephaven", "EODTrades");
                    var (importDate, ticker) =
                        trades.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
                    var filtered = trades.Where(importDate == "2017-11-01");

                    var result_literal = filtered.InnerJoin(phones, new[] {"Ticker"}, new[] {"Phone"});
                    var result_fluent = filtered.InnerJoin(phones, new[] {ticker}, new[] {phonesNumber});
                    PrintUtils.PrintTableData(result_literal);
                    PrintUtils.PrintTableData(result_fluent);
                }
            }
        }

        private void MatchWithExample()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var scope = workerSession.QueryScope;
                    var tickerValues = new StringColumnData(new[] {"AAPL", "IBM"});
                    var phoneValues = new StringColumnData(new[] {"1-800-AAA-AAPL", "1-888-IBM-XXXX"});
                    var phones = scope.TempTable(new[]
                    {
                        new ColumnDataHolder("PhonesTicker", tickerValues),
                        new ColumnDataHolder("Phone", phoneValues)
                    });
                    var (phonesTicker, phonesNumber) =
                        phones.GetColumns<StrCol, StrCol>("PhonesTicker", "Phone");

                    var trades = scope.HistoricalTable("LearnDeephaven", "EODTrades");
                    var (importDate, ticker) =
                        trades.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
                    var filtered = trades.Where(importDate == "2017-11-01");

                    var result_literal = filtered.InnerJoin(phones, new[] {"Ticker=PhonesTicker"}, new[] {"Phone"});
                    var result_fluent = filtered.InnerJoin(phones, new[] {ticker.MatchWith(phonesTicker)},
                        new[] {phonesNumber});
                    PrintUtils.PrintTableData(result_literal);
                    PrintUtils.PrintTableData(result_fluent);
                }
            }
        }

        private void MatchWithAndAsExample()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var scope = workerSession.QueryScope;
                    var tickerValues = new StringColumnData(new[] {"AAPL", "IBM"});
                    var phoneValues = new StringColumnData(new[] {"1-800-AAA-AAPL", "1-888-IBM-XXXX"});
                    var phones = scope.TempTable(new[]
                    {
                        new ColumnDataHolder("PhonesTicker", tickerValues),
                        new ColumnDataHolder("Phone", phoneValues)
                    });
                    var (phonesTicker, phonesNumber) =
                        phones.GetColumns<StrCol, StrCol>("PhonesTicker", "Phone");

                    var trades = scope.HistoricalTable("LearnDeephaven", "EODTrades");
                    var (importDate, ticker) =
                        trades.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
                    var filtered = trades.Where(importDate == "2017-11-01");

                    var result_literal = filtered.InnerJoin(
                        phones, new[] {"Ticker=PhonesTicker"}, new[] {"AddedPhone=Phone"});
                    var result_fluent = filtered.InnerJoin(
                        phones, new[] {ticker.MatchWith(phonesTicker)}, new[] {phonesNumber.As("AddedPhone")});
                    PrintUtils.PrintTableData(result_literal);
                    PrintUtils.PrintTableData(result_fluent);
                }
            }
        }

        private void SumByExample()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var trades = workerSession.QueryScope.HistoricalTable("LearnDeephaven", "EODTrades");
                    var (importDate, ticker, volume) =
                        trades.GetColumns<StrCol, StrCol, NumCol>("ImportDate", "Ticker", "Volume");
                    var filtered = trades.Where(importDate == "2017-11-01")
                        .View(ticker, volume);
                    var result_literal = filtered.SumBy("Ticker");
                    var result_fluent = filtered.SumBy(ticker);
                    PrintUtils.PrintTableData(result_literal);
                    PrintUtils.PrintTableData(result_fluent);
                }
            }
        }

        private void ADOExample()
        {
            var builder = new DeephavenConnectionStringBuilder
            {
                Host = _host, Username = _username, Password = _password
            };

            using (var connection = new DeephavenConnection(builder))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "db.i(\"DbInternal\",\"ProcessEventLog\")"
                                          + ".where(\"Date = currentDateNy()\")"
                                          + ".head(100)";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write("\t");
                                Console.Write(reader[i]);
                            }

                            Console.WriteLine();
                        }
                    }
                }
            }
        }

        private void NormalVsFluentExpressions()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var table = workerSession.QueryScope.HistoricalTable("LearnDeephaven", "EODTrades");
                    var (importDate, ticker, close) =
                        table.GetColumns<StrCol, StrCol, NumCol>("ImportDate", "Ticker", "Close");
                    var t0 = table.Where(importDate == "2017-11-01" && ticker == "AAPL");

                    var x = 1;

                    // using a local function here, for convenience
                    int myFunc(int arg)
                    {
                        return arg + 10;
                    }

                    // Sent to server as "Result = 100 + Close"
                    var t1a = t0.Select((100 + close).As("Result"));
                    // Sent to server as "Result = 300 + Close"
                    var t2a = t0.Select((100 + 200 + close).As("Result"));
                    // Sent to server as "Result = 101 + Close"
                    var t3a = t0.Select((100 + x + close).As("Result"));
                    // Sent to server as "Result = 111 + Close"
                    var t4a = t0.Select((100 + myFunc(x) + close).As("Result"));
                    // Sent to server as "Result = Close + 100"
                    var t1b = t0.Select((close + 100).As("Result"));
                    // Sent to server as "Result = (Close + 100) + 200"
                    var t2b = t0.Select((close + 100 + 200).As("Result"));
                    // Sent to server as "Result = (Close + 100) + 1"
                    var t3b = t0.Select((close + 100 + x).As("Result"));
                    // Sent to server as "Result = (Close + 100) + 11"
                    var t4b = t0.Select((close + 100 + myFunc(x)).As("Result"));
                }
            }
        }

        private void Combinators()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var table = workerSession.QueryScope.HistoricalTable("LearnDeephaven", "EODTrades");
                    var (importDate, ticker, close) =
                        table.GetColumns<StrCol, StrCol, NumCol>("ImportDate", "Ticker", "Close");
                    var t0 = table.Where(importDate == "2017-11-01" && ticker == "AAPL");

                    // using a local function here, for convenience
                    NumericExpression add5(NumericExpression expr)
                    {
                        return expr + 5;
                    }

                    // Sent to server as "Result = Close + 5"
                    var t1 = t0.Select(add5(close).As("Result"));
                }
            }
        }

        private void StringMethods()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var table = workerSession.QueryScope.HistoricalTable("LearnDeephaven", "EODTrades");
                    var (importDate, ticker, close) =
                        table.GetColumns<StrCol, StrCol, NumCol>("ImportDate", "Ticker", "Close");
                    var t0 = table.Where(importDate == "2017-11-01");
                    var t1 = t0.Where(ticker.StartsWith("AA"));
                    var t1_literal = t0.Where("ticker.startsWith(`AA`)");
                    var t2 = t0.Where(ticker.Matches(".*P.*"));
                    var t2_literal = t0.Where("ticker.matches(`.*P.*`)");
                }
            }
        }

        private void DateTimeExamples()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var table = workerSession.QueryScope.HistoricalTable("LearnDeephaven", "EODTrades");
                    var (importDate, ticker, eodTimestamp) =
                        table.GetColumns<StrCol, StrCol, DateTimeCol>("ImportDate", "Ticker", "EODTimestamp");
                    var t0 = table.Where(importDate == "2017-11-01" && ticker == "AAPL");
                    var ts = new DBDateTime(2017, 3, 1, 13, 11, 34, 123_456_789);
                    var t1 = t0.Where(eodTimestamp > ts);
                    PrintUtils.PrintTableData(t1);
                }
            }
        }

        private void BooleanExamples()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var empty = workerSession.QueryScope.EmptyTable(5, new string[0], new string[0]);
                    var t = empty.Update(true.As("A"), false.As("B"));
                    var t_literal = empty.Update("A = true", "B = false");
                    var (a, b) = t.GetColumns<BoolCol, BoolCol>("A", "B");
                    var t2 = t.Where(a);
                    PrintUtils.PrintTableData(t2);
                    var t3 = t.Where(a && b);
                    PrintUtils.PrintTableData(t3);
                }
            }
        }

        private void WhereExample()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var aValues = new IntColumnData(new[] {10, 20, 30});
                    var sValues = new StringColumnData(new[] {"x", "y", "z"});
                    var temp = workerSession.QueryScope.TempTable(new[]
                    {
                        new ColumnDataHolder("A", aValues),
                        new ColumnDataHolder("S", sValues)
                    });
                    var a = temp.GetColumn<NumCol>("A");
                    var result = temp.Where(a > 15);
                    PrintUtils.PrintTableData(result);
                }
            }
        }

        private void TaskExample()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var ticker0 = "AAPL";
                var ticker1 = "ZNGA";

                var task0 = MakeWorkerAndFetchTable(client, ticker0);
                var task1 = MakeWorkerAndFetchTable(client, ticker1);
                var sizes = Task.WhenAll(task0, task1).Result;

                Console.WriteLine(
                    $"Tasks are finished: {ticker0} had {sizes[0]} rows, and {ticker1} had {sizes[1]} rows.");
            }
        }

        // Note async keyword
        private static async Task<long> MakeWorkerAndFetchTable(IOpenApiClient client, string tickerToFind)
        {
            var workerOptions = new WorkerOptions("Default");
            DebugUtil.Print($"[{tickerToFind}]: Starting worker");
            // Note: we are using the await keyword
            using (var workerSession = await client.StartWorkerTask(workerOptions))
            {
                var scope = workerSession.QueryScope;

                DebugUtil.Print($"[{tickerToFind}]: Getting historical table");
                var table = scope.HistoricalTable("LearnDeephaven", "EODTrades");

                DebugUtil.Print($"[{tickerToFind}]: Getting columns");
                // note await keyword
                var (importDate, ticker) = await table.GetColumnsTask<StrCol, StrCol>("ImportDate", "Ticker");
                var filtered = table.Where(importDate == "2017-11-01" && ticker == tickerToFind);

                DebugUtil.Print($"[{tickerToFind}]: Getting table data");
                // note await keyword
                var tableData = await filtered.GetTableDataTask();
                return tableData.Rows;
            }
        }

        private void SortExample1()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var scope = workerSession.QueryScope;
                    var table = scope.HistoricalTable("LearnDeephaven", "EODTrades");

                    var filtered1 = table
                        .Where("ImportDate == `2017-11-01` && Ticker.startsWith(`K`)")
                        .Select("Ticker", "Close", "Volume")
                        .Sort("Volume")
                        .SortDescending("Ticker");

                    var (importDate, ticker, close, volume) = table.GetColumns<StrCol, StrCol, NumCol, NumCol>(
                        "ImportDate", "Ticker", "Close", "Volume");
                    var filtered2 = table
                        .Where(importDate == "2017-11-01" && ticker.StartsWith("K"))
                        .Select(ticker, close, volume)
                        .Sort(volume)
                        .SortDescending(ticker);

                    PrintUtils.PrintTableData(filtered1);
                    PrintUtils.PrintTableData(filtered2);
                }
            }
        }

        private void SortExample2()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var scope = workerSession.QueryScope;
                    var table = scope.HistoricalTable("LearnDeephaven", "EODTrades");

                    var filtered1 = table
                        .Where("ImportDate == `2017-11-01` && Ticker.startsWith(`K`)")
                        .Select("Ticker", "Close", "Volume")
                        .Sort(SortPair.Ascending("Ticker"), SortPair.Descending("Volume"));

                    var (importDate, ticker, close, volume) = table.GetColumns<StrCol, StrCol, NumCol, NumCol>(
                        "ImportDate", "Ticker", "Close", "Volume");
                    var filtered2 = table
                        .Where(importDate == "2017-11-01" && ticker.StartsWith("K"))
                        .Select(ticker, close, volume)
                        .Sort(ticker.Ascending(), volume.Descending());

                    PrintUtils.PrintTableData(filtered1);
                    PrintUtils.PrintTableData(filtered2);
                }
            }
        }

        private void AggregateExample()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var table = workerSession.QueryScope.HistoricalTable(
                        DemoConstants.HistoricalNamespace, DemoConstants.HistoricalTable);
                    var (ticker, close, volume) =
                        table.GetColumns<StrCol, NumCol, NumCol>(
                            "Ticker", "Close", "Volume");

                    var aggTableLiteral = table.View("Ticker", "Close", "Volume")
                        .By(AggregateCombo.Create(
                                Aggregate.Min("Low=Close"),
                                Aggregate.Max("High=Close"),
                                Aggregate.Sum("TotalVolume=Volume"),
                                Aggregate.Count("Days")),
                            "Ticker");

                    var aggTableFluent = table.View(ticker, close, volume)
                        .By(AggCombo(
                                AggMin(close.As("Low")),
                                AggMax(close.As("High")) ,
                                AggSum(volume.As("TotalVolume")),
                                AggCount("Days")),
                            "Ticker");

                    PrintUtils.PrintTableData(aggTableLiteral);
                    PrintUtils.PrintTableData(aggTableFluent);
                }
            }
        }

        private void AccessingColumnData()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession = client.StartWorker(workerOptions))
                {
                    var table = workerSession.QueryScope.HistoricalTable("LearnDeephaven", "EODTrades");
                    var (importDate, ticker) =
                        table.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
                    var ten = table.Where(importDate == "2017-11-01" && ticker == "AAPL").Head(10);

                    PrintTableTata(ten.GetTableData());
                }
            }
        }

        private static void PrintTableTata(ITableData table)
        {
            var columnData = table.ColumnData;
            for (var row = 0; row < table.Rows; ++row)
            {
                var separator = "";
                foreach (var col in columnData)
                {
                    var o = col.GetObject(row);
                    var humanReadable = o != null ? o.ToString() : "(null)";
                    Console.Write($"{separator}{humanReadable}");
                    separator = ",";
                }
                Console.WriteLine();
            }
        }

        private void MixWorkersAndPQs()
        {
            using (var client = OpenApi.Connect(_host))
            {
                client.Login(_username, _password, _operateAs);
                var workerOptions = new WorkerOptions("Default");
                using (var workerSession1 = client.StartWorker(workerOptions))
                using (var pqSession1 = client.AttachWorkerByName("GetSomeDataQuery"))
                using (var workerSession2 = client.StartWorker(workerOptions))
                {
                    var table1 = workerSession1.QueryScope.HistoricalTable("LearnDeephaven", "EODTrades");
                    var (importDate1, ticker1) =
                        table1.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
                    var topTen = table1.Where(importDate1 == "2017-11-01" && ticker1 == "AAPL").Head(10);

                    var scope = pqSession1.QueryScope;
                    var pqTable1 = scope.BoundTable("query");
                    var def = pqTable1.LowLevel.InitialTableDefinition;

                    var table2 = workerSession2.QueryScope.HistoricalTable("LearnDeephaven", "EODTrades");
                    var (importDate2, ticker2) =
                        table2.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
                    var bottomTen = table2.Where(importDate2 == "2017-11-01" && ticker2 == "XOM").Tail(10);

                    PrintTableTata(topTen.GetTableData());

                    Console.WriteLine();
                    Console.WriteLine($"There are {def.Columns.Length} columns");
                    foreach (var col in def.Columns)
                    {
                        Console.WriteLine($"{col.Name} {col.Type}");
                    }

                    PrintTableTata(bottomTen.GetTableData());

                    Console.WriteLine("Completed");
                }
            }
        }
    }
}
