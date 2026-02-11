/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Shared.Batch;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client.Internal.TableOperations
{
    internal static class HeadOrTailOperation
    {
        public static void Invoke(TableState parentState, TableStateBuilder childBuilder, bool head, long rows)
        {
            var op = new BatchTableRequest.BatchTableRequestSerializedTableOps
            {
                Handles = new HandleMapping(parentState.TableHandle, childBuilder.TableHandle),
                HeadOrTail = new HeadOrTailDescriptor {Head = head, Rows = rows},
            };
            BatchUtil.InvokeBatchOperation(parentState, childBuilder, op);
        }
    }
}
