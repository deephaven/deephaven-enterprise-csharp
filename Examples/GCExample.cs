/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Linq;
using System.Threading.Tasks;
using Deephaven.OpenAPI.Client;
using Deephaven.OpenAPI.Client.Data;
using Deephaven.OpenAPI.Client.Fluent;

namespace Examples
{
    public static class GCExample
    {
        /// <summary>
        /// Extremely inefficient code that tries to create a lot of tables and garbage collects
        /// them at the right time (releasing their TableHandles) but hopefully not prematurely.
        /// </summary>
        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
            // Try to issue multiple requests in parallel for extra drama.
            const int numValues = 100;
            const int parallelism = 4;
            if ((numValues % parallelism) != 0)
            {
                throw new Exception($"{parallelism} needs to evenly divide {numValues}");
            }

            var indexVals = Enumerable.Range(0, numValues).ToArray();
            var dataVals = Enumerable.Range(0, numValues).Select(x => -x).ToArray();
            using (var srcTable = scope.TempTable(new[]
            {
                new ColumnDataHolder("Index", new IntColumnData(indexVals)),
                new ColumnDataHolder("Data", new IntColumnData(dataVals)),
            }))
            {
                PrintUtils.PrintTableData(srcTable);

                var (index, data) = srcTable.GetColumns<NumCol, NumCol>("Index", "Data");

                var total = 0;
                for (var i = 0; i < numValues; i += parallelism)
                {
                    var tables = new IQueryTable[parallelism];
                    for (var j = 0; j < parallelism; ++j)
                    {
                        tables[j] = srcTable.Where(index == i + j).Select((-data).As("Result")).Head(1);
                    }

                    var tasks = tables.Select(t => t.GetTableDataTask()).ToArray();
                    Task.WaitAll(tasks);

                    var sum = tasks.Select(t => t.Result.ColumnData[0].GetInt32(0)).Sum();
                    total += sum;
                    Console.WriteLine($"After iteration {i}, sum is {sum}, total is {total}");
                }

                var expected = (numValues - 1) * numValues / 2;
                if (expected != total)
                {
                    throw new Exception($"Expected is {expected}, but actual is {total}");
                }
            }
        }
    }
}
