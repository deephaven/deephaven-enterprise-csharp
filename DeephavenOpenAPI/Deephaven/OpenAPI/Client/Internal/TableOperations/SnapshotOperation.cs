/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using System.Linq;

namespace Deephaven.OpenAPI.Client.Internal.TableOperations
{
    internal static class SnapshotOperation
    {
        // In groovy, _rightTableQueryable is the "left hand side" table (drives the snapshot/sample frequency)
        // Here we put it on the right side as an argument to the snapshot function because it we want to permit null,
        // which will produce a single one-time snapshot.
        public static void Invoke(TableState parentState, TableStateBuilder childBuilder,
            TableState timeTableState, bool doInitialSnapshot, string[] stampColumns)
        {
            // SnapshotAsync doesn't like nulls for stampColumns
            stampColumns = stampColumns ?? new string[0];

            var dependentStates = new[] {parentState, timeTableState}.Where(s => s != null).ToArray();
            parentState.Context.InvokeServerForItd(dependentStates, childBuilder,
                (ws, sa, fa) =>
                {
                    ws.SnapshotAsync(timeTableState?.TableHandle, parentState.TableHandle,
                        childBuilder.TableHandle, doInitialSnapshot, stampColumns, sa, fa, fa);
                });
        }
    }
}
