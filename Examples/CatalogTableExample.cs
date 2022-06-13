/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Client;

namespace Examples
{
    public static class CatalogTableExample
    {
        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
            PrintUtils.PrintTableData(scope.CatalogTable());
        }
    }
}
