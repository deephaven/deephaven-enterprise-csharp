/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using System.Linq;
using Deephaven.OpenAPI.Shared.Batch.Aggregates;

namespace Deephaven.OpenAPI.Client.Internal.TableOperations
{
    internal static class ApplyAggregatesOperation
    {
        public static class Strategy
        {
            public const string Default = "DEFAULT";
            public const string Linear = "LINEAR";
            public const string LinearGroups = "LINEAR_GROUPS";
            public const string UseExistingGroups = "USE_EXISTING_GROUPS";
            public const string CreateGroups = "CREATE_GROUPS";
            public const string UseExistingGroupsLinearRefresh = "USE_EXISTING_GROUPS_LINEAR_REFRESH";
            public const string CreateGroupsLinearRefresh = "CREATE_GROUPS_LINEAR_REFRESH";
            public const string UseExistingGroupsLinearGroupRefresh = "USE_EXISTING_GROUPS_LINEAR_GROUP_REFRESH";
            public const string CreateGroupsLinearGroupRefresh = "CREATE_GROUPS_LINEAR_GROUP_REFRESH";
        }

        public static class AggType
        {
            public const string Count = "Count";
            public const string Min = "Min";
            public const string Max = "Max";
            public const string Sum = "Sum";
            public const string AbsSum = "AbsSum";
            public const string Var = "Var";
            public const string Avg = "Avg";
            public const string Std = "Std";
            public const string First = "First";
            public const string Last = "Last";
            public const string Array = "Array";
            public const string Percentile = "Percentile";
            public const string WeightedAvg = "WeightedAvg";
            public const string Median = "Median";
        }

        public static void Invoke(TableState parentState, TableStateBuilder childBuilder,
            string aggregateStrategy, AggregateDescriptor aggregateDescriptor, string[] groupByColumns)
        {
            Invoke(parentState, childBuilder, aggregateStrategy, new[] {aggregateDescriptor}, groupByColumns);
        }

        public static void Invoke(TableState parentState, TableStateBuilder childBuilder,
            string aggregateStrategy, AggregateCombo comboAggregate, string[] groupByColumns)
        {
            var descriptors = comboAggregate.Aggregates.Select(a => a.Descriptor).ToArray();
            Invoke(parentState, childBuilder, aggregateStrategy, descriptors, groupByColumns);
        }

        private static void Invoke(TableState parentState, TableStateBuilder childBuilder,
            string aggregateStrategy, AggregateDescriptor[] descriptors, string[] groupByColumns)
        {
            var combo = new ComboAggregateDescriptor
            {
                AggregateStrategy = aggregateStrategy,
                Aggregates = descriptors,
                GroupByColumns = groupByColumns
            };
            parentState.Context.InvokeServerForItd(new[]{parentState}, childBuilder,
                (ws, sa, fa) => ws.ComboAggregateAsync(parentState.TableHandle, childBuilder.TableHandle, combo, sa, fa, fa));
        }
    }
}
