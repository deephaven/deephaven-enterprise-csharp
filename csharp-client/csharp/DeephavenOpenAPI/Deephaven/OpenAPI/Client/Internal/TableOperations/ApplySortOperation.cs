/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System.Linq;
using Deephaven.OpenAPI.Shared.Batch;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client.Internal.TableOperations
{
    internal static class ApplySortOperation
    {
        public static void Invoke(TableState parentState, TableStateBuilder childBuilder, SortPair[] sorts)
        {
            var descriptors = sorts.Select(s => new SortDescriptor
            {
                ColumnName = s.Column,
                Abs = s.Abs,
                Dir = s.Direction == SortDirection.Ascending
                    ? "asc"
                    : "desc"
            }).ToArray();
            var op = new BatchTableRequest.BatchTableRequestSerializedTableOps
            {
                Handles = new HandleMapping(parentState.TableHandle, childBuilder.TableHandle),
                Sorts = descriptors,
            };
            BatchUtil.InvokeBatchOperation(parentState, childBuilder, op);
        }
    }
}
