/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Client;
using Deephaven.OpenAPI.Shared.Data;

namespace Examples
{
    public static class CatalogExample
    {
        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
            using (var newScope = scope.NewScope())
            {
                var catalog = newScope.GetDatabaseCatalog();
                Console.WriteLine("Catalog of all tables:");
                foreach (var catalogTable in catalog.Tables)
                {
                    Console.WriteLine($"{catalogTable.Namespace}.{catalogTable.TableName}");
                }

                catalog = newScope.GetDatabaseCatalog(true, true, "^F.*", null);
                Console.WriteLine("Catalog of all tables in namespaces starting with \"F\": ");
                foreach (var catalogTable in catalog.Tables)
                {
                    Console.WriteLine($"{catalogTable.Namespace}.{catalogTable.TableName}");
                }
            }
        }
    }
}
