/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Deephaven.OpenAPI.Client;
using Deephaven.OpenAPI.Client.Data;

namespace Examples
{
    public static class ValidationExample
    {
        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
            TestWheres(scope);
            TestSelects(scope);
        }

        private static void TestWheres(IQueryScope scope)
        {
            var badWheres = new[]
            {
                "X > 3)", // syntax error
                "S = new String(`hello`)", // new not allowed
                "S = java.util.regex.Pattern.quote(S)", // Pattern.quote not on whitelist
                "X = Math.min(3, 4)" // Math.min not on whitelist
            };
            var badWheresWhenDynamic = new[]
            {
                "X = i", // clients can't use i on dynamic tables
                "X = ii", // clients can't use ii on dynamic tables
            };
            var goodWheres = new[]
            {
                "X = 3",
                "S = `hello`",
                "S.length() = 17", // instance methods of String ok
                "X = min(3, 4)", // "builtin" from GroovyStaticImports
                "X = isNormal(3)", // another builtin from GroovyStaticImports
                "X in 3, 4, 5",
            };

            using (var newScope = scope.NewScope())
            {
                var staticTable = newScope.EmptyTable(10, new string[0], new string[0])
                    .Update("X = 12", "S = `hello`");
                // "badWheresWhenDynamic" are ok for static tables
                TestWheresHelper("static table", staticTable, badWheres, goodWheres.Concat(badWheresWhenDynamic));

                var dynamicTable = newScope.TimeTable(DateTime.Now, TimeSpan.FromSeconds(10))
                    .Update("X = 12", "S = `hello`")
                    .Preemptive(100);
                // "badWheresWhenDynamic" are bad for dynamic tables
                TestWheresHelper("dynamic table", dynamicTable, badWheres.Concat(badWheresWhenDynamic), goodWheres);
            }
        }

        private static void TestWheresHelper(string what, IQueryTable table, IEnumerable<string> badWheres,
            IEnumerable<string> goodWheres)
        {
            foreach (var bw in badWheres)
            {
                try
                {
                    _ = table.Where(bw).GetTableData();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{what}: {bw}: Failed as expected with: {e.Message}");
                    continue;
                }

                throw new Exception($"{what}: {bw}: Expected to fail, but succeeded");
            }

            foreach (var gw in goodWheres)
            {
                table.Where(gw).GetTableData();
                Console.WriteLine($"{what}: {gw}: succeeded as expected");
            }
        }

        private static void TestSelects(IQueryScope scope)
        {
            var badSelects = new []
            {
                new [] {"X = 3)"}, // syntax error
                new [] {"S = new String(`hello`)"}, // new not allowed
                new [] {"S = `hello`", "T = java.util.regex.Pattern.quote(S)"}, // Pattern.quote not on whitelist
                new [] {"X = Math.min(3, 4)"} // Math.min not on whitelist
            };
            var badSelectsWhenDynamic = new[]
            {
                new [] {"X = i"}, // clients can't use i on dynamic tables
                new [] {"X = ii"} // clients can't use ii on dynamic tables
            };
            var goodSelects = new []
            {
                new [] {"X = 3"},
                new [] {"S = `hello`", "T = S.length()"}, // instance methods of String ok
                new [] {"X = min(3, 4)"}, // "builtin" from GroovyStaticImports
                new [] {"X = isNormal(3)"}, // another builtin from GroovyStaticImports
            };
            using (var newScope = scope.NewScope())
            {
                var staticTable = newScope.EmptyTable(10, new string[0], new string[0])
                    .Update("X = 12", "S = `hello`");
                // "badSelectsWhenDynamic" are ok for static tables
                TestSelectsHelper("static table", staticTable, badSelects, goodSelects.Concat(badSelectsWhenDynamic));

                var dynamicTable = newScope.TimeTable(DateTime.Now, TimeSpan.FromSeconds(10))
                    .Update("X = 12", "S = `hello`")
                    .Preemptive(100);
                // "badWheresWhenDynamic" are bad for dynamic tables
                TestSelectsHelper("dynamic table", dynamicTable, badSelects.Concat(badSelectsWhenDynamic), goodSelects);

            }
        }

        private static void TestSelectsHelper(string what, IQueryTable table, IEnumerable<string[]> badSelects,
            IEnumerable<string[]> goodSelects)
        {
            foreach (var bs in badSelects)
            {
                var selection = string.Join(", ", bs);
                try
                {
                    _ = table.Select(bs).GetTableData();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{what}: {selection}: Failed as expected with: {e.Message}");
                    continue;
                }
                throw new Exception($"{what}: {selection}: Expected to fail, but succeeded");
            }

            foreach (var gs in goodSelects)
            {
                var selection = string.Join(", ", gs);
                _ = table.Select(gs).GetTableData();
                Console.WriteLine($"{what}: {selection}: succeded as expected");
            }
        }
    }
}
