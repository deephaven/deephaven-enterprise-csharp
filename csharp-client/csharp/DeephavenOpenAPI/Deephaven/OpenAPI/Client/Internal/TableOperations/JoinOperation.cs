/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client.Internal.TableOperations
{
    internal static class JoinOperation
    {
        public static void Invoke(TableState parentState, TableStateBuilder childBuilder,
            JoinDescriptor.JoinDescriptorJoinType joinType, TableState rightSideState,
            string[] columnsToMatch, string[] columnsToAdd)
        {
            var joinDescriptor = new JoinDescriptor
            {
                JoinType = joinType,
                LeftTableHandle = parentState.TableHandle,
                RightTableHandle = rightSideState.TableHandle,
                ColumnsToMatch = columnsToMatch,
                ColumnsToAdd = columnsToAdd
            };
            parentState.Context.InvokeServerForItd(new []{parentState, rightSideState}, childBuilder,
                (ws, sa, fa) => ws.JoinAsync(joinDescriptor, childBuilder.TableHandle, sa, fa, fa));
        }
    }
}
