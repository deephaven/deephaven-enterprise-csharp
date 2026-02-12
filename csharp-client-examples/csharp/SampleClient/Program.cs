using Deephaven.OpenAPI.Shared.Data;
using System;
using System.Collections;
using System.Linq;

using Range = Deephaven.OpenAPI.Shared.Data.Range;

namespace SampleClient
{
    /// <summary>
    /// A simple interactive program for exercising the Open API.
    ///
    /// There are options to connect to running workers (persistent queries),
    /// attach (fetch) to tables and get data from them.
    ///
    /// For simplicity only a single worker connection and fetched table is
    /// supported here (although the API clearly permits any number of
    /// concurrent worker connections and table fetches).
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if(args.Length != 3)
                {
                    Console.WriteLine("usage: SampleClient <url> <username> <password>");
                    Console.WriteLine("url is in the form wss://{host}:{port}/socket");
                    Console.WriteLine("port is typically 8123");
                    return;
                }

                // enclosed in using block so we disconnect when done
                using (var client = new OpenAPIClient(args[0], args[1], args[2]))
                {
                    WorkerConnection workerConnection = null;
                    TableHandle tableHandle = null;
                    while (true)
                    {
                        Console.Out.WriteLine("Enter an instruction:");

                        var line = Console.In.ReadLine();
                        if (line == null || line.Equals("exit"))
                        {
                            break;
                        }
                        // ask the server for a connect token (required for new worker connection)
                        
                        if (line.Equals("rct"))
                        {
                            client.RefreshConnectToken();
                            continue;
                        }

                        // open a connection to a specific running worker/persistent query
                        if (line.StartsWith("o "))
                        {
                            if (workerConnection == null)
                            {
                                if (client.LastConnectToken != null)
                                {
                                    var name = line.Substring(2).Trim();
                                    var wrapper = client.GetQueryStatusWrapper(name);
                                    if (wrapper != null)
                                    {
                                        workerConnection = new WorkerConnection(wrapper, client.LastConnectToken);
                                    }
                                    else
                                    {
                                        Console.WriteLine("No query config with name: " + name);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("No connect token!");
                                }
                            }
                            continue;
                        }

                        // close an open worker connection, if there is one
                        if (line.StartsWith("c"))
                        {
                            if (workerConnection != null)
                            {
                                Console.WriteLine("Closing open worker connection.");
                                workerConnection.Dispose();
                                workerConnection = null;
                            }
                            else
                            {
                                Console.WriteLine("No worker to close!");
                            }
                            continue;
                        }

                        // fetch a table by name from the connected worker
                        if (line.StartsWith("t "))
                        {
                            if (workerConnection != null)
                            {
                                var tableName = line.Substring(2).Trim();
                                Console.WriteLine("Fetching table with name: " + tableName);
                                tableHandle = workerConnection.FetchTable(tableName);
                            }
                            else
                            {
                                Console.WriteLine("No worker to fetch table from!");
                            }

                            continue;
                        }

                        // subscribe to fetched table
                        if (line.Equals("s"))
                        {
                            if (workerConnection != null)
                            {
                                if (tableHandle != null)
                                {
                                    Console.WriteLine("Subscribing to fetched table.");
                                    workerConnection.Subscribe(tableHandle);
                                }
                                else
                                {
                                    Console.WriteLine("No table fetched!");
                                }
                            }
                            else
                            {
                                Console.WriteLine("No worker connection!");
                            }
                            continue;
                        }

                        // unsubscribe from the fetched table
                        if (line.Equals("u"))
                        {
                            if (workerConnection != null)
                            {
                                Console.WriteLine("Unsubscribing from fetched table.");
                                if (tableHandle != null)
                                {
                                    workerConnection.Unsubscribe(tableHandle);
                                }
                                else
                                {
                                    Console.WriteLine("No table fetched!");
                                }
                            }
                            else
                            {
                                Console.WriteLine("No worker connection!");
                            }
                            continue;
                        }

                        // get a snapshot of the specified rows from the fetched table
                        if (line.StartsWith("snap "))
                        {
                            if (workerConnection != null)
                            {
                                if (tableHandle != null)
                                {
                                    var rows = new RangeSet();
                                    var tokens = line.Substring(5).Trim().Split(' ');
                                    if (tokens.Length < 2)
                                    {
                                        Console.WriteLine("usage: snap <first> <last> [<column>...]");
                                        continue;
                                    }

                                    var first = Int64.Parse(tokens[0]);
                                    var last = Int64.Parse(tokens[1]);
                                    rows.AddRange(new Range(first, last));
                                    var initialTableDefinition = workerConnection.GetTableDefinition(tableHandle);
                                    var columnDefinitions = initialTableDefinition.Columns;
                                    var columns = new BitArray(columnDefinitions.Length);
                                    if (tokens.Length > 2)
                                    {
                                        for (var i = 0; i < columnDefinitions.Length; i++)
                                        {
                                            var columnDefinition = columnDefinitions[i];
                                            if (tokens.Contains(columnDefinition.Name))
                                            {
                                                columns.Set(i, true);
                                            }
                                            else
                                            {
                                                columns.Set(i, false);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        columns.SetAll(true);
                                    }
                                    workerConnection.ConstructTableSnapshot(tableHandle, rows, columns);
                                }
                                else
                                {
                                    Console.WriteLine("No table fetched!");
                                }
                            }
                            else
                            {
                                Console.WriteLine("No worker connection!");
                            }
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
    }
}
