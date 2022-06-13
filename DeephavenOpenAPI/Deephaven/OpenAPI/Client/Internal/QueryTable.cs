/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Linq;
using System.Threading.Tasks;
using Deephaven.OpenAPI.Client.Fluent;
using Deephaven.OpenAPI.Client.Internal.TableOperations;
using Deephaven.OpenAPI.Shared.Batch.Aggregates;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client.Internal
{
    internal sealed class QueryTable : IQueryTable, ILowLevel
    {
        public IQueryScope Scope { get; }
        private readonly TableStateTracker _tracker;

        public event QueryTableUpdateHandler OnTableUpdate;
        public event QueryTableSnapshotHandler OnTableSnapshot;

        public TableState TableState => _tracker.TableState;

        public static QueryTable Create(IQueryScope scope, TableStateTracker tracker)
        {
            return new QueryTable(scope, tracker);
        }

        private QueryTable(IQueryScope scope, TableStateTracker tracker)
        {
            Scope = scope;
            _tracker = tracker;
            TableState.OnTableUpdate += InvokeOnTableUpdate;
            TableState.OnTableSnapshot += InvokeOnTableSnapshot;
        }

        public void Dispose()
        {
            TableState.OnTableUpdate -= InvokeOnTableUpdate;
            TableState.OnTableSnapshot -= InvokeOnTableSnapshot;
            _tracker.Dispose();
        }

        private void InvokeOnTableUpdate(ITableUpdate update)
        {
            OnTableUpdate?.Invoke(this, update);
        }

        private void InvokeOnTableSnapshot(ITableSnapshot snapshot)
        {
            OnTableSnapshot?.Invoke(this, snapshot);
        }

        public IQueryScope NewScope(out IQueryTable selfInNewScope)
        {
            var newScope = Scope.NewScope();
            selfInNewScope = newScope.Manage(this);
            return newScope;
        }

        private IQueryTable DefaultAggregateByDescriptor(AggregateDescriptor descriptor,
            params string[] groupByColumns)
        {
            var rb = MakeResultBuilder();
            ApplyAggregatesOperation.Invoke(TableState, rb.ResultTsBuilder,
                descriptor,
                groupByColumns);
            return rb.ResultTable;
        }

        private IQueryTable DefaultAggregateByType(string aggregateType, params string[] groupByColumns)
        {
            var descriptor = new AggregateDescriptor
            {
                AggregateType = aggregateType
            };
            return DefaultAggregateByDescriptor(descriptor, groupByColumns);
        }

        public IQueryTable AbsSumBy(params string[] groupByColumns)
        {
            return DefaultAggregateByType(ApplyAggregatesOperation.AggType.AbsSum, groupByColumns);
        }

        public IQueryTable By(params string[] groupByColumns)
        {
            return DefaultAggregateByType(ApplyAggregatesOperation.AggType.Array, groupByColumns);
        }

        public IQueryTable AvgBy(params string[] columns)
        {
            return DefaultAggregateByType(ApplyAggregatesOperation.AggType.Avg, columns);
        }

        public IQueryTable CountBy(string resultColumn, params string[] groupByColumns)
        {
            var descriptor = new AggregateDescriptor
            {
                AggregateType = ApplyAggregatesOperation.AggType.Count,
                ColumnName = resultColumn
            };
            return DefaultAggregateByDescriptor(descriptor, groupByColumns);
        }

        public IQueryTable FirstBy(params string[] groupByColumns)
        {
            return DefaultAggregateByType(ApplyAggregatesOperation.AggType.First, groupByColumns);
        }

        public IQueryTable LastBy(params string[] groupByColumns)
        {
            return DefaultAggregateByType(ApplyAggregatesOperation.AggType.Last, groupByColumns);
        }

        public IQueryTable MinBy(params string[] groupByColumns)
        {
            return DefaultAggregateByType(ApplyAggregatesOperation.AggType.Min, groupByColumns);
        }

        public IQueryTable MaxBy(params string[] groupByColumns)
        {
            return DefaultAggregateByType(ApplyAggregatesOperation.AggType.Max, groupByColumns);
        }

        public IQueryTable MedianBy(params string[] groupByColumns)
        {
            return DefaultAggregateByType(ApplyAggregatesOperation.AggType.Median, groupByColumns);
        }

        public IQueryTable PercentileBy(double percentile, bool avgMedian, params string[] groupByColumns)
        {
            var descriptor = new AggregateDescriptor
            {
                AggregateType = ApplyAggregatesOperation.AggType.Percentile,
                Percentile = new PercentileDescriptor
                {
                    Percentile = percentile,
                    AvgMedian = avgMedian
                }
            };
            return DefaultAggregateByDescriptor(descriptor, groupByColumns);
        }

        public IQueryTable PercentileBy(double percentile, params string[] groupByColumns)
        {
            return PercentileBy(percentile, false, groupByColumns);
        }

        public IQueryTable SumBy(params string[] groupByColumns)
        {
            return DefaultAggregateByType(ApplyAggregatesOperation.AggType.Sum, groupByColumns);
        }

        public IQueryTable VarBy(params string[] groupByColumns)
        {
            return DefaultAggregateByType(ApplyAggregatesOperation.AggType.Var, groupByColumns);
        }

        public IQueryTable StdBy(params string[] groupByColumns)
        {
            return DefaultAggregateByType(ApplyAggregatesOperation.AggType.Std, groupByColumns);
        }

        public IQueryTable WAvgBy(string weightColumn, params string[] groupByColumns)
        {
            var descriptor = new AggregateDescriptor
            {
                AggregateType = ApplyAggregatesOperation.AggType.WeightedAvg,
                ColumnName = weightColumn
            };
            return DefaultAggregateByDescriptor(descriptor, groupByColumns);
        }

        public IQueryTable By(AggregateCombo comboAggregate, params string[] groupByColumns)
        {
            var rb = MakeResultBuilder();
            ApplyAggregatesOperation.Invoke(TableState, rb.ResultTsBuilder,
                comboAggregate,
                groupByColumns);
            return rb.ResultTable;
        }

        private IQueryTable InternalJoin(JoinDescriptor.JoinDescriptorJoinType joinType,
            IQueryTable rightSide, string[] columnsToMatch, string[] columnsToAdd)
        {
            var rb = MakeResultBuilder();
            JoinOperation.Invoke(TableState, rb.ResultTsBuilder, joinType, rightSide.TableState,
                columnsToMatch, columnsToAdd);
            return rb.ResultTable;
        }

        private IQueryTable InternalJoin(JoinDescriptor.JoinDescriptorJoinType joinType,
            IQueryTable rightTableHandle, string columnsToMatchStr, string columnsToAddStr)
        {
            var columnsToMatch = columnsToMatchStr.Split(',');
            var columnsToAdd = columnsToAddStr.Split(',');
            return InternalJoin(joinType, rightTableHandle, columnsToMatch, columnsToAdd);
        }

        public IQueryTable InnerJoin(IQueryTable rhs, string[] columnsToMatch, string[] columnsToAdd)
        {
            return InternalJoin(JoinDescriptor.JoinDescriptorJoinType.Join, rhs, columnsToMatch,
                columnsToAdd);
        }

        public IQueryTable InnerJoin(IQueryTable rhs, string columnsToMatch, string columnsToAdd)
        {
            return InternalJoin(JoinDescriptor.JoinDescriptorJoinType.Join,
                rhs, columnsToMatch, columnsToAdd);
        }

        public IQueryTable InnerJoin(IQueryTable rhs, string columnsToMatch)
        {
            return InternalJoin(JoinDescriptor.JoinDescriptorJoinType.Join,
                rhs, columnsToMatch, null);
        }

        public IQueryTable NaturalJoin(IQueryTable rhs, string[] columnsToMatch, string[] columnsToAdd)
        {
            return InternalJoin(JoinDescriptor.JoinDescriptorJoinType.Natural, rhs, columnsToMatch,
                columnsToAdd);
        }

        public IQueryTable NaturalJoin(IQueryTable rhs, string columnsToMatch, string columnsToAdd)
        {
            return InternalJoin(JoinDescriptor.JoinDescriptorJoinType.Natural,
                rhs, columnsToMatch, columnsToAdd);
        }

        public IQueryTable NaturalJoin(IQueryTable rhs, string columnsToMatch)
        {
            return InternalJoin(JoinDescriptor.JoinDescriptorJoinType.Natural,
                rhs, columnsToMatch, null);
        }

        public IQueryTable NaturalJoin(IQueryTable rhs, IColumn[] columnsToMatch, ISelectColumn[] columnsToAdd)
        {
            var cmStrings = columnsToMatch.Select(c => c.Name).ToArray();
            var caStrings = columnsToAdd.Select(c => c.ToIrisRepresentation()).ToArray();

            return InternalJoin(JoinDescriptor.JoinDescriptorJoinType.Natural, rhs, cmStrings,
                caStrings);
        }


        public IQueryTable ExactJoin(IQueryTable rhs, string[] columnsToMatch, string[] columnsToAdd)
        {
            return InternalJoin(JoinDescriptor.JoinDescriptorJoinType.ExactJoin, rhs, columnsToMatch,
                columnsToAdd);
        }

        public IQueryTable ExactJoin(IQueryTable rhs, string columnsToMatch, string columnsToAdd)
        {
            return InternalJoin(JoinDescriptor.JoinDescriptorJoinType.ExactJoin,
                rhs, columnsToMatch, columnsToAdd);
        }

        public IQueryTable ExactJoin(IQueryTable rhs, string columnsToMatch)
        {
            return InternalJoin(JoinDescriptor.JoinDescriptorJoinType.ExactJoin,
                rhs, columnsToMatch, null);
        }

        public IQueryTable LeftJoin(IQueryTable rhs, string[] columnsToMatch, string[] columnsToAdd)
        {
            return InternalJoin(JoinDescriptor.JoinDescriptorJoinType.LeftJoin, rhs, columnsToMatch,
                columnsToAdd);
        }

        public IQueryTable LeftJoin(IQueryTable rhs, string columnsToMatch, string columnsToAdd)
        {
            return InternalJoin(JoinDescriptor.JoinDescriptorJoinType.LeftJoin,
                rhs, columnsToMatch, columnsToAdd);
        }

        public IQueryTable LeftJoin(IQueryTable rhs, string columnsToMatch)
        {
            return InternalJoin(JoinDescriptor.JoinDescriptorJoinType.LeftJoin,
                rhs, columnsToMatch, null);
        }

        public IQueryTable AsOfJoin(IQueryTable rhs, string[] columnsToMatch, string[] columnsToAdd)
        {
            return InternalJoin(JoinDescriptor.JoinDescriptorJoinType.AJ, rhs, columnsToMatch,
                columnsToAdd);
        }

        public IQueryTable AsOfJoin(IQueryTable rhs, string columnsToMatch, string columnsToAdd)
        {
            return InternalJoin(JoinDescriptor.JoinDescriptorJoinType.AJ,
                rhs, columnsToMatch, columnsToAdd);
        }

        public IQueryTable AsOfJoin(IQueryTable rhs, string columnsToMatch)
        {
            return InternalJoin(JoinDescriptor.JoinDescriptorJoinType.AJ,
                rhs, columnsToMatch, null);
        }

        public IQueryTable ReverseAsOfJoin(IQueryTable rhs, string[] columnsToMatch, string[] columnsToAdd)
        {
            return InternalJoin(JoinDescriptor.JoinDescriptorJoinType.ReverseAJ, rhs, columnsToMatch,
                columnsToAdd);
        }

        public IQueryTable ReverseAsOfJoin(IQueryTable rhs, string columnsToMatch, string columnsToAdd)
        {
            return InternalJoin(JoinDescriptor.JoinDescriptorJoinType.ReverseAJ,
                rhs, columnsToMatch, columnsToAdd);
        }

        public IQueryTable ReverseAsOfJoin(IQueryTable rhs, string columnsToMatch)
        {
            return InternalJoin(JoinDescriptor.JoinDescriptorJoinType.ReverseAJ,
                rhs, columnsToMatch, null);
        }

        public IQueryTable Freeze()
        {
            var rb = MakeResultBuilder();
            SnapshotOperation.Invoke(TableState, rb.ResultTsBuilder, null, true, null);
            return rb.ResultTable;
        }

        public IQueryTable Snapshot(IQueryTable targetTable, bool doInitialSnapshot, string[] stampColumns)
        {
            var rb = MakeResultBuilder();
            // "this" is the time table
            SnapshotOperation.Invoke(targetTable.TableState, rb.ResultTsBuilder, TableState,
                doInitialSnapshot, stampColumns);
            return rb.ResultTable;
        }

        public IQueryTable Merge(string keyColumn, params IQueryTable[] sources)
        {
            var rb = MakeResultBuilder();
            var sourceStates = sources.Select(s => s.TableState).ToArray();
            MergeTablesOperation.Invoke(TableState,rb.ResultTsBuilder, keyColumn, sourceStates);
            return rb.ResultTable;
        }

        public IQueryTable Merge(params IQueryTable[] sources)
        {
            return Merge(null, sources);
        }

        public IQueryTable HeadBy(int rows, params string[] groupByColumns)
        {
            var rb = MakeResultBuilder();
            HeadByOperation.Invoke(TableState, rb.ResultTsBuilder, rows, groupByColumns);
            return rb.ResultTable;
        }

        public IQueryTable TailBy(int rows, params string[] groupByColumns)
        {
            var rb = MakeResultBuilder();
            TailByOperation.Invoke(TableState, rb.ResultTsBuilder, rows, groupByColumns);
            return rb.ResultTable;
        }

        public IQueryTable Head(int rows)
        {
            var rb = MakeResultBuilder();
            HeadOrTailOperation.Invoke(TableState, rb.ResultTsBuilder, true, rows);
            return rb.ResultTable;
        }

        public IQueryTable Tail(int rows)
        {
            var rb = MakeResultBuilder();
            HeadOrTailOperation.Invoke(TableState, rb.ResultTsBuilder, false, rows);
            return rb.ResultTable;
        }

        public IQueryTable Flatten()
        {
            var rb = MakeResultBuilder();
            FlattenOperation.Invoke(TableState, rb.ResultTsBuilder);
            return rb.ResultTable;
        }

        public IQueryTable Sort(params string[] columns)
        {
            return Sort(SortDirection.Ascending, false, columns);
        }

        public IQueryTable SortDescending(params string[] columns)
        {
            return Sort(SortDirection.Descending, false, columns);
        }

        public IQueryTable SortAbs(params string[] columns)
        {
            return Sort(SortDirection.Ascending, true, columns);
        }

        public IQueryTable SortDescendingAbs(params string[] columns)
        {
            return Sort(SortDirection.Descending, true, columns);
        }

        public IQueryTable Sort(SortDirection direction, bool abs, params string[] columns)
        {
            var sorts = columns.Select(c => new SortPair(c, direction, abs)).ToArray();
            return Sort(sorts);
        }

        public IQueryTable Sort(params SortPair[] sorts)
        {
            var rb = MakeResultBuilder();
            ApplySortOperation.Invoke(TableState, rb.ResultTsBuilder, sorts);
            return rb.ResultTable;
        }

        public IQueryTable UpdateView(params string[] customColumns)
        {
            var rb = MakeResultBuilder();
            var ccds = customColumns.Select(cc => new CustomColumnDescriptor {Expression = cc}).ToArray();
            CustomColumnsTableOperation.Invoke(TableState, rb.ResultTsBuilder, ccds);
            return rb.ResultTable;
        }

        public IQueryTable Update(params string[] columns)
        {
            var rb = MakeResultBuilder();
            UpdateOperation.Invoke(TableState, rb.ResultTsBuilder, columns);
            return rb.ResultTable;
        }

        public IQueryTable LazyUpdate(params string[] columns)
        {
            var rb = MakeResultBuilder();
            LazyUpdateOperation.Invoke(TableState, rb.ResultTsBuilder, columns);
            return rb.ResultTable;
        }

        public IQueryTable View(params string[] columns)
        {
            var rb = MakeResultBuilder();
            ViewOperation.Invoke(TableState, rb.ResultTsBuilder, columns);
            return rb.ResultTable;
        }

        public IQueryTable Select(params string[] columns)
        {
            var rb = MakeResultBuilder();
            SelectOperation.Invoke(TableState, rb.ResultTsBuilder, columns);
            return rb.ResultTable;
        }

        public IQueryTable DropColumns(params string[] columns)
        {
            var rb = MakeResultBuilder();
            DropColumnsOperation.Invoke(TableState, rb.ResultTsBuilder, columns);
            return rb.ResultTable;
        }

        public IQueryTable Preemptive(int updateIntervalMs)
        {
            var rb = MakeResultBuilder();
            PreemptiveTableOperation.Invoke(TableState, rb.ResultTsBuilder, updateIntervalMs);
            return rb.ResultTable;
        }

        public IQueryTable Ungroup(bool nullFill, params string[] columns)
        {
            var rb = MakeResultBuilder();
            UngroupOperation.Invoke(TableState, rb.ResultTsBuilder, nullFill, columns);
            return rb.ResultTable;
        }

        public IQueryTable Ungroup(bool nullFill)
        {
            return Ungroup(nullFill, null);
        }

        public IQueryTable Ungroup(params string[] columns)
        {
            if (columns != null && columns.Length == 0)
            {
                // ungroup craps out with an empty list (null is OK)
                throw new ArgumentException("No columns specified");
            }
            return Ungroup(false, columns);
        }

        public IQueryTable Ungroup()
        {
            return Ungroup(false, null);
        }

        public IQueryTable Where(string bexpr)
        {
            var rb = MakeResultBuilder();
            WhereOperation.Invoke(TableState, rb.ResultTsBuilder, bexpr);
            return rb.ResultTable;
        }

        public async Task<IColumn[]> GetColumnsTask()
        {
            var t = await TableState.ResolveTask();
            return t.GetColumns();
        }

        public async Task<IColumn[]> GetColumnsTask(string[] names, Type[] types)
        {
            var t = await TableState.ResolveTask();
            return t.GetColumns(names, types);
        }

        public async Task<ITableData> GetTableDataTask()
        {
            var t = await TableState.ResolveTask();
            var result = await t.GetTableData();
            return result;
        }

        public async Task<ITableData> GetTableDataTask(params string[] columns)
        {
            var t = await TableState.ResolveTask();
            return await t.GetTableData(columns);
        }

        public async Task<ITableData> GetTableDataTask(long first, long last)
        {
            var t = await TableState.ResolveTask();
            return await t.GetTableData(first, last);
        }

        public async Task<ITableData> GetTableDataTask(long first, long last, params string[] columns)
        {
            var t = await TableState.ResolveTask();
            return await t.GetTableData(first, last, columns);
        }

        public async Task<ITableData> GetTableDataTask(RowRangeSet rowRangeSet, params string[] columns)
        {
            var t = await TableState.ResolveTask();
            return await t.GetTableData(rowRangeSet, columns);
        }

        public async Task SubscribeAllTask()
        {
            var t = await TableState.ResolveTask();
            await t.SubscribeAll();
        }

        public async Task SubscribeAllTask(params string[] columns)
        {
            var t = await TableState.ResolveTask();
            await t.SubscribeAll(columns);
        }

        public async Task SubscribeTask(RowRangeSet rows, params string[] columns)
        {
            var t = await TableState.ResolveTask();
            await t.Subscribe(rows, columns);
        }

        public async Task UpdateSubscriptionTask(RowRangeSet rows, params string[] columns)
        {
            var t = await TableState.ResolveTask();
            await t.UpdateSubscription(rows, columns);
        }

        public async Task UnsubscribeTask()
        {
            var t = await TableState.ResolveTask();
            await t.Unsubscribe();
        }

        private ResultBuilder MakeResultBuilder()
        {
            var intr = Scope.Internal;
            var tsb = TableStateBuilder.Create(intr.TableStateScope, intr.Context);
            var qt = Create(Scope, tsb.TableStateTracker);
            return new ResultBuilder(tsb, qt);
        }

        public ILowLevel LowLevel => this;

        InitialTableDefinition ILowLevel.InitialTableDefinition => TableState.Resolve()._tableDefinition;
    }

    /// <summary>
    /// This class holds a TableStateBuilder and QueryTable that are in the process of being built.
    /// The usual process for the caller is to make a ResultBuilder, asynchronously fire off the request
    /// to the server (typically using the ResultTsBuilder) and then return the ResultTable to your caller.
    /// (By the way, this ResultTable will likely contain asynchronous state, namely its pending
    /// InitialTableDefinition, that's not resolved yet, but that's fine. QueryTable is designed to be
    /// used that way).
    /// </summary>
    internal class ResultBuilder
    {
        public TableStateBuilder ResultTsBuilder { get; }
        public QueryTable ResultTable { get; }

        public ResultBuilder(TableStateBuilder resultTsBuilder, QueryTable resultTable)
        {
            ResultTsBuilder = resultTsBuilder;
            ResultTable = resultTable;
        }
    }
}
