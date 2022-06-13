/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Shared.Batch;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client.Internal.TableOperations
{
    internal static class FlattenOperation
    {
        public static void Invoke(TableState parentState, TableStateBuilder childBuilder)
        {
            var op = new BatchTableRequest.BatchTableRequestSerializedTableOps
            {
                Handles = new HandleMapping(parentState.TableHandle, childBuilder.TableHandle),
                IsFlat = true,
            };
            BatchUtil.InvokeBatchOperation(parentState, childBuilder, op);
        }
    }
}
