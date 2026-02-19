/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Shared.Batch;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client.Internal.TableOperations
{
    internal static class CustomColumnsTableOperation
    {
        public static void Invoke(TableState parentState, TableStateBuilder childBuilder,
            CustomColumnDescriptor[] customColumns)
        {
            var op = new BatchTableRequest.BatchTableRequestSerializedTableOps
            {
                Handles = new HandleMapping(parentState.TableHandle, childBuilder.TableHandle),
                CustomColumns = customColumns
            };
            BatchUtil.InvokeBatchOperation(parentState, childBuilder, op);
        }
    }
}
