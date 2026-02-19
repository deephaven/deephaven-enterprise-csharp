/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Threading.Tasks;
using Deephaven.OpenAPI.Client.Fluent;
using Deephaven.OpenAPI.Client.Internal;
using Deephaven.OpenAPI.Core.API.Util;

namespace Deephaven.OpenAPI.Client
{
    public delegate void QueryTableUpdateHandler(IQueryTable table, ITableUpdate deltaUpdate);
    public delegate void QueryTableSnapshotHandler(IQueryTable table, ITableSnapshot snapshot);

    public interface IQueryTable : IDisposable
    {
        IQueryScope Scope { get; }
        TableState TableState { get; }

        IQueryScope NewScope(out IQueryTable selfInNewScope);

        event QueryTableUpdateHandler OnTableUpdate;
        event QueryTableSnapshotHandler OnTableSnapshot;

        IQueryTable Freeze();

        IQueryTable Snapshot(IQueryTable targetTable, bool doInitialSnapshot = true, string[] stampColumns = null);

        IQueryTable Merge(string keyColumn, params IQueryTable[] sources);
        IQueryTable Merge(params IQueryTable[] sources);

        IQueryTable Where(string literalCondition);

        IQueryTable Sort(params string[] columns);
        IQueryTable SortDescending(params string[] columns);
        IQueryTable SortAbs(params string[] columns);
        IQueryTable SortDescendingAbs(params string[] columns);
        IQueryTable Sort(SortDirection direction, bool abs, params string[] columns);
        IQueryTable Sort(params SortPair[] sorts);

        IQueryTable UpdateView(params string[] customColumns);
        IQueryTable Update(params string[] customColumns);
        IQueryTable LazyUpdate(params string[] customColumns);

        IQueryTable View(params string[] customColumns);
        IQueryTable Select(params string[] customColumns);

        IQueryTable DropColumns(params string[] columns);

        IQueryTable Preemptive(int sampleIntervalMs);

        IQueryTable InnerJoin(IQueryTable rhs, string[] columnsToMatch, string[] columnsToAdd);
        IQueryTable InnerJoin(IQueryTable rhs, string columnsToMatch, string columnsToAdd);
        IQueryTable InnerJoin(IQueryTable rhs, string columnsToMatch);

        IQueryTable NaturalJoin(IQueryTable rhs, string[] columnsToMatch, string[] columnsToAdd);
        IQueryTable NaturalJoin(IQueryTable rhs, string columnsToMatch, string columnsToAdd);
        IQueryTable NaturalJoin(IQueryTable rhs, string columnsToMatch);

        IQueryTable AsOfJoin(IQueryTable rhs, string[] columnsToMatch, string[] columnsToAdd);
        IQueryTable AsOfJoin(IQueryTable rhs, string columnsToMatch, string columnsToAdd);
        IQueryTable AsOfJoin(IQueryTable rhs, string columnsToMatch);

        IQueryTable ReverseAsOfJoin(IQueryTable rhs, string[] columnsToMatch, string[] columnsToAdd);
        IQueryTable ReverseAsOfJoin(IQueryTable rhs, string columnsToMatch, string columnsToAdd);
        IQueryTable ReverseAsOfJoin(IQueryTable rhs, string columnsToMatch);

        IQueryTable ExactJoin(IQueryTable rhs, string[] columnsToMatch, string[] columnsToAdd);
        IQueryTable ExactJoin(IQueryTable rhs, string columnsToMatch, string columnsToAdd);
        IQueryTable ExactJoin(IQueryTable rhs, string columnsToMatch);

        IQueryTable LeftJoin(IQueryTable rhs, string[] columnsToMatch, string[] columnsToAdd);
        IQueryTable LeftJoin(IQueryTable rhs, string columnsToMatch, string columnsToAdd);
        IQueryTable LeftJoin(IQueryTable rhs, string columnsToMatch);

        IQueryTable By(AggregateCombo comboAggregate, params string[] groupByColumns);

        IQueryTable By(params string[] groupByColumns);
        IQueryTable MinBy(params string[] groupByColumns);
        IQueryTable MaxBy(params string[] groupByColumns);
        IQueryTable SumBy(params string[] groupByColumns);
        IQueryTable AbsSumBy(params string[] groupByColumns);
        IQueryTable VarBy(params string[] groupByColumns);
        IQueryTable StdBy(params string[] groupByColumns);
        IQueryTable AvgBy(params string[] columns);
        IQueryTable FirstBy(params string[] groupByColumns);
        IQueryTable LastBy(params string[] groupByColumns);
        IQueryTable MedianBy(params string[] groupByColumns);
        IQueryTable PercentileBy(double percentile, bool avgMedian, params string[] groupByColumns);
        IQueryTable PercentileBy(double percentile, params string[] groupByColumns);
        IQueryTable CountBy(string countByColumn, params string[] groupByColumns);
        IQueryTable WAvgBy(string weightColumn, params string[] groupByColumns);

        IQueryTable TailBy(int n, params string[] groupByColumns);
        IQueryTable HeadBy(int n, params string[] groupByColumns);

        IQueryTable Tail(int n);
        IQueryTable Head(int n);
        IQueryTable Flatten();

        IQueryTable Ungroup(bool nullFill, params string[] groupByColumns);
        IQueryTable Ungroup(bool nullFill);
        IQueryTable Ungroup(params string[] groupByColumns);
        IQueryTable Ungroup();

        Task<IColumn[]> GetColumnsTask();
        Task<IColumn[]> GetColumnsTask(string[] names, Type[] types);

        Task<ITableData> GetTableDataTask();
        Task<ITableData> GetTableDataTask(params string[] columns);
        Task<ITableData> GetTableDataTask(long first, long last);
        Task<ITableData> GetTableDataTask(long first, long last, params string[] columns);
        Task<ITableData> GetTableDataTask(RowRangeSet rowRangeSet, params string[] columns);

        Task SubscribeAllTask();
        Task SubscribeAllTask(params string[] columns);
        Task SubscribeTask(RowRangeSet rows, params string[] columns);
        Task UpdateSubscriptionTask(RowRangeSet rows, params string[] columns);

        Task UnsubscribeTask();

        ILowLevel LowLevel { get; }
    }

    public static class IQueryTable_FluentExtensions
    {
        public static IQueryTable Where(this IQueryTable self, BooleanExpression condition)
        {
            return self.Where(condition.ToIrisRepresentation());
        }

        public static IQueryTable Sort(this IQueryTable self, params IColumn[] columns)
        {
            return self.Sort(columns.ToIrisRepArray());
        }

        public static IQueryTable SortDescending(this IQueryTable self, params IColumn[] columns)
        {
            return self.SortDescending(columns.ToIrisRepArray());
        }

        public static IQueryTable SortAbs(this IQueryTable self, params IColumn[] columns)
        {
            return self.SortAbs(columns.ToIrisRepArray());
        }

        public static IQueryTable SortDescendingAbs(this IQueryTable self, params IColumn[] columns)
        {
            return self.SortDescendingAbs(columns.ToIrisRepArray());
        }

        public static IQueryTable Sort(this IQueryTable self, SortDirection direction, bool abs,
            params IColumn[] columns)
        {
            return self.Sort(direction, abs, columns.ToIrisRepArray());
        }

        public static IQueryTable UpdateView(this IQueryTable self, params ISelectColumn[] customColumns)
        {
            return self.UpdateView(customColumns.ToIrisRepArray());
        }

        public static IQueryTable Update(this IQueryTable self, params ISelectColumn[] customColumns)
        {
            return self.Update(customColumns.ToIrisRepArray());
        }

        public static IQueryTable LazyUpdate(this IQueryTable self, params ISelectColumn[] customColumns)
        {
            return self.LazyUpdate(customColumns.ToIrisRepArray());
        }

        public static IQueryTable View(this IQueryTable self, params ISelectColumn[] customColumns)
        {
            return self.View(customColumns.ToIrisRepArray());
        }

        public static IQueryTable Select(this IQueryTable self, params ISelectColumn[] columns)
        {
            return self.Select(columns.ToIrisRepArray());
        }

        public static IQueryTable DropColumns(this IQueryTable self, params IColumn[] columns)
        {
            return self.DropColumns(columns.ToIrisRepArray());
        }

        public static IQueryTable InnerJoin(this IQueryTable self, IQueryTable rhs, IMatchWithColumn[] columnsToMatch,
            ISelectColumn[] columnsToAdd)
        {
            return self.InnerJoin(rhs, columnsToMatch.ToIrisRepArray(), columnsToAdd.ToIrisRepArray());
        }

        public static IQueryTable NaturalJoin(this IQueryTable self, IQueryTable rhs, IMatchWithColumn[] columnsToMatch,
            ISelectColumn[] columnsToAdd)
        {
            return self.NaturalJoin(rhs, columnsToMatch.ToIrisRepArray(), columnsToAdd.ToIrisRepArray());
        }

        public static IQueryTable AsOfJoin(this IQueryTable self, IQueryTable rhs, IMatchWithColumn[] columnsToMatch,
            ISelectColumn[] columnsToAdd)
        {
            return self.AsOfJoin(rhs, columnsToMatch.ToIrisRepArray(), columnsToAdd.ToIrisRepArray());
        }

        public static IQueryTable ReverseAsOfJoin(this IQueryTable self, IQueryTable rhs, IMatchWithColumn[] columnsToMatch,
            ISelectColumn[] columnsToAdd)
        {
            return self.ReverseAsOfJoin(rhs, columnsToMatch.ToIrisRepArray(), columnsToAdd.ToIrisRepArray());
        }

        public static IQueryTable ExactJoin(this IQueryTable self, IQueryTable rhs, IMatchWithColumn[] columnsToMatch,
            ISelectColumn[] columnsToAdd)
        {
            return self.ExactJoin(rhs, columnsToMatch.ToIrisRepArray(), columnsToAdd.ToIrisRepArray());
        }

        public static IQueryTable LeftJoin(this IQueryTable self, IQueryTable rhs, IMatchWithColumn[] columnsToMatch,
            ISelectColumn[] columnsToAdd)
        {
            return self.LeftJoin(rhs, columnsToMatch.ToIrisRepArray(), columnsToAdd.ToIrisRepArray());
        }

        public static IQueryTable By(this IQueryTable self, AggregateCombo comboAggregate,
            params ISelectColumn[] groupByColumns)
        {
            return self.By(comboAggregate, groupByColumns.ToIrisRepArray());
        }

        public static IQueryTable By(this IQueryTable self, params ISelectColumn[] columns)
        {
            return self.By(columns.ToIrisRepArray());
        }

        public static IQueryTable MinBy(this IQueryTable self, params ISelectColumn[] groupByColumns)
        {
            return self.MinBy(groupByColumns.ToIrisRepArray());
        }

        public static IQueryTable MaxBy(this IQueryTable self, params ISelectColumn[] groupByColumns)
        {
            return self.MaxBy(groupByColumns.ToIrisRepArray());
        }

        public static IQueryTable SumBy(this IQueryTable self, params ISelectColumn[] groupByColumns)
        {
            return self.SumBy(groupByColumns.ToIrisRepArray());
        }

        public static IQueryTable AbsSumBy(this IQueryTable self, params ISelectColumn[] groupByColumns)
        {
            return self.AbsSumBy(groupByColumns.ToIrisRepArray());
        }

        public static IQueryTable VarBy(this IQueryTable self, params ISelectColumn[] groupByColumns)
        {
            return self.VarBy(groupByColumns.ToIrisRepArray());
        }

        public static IQueryTable StdBy(this IQueryTable self, params ISelectColumn[] groupByColumns)
        {
            return self.StdBy(groupByColumns.ToIrisRepArray());
        }

        public static IQueryTable AvgBy(this IQueryTable self, params ISelectColumn[] columns)
        {
            return self.AvgBy(columns.ToIrisRepArray());
        }

        public static IQueryTable FirstBy(this IQueryTable self, params ISelectColumn[] groupByColumns)
        {
            return self.FirstBy(groupByColumns.ToIrisRepArray());
        }

        public static IQueryTable LastBy(this IQueryTable self, params ISelectColumn[] groupByColumns)
        {
            return self.LastBy(groupByColumns.ToIrisRepArray());
        }

        public static IQueryTable MedianBy(this IQueryTable self, params ISelectColumn[] groupByColumns)
        {
            return self.MedianBy(groupByColumns.ToIrisRepArray());
        }

        public static IQueryTable PercentileBy(this IQueryTable self, double percentile, bool avgMedian,
            params ISelectColumn[] groupByColumns)
        {
            return self.PercentileBy(percentile, avgMedian, groupByColumns.ToIrisRepArray());
        }

        public static IQueryTable PercentileBy(this IQueryTable self, double percentile,
            params ISelectColumn[] groupByColumns)
        {
            return self.PercentileBy(percentile, groupByColumns.ToIrisRepArray());
        }

        public static IQueryTable CountBy(this IQueryTable self, IColumn resultColumn,
            params ISelectColumn[] groupByColumns)
        {
            return self.CountBy(resultColumn.ToIrisRepresentation(), groupByColumns.ToIrisRepArray());
        }

        public static IQueryTable WAvgBy(this IQueryTable self, IColumn weightColumn,
            params ISelectColumn[] groupByColumns)
        {
            return self.WAvgBy(weightColumn.ToIrisRepresentation(), groupByColumns.ToIrisRepArray());
        }

        public static IQueryTable TailBy(this IQueryTable self, int n, params ISelectColumn[] groupByColumns)
        {
            return self.TailBy(n, groupByColumns.ToIrisRepArray());
        }

        public static IQueryTable HeadBy(this IQueryTable self, int n, params ISelectColumn[] groupByColumns)
        {
            return self.HeadBy(n, groupByColumns.ToIrisRepArray());
        }

        public static IQueryTable Ungroup(this IQueryTable self, bool nullFill, params ISelectColumn[] groupByColumns)
        {
            return self.Ungroup(nullFill, groupByColumns.ToIrisRepArray());
        }

        public static IQueryTable Ungroup(this IQueryTable self, params ISelectColumn[] groupByColumns)
        {
            return self.Ungroup(groupByColumns.ToIrisRepArray());
        }
    }

    /// <summary>
    /// These extension methods forward a "Task"-style request from an IColumn representation to an Iris
    /// string representation
    /// </summary>
    public static class IQueryTable_TaskColumnExtensions
    {
        public static Task<ITableData> GetTableDataTask(this IQueryTable self, params IColumn[] columns)
        {
            return self.GetTableDataTask(columns.ToIrisRepArray());
        }

        public static Task<ITableData> GetTableDataTask(this IQueryTable self, long first, long last,
            params IColumn[] columns)
        {
            return self.GetTableDataTask(first, last, columns.ToIrisRepArray());
        }

        public static Task<ITableData> GetTableDataTask(this IQueryTable self, RowRangeSet rowRangeSet,
            params IColumn[] columns)
        {
            return self.GetTableDataTask(rowRangeSet, columns.ToIrisRepArray());
        }

        public static Task SubscribeAllTask(this IQueryTable self, params IColumn[] columns)
        {
            return self.SubscribeAllTask(columns.ToIrisRepArray());
        }

        public static Task SubscribeTask(this IQueryTable self, RowRangeSet rows, params IColumn[] columns)
        {
            return self.SubscribeTask(rows, columns.ToIrisRepArray());
        }

        public static Task UpdateSubscriptionTask(this IQueryTable self, RowRangeSet rows, params IColumn[] columns)
        {
            return self.UpdateSubscriptionTask(rows, columns.ToIrisRepArray());
        }
    }

    /// <summary>
    /// These extension methods hold all the N-ary overloads for GetColumns, and the translation from Task
    /// to blocking call.
    /// </summary>
    public static class IQueryTable_GetColumnExtensions
    {
        public static IColumn[] GetColumns(this IQueryTable self)
        {
            return ExceptionUtil.ResultOrUnwrappedException(self.GetColumnsTask());
        }

        public static IColumn[] GetColumns(this IQueryTable self, string[] names, Type[] types)
        {
            return ExceptionUtil.ResultOrUnwrappedException(self.GetColumnsTask(names, types));
        }

        public static async Task<TCol> GetColumnTask<TCol>(
            this IQueryTable self, string name) where TCol : IColumn
        {
            var result = await self.GetColumnsTask(new[] {name}, new[] {typeof(TCol)});
            return (TCol) result[0];
        }

        public static TCol GetColumn<TCol>(
            this IQueryTable self, string name) where TCol : IColumn
        {
            return ExceptionUtil.ResultOrUnwrappedException(GetColumnTask<TCol>(self, name));
        }

        public static async Task<Tuple<TC1, TC2>> GetColumnsTask<TC1, TC2>(
            this IQueryTable self, string name1, string name2)
            where TC1 : IColumn
            where TC2 : IColumn
        {
            var result = await self.GetColumnsTask(new[] {name1, name2},
                new[] {typeof(TC1), typeof(TC2)});
            return Tuple.Create((TC1) result[0], (TC2) result[1]);
        }

        public static Tuple<TC1, TC2> GetColumns<TC1, TC2>(
            this IQueryTable self, string name1, string name2)
            where TC1 : IColumn
            where TC2 : IColumn
        {
            return ExceptionUtil.ResultOrUnwrappedException(GetColumnsTask<TC1, TC2>(self, name1, name2));
        }

        public static async Task<Tuple<TC1, TC2, TC3>> GetColumnsTask<TC1, TC2, TC3>(
            this IQueryTable self, string name1, string name2, string name3)
            where TC1 : IColumn
            where TC2 : IColumn
            where TC3 : IColumn
        {
            var result = await self.GetColumnsTask(new[] {name1, name2, name3},
                new[] {typeof(TC1), typeof(TC2), typeof(TC3)});
            return Tuple.Create((TC1) result[0], (TC2) result[1], (TC3) result[2]);
        }

        public static Tuple<TC1, TC2, TC3> GetColumns<TC1, TC2, TC3>(
            this IQueryTable self, string name1, string name2, string name3)
            where TC1 : IColumn
            where TC2 : IColumn
            where TC3 : IColumn
        {
            return ExceptionUtil.ResultOrUnwrappedException(GetColumnsTask<TC1, TC2, TC3>(self, name1, name2, name3));
        }

        public static async Task<Tuple<TC1, TC2, TC3, TC4>> GetColumnsTask<TC1, TC2, TC3, TC4>(
            this IQueryTable self, string name1, string name2, string name3, string name4)
            where TC1 : IColumn
            where TC2 : IColumn
            where TC3 : IColumn
            where TC4 : IColumn
        {
            var result = await self.GetColumnsTask(new[] {name1, name2, name3, name4},
                new[] {typeof(TC1), typeof(TC2), typeof(TC3), typeof(TC4)});
            return Tuple.Create((TC1) result[0], (TC2) result[1], (TC3) result[2], (TC4) result[3]);
        }

        public static Tuple<TC1, TC2, TC3, TC4> GetColumns<TC1, TC2, TC3, TC4>(
            this IQueryTable self, string name1, string name2, string name3, string name4)
            where TC1 : IColumn
            where TC2 : IColumn
            where TC3 : IColumn
            where TC4 : IColumn
        {
            return ExceptionUtil.ResultOrUnwrappedException(GetColumnsTask<TC1, TC2, TC3, TC4>(
                self, name1, name2, name3, name4));
        }

        public static async Task<Tuple<TC1, TC2, TC3, TC4, TC5>> GetColumnsTask<TC1, TC2, TC3, TC4, TC5>(
            this IQueryTable self, string name1, string name2, string name3, string name4, string name5)
            where TC1 : IColumn
            where TC2 : IColumn
            where TC3 : IColumn
            where TC4 : IColumn
            where TC5 : IColumn
        {
            var result = await self.GetColumnsTask(new[] {name1, name2, name3, name4, name5},
                new[] {typeof(TC1), typeof(TC2), typeof(TC3), typeof(TC4), typeof(TC5)});
            return Tuple.Create((TC1) result[0], (TC2) result[1], (TC3) result[2], (TC4) result[3], (TC5) result[4]);
        }

        public static Tuple<TC1, TC2, TC3, TC4, TC5> GetColumns<TC1, TC2, TC3, TC4, TC5>(
            this IQueryTable self, string name1, string name2, string name3, string name4, string name5)
            where TC1 : IColumn
            where TC2 : IColumn
            where TC3 : IColumn
            where TC4 : IColumn
            where TC5 : IColumn
        {
            return ExceptionUtil.ResultOrUnwrappedException(GetColumnsTask<TC1, TC2, TC3, TC4, TC5>(
                self, name1, name2, name3, name4, name5));
        }

        public static async Task<Tuple<TC1, TC2, TC3, TC4, TC5, TC6>> GetColumnsTask<TC1, TC2, TC3, TC4, TC5, TC6>(
            this IQueryTable self, string name1, string name2, string name3, string name4, string name5, string name6)
            where TC1 : IColumn
            where TC2 : IColumn
            where TC3 : IColumn
            where TC4 : IColumn
            where TC5 : IColumn
            where TC6 : IColumn
        {
            var result = await self.GetColumnsTask(new[] {name1, name2, name3, name4, name5, name6},
                new[] {typeof(TC1), typeof(TC2), typeof(TC3), typeof(TC4), typeof(TC5), typeof(TC6)});
            return Tuple.Create((TC1) result[0], (TC2) result[1], (TC3) result[2], (TC4) result[3], (TC5) result[4],
                (TC6) result[5]);
        }

        public static Tuple<TC1, TC2, TC3, TC4, TC5, TC6> GetColumns<TC1, TC2, TC3, TC4, TC5, TC6>(
            this IQueryTable self, string name1, string name2, string name3, string name4, string name5, string name6)
            where TC1 : IColumn
            where TC2 : IColumn
            where TC3 : IColumn
            where TC4 : IColumn
            where TC5 : IColumn
            where TC6 : IColumn
        {
            return ExceptionUtil.ResultOrUnwrappedException(GetColumnsTask<TC1, TC2, TC3, TC4, TC5, TC6>(
                self, name1, name2, name3, name4, name5, name6));
        }

        public static async Task<Tuple<TC1, TC2, TC3, TC4, TC5, TC6, TC7>> GetColumnsTask<TC1, TC2, TC3, TC4, TC5, TC6, TC7>(
            this IQueryTable self, string name1, string name2, string name3, string name4, string name5, string name6,
            string name7)
            where TC1 : IColumn
            where TC2 : IColumn
            where TC3 : IColumn
            where TC4 : IColumn
            where TC5 : IColumn
            where TC6 : IColumn
            where TC7 : IColumn
        {
            var result = await self.GetColumnsTask(new[] {name1, name2, name3, name4, name5, name6, name7},
                new[] {typeof(TC1), typeof(TC2), typeof(TC3), typeof(TC4), typeof(TC5), typeof(TC6), typeof(TC7)});
            return Tuple.Create((TC1) result[0], (TC2) result[1], (TC3) result[2], (TC4) result[3], (TC5) result[4],
                (TC6) result[5], (TC7) result[6]);
        }

        public static Tuple<TC1, TC2, TC3, TC4, TC5, TC6, TC7> GetColumns<TC1, TC2, TC3, TC4, TC5, TC6, TC7>(
            this IQueryTable self, string name1, string name2, string name3, string name4, string name5, string name6,
            string name7)
            where TC1 : IColumn
            where TC2 : IColumn
            where TC3 : IColumn
            where TC4 : IColumn
            where TC5 : IColumn
            where TC6 : IColumn
            where TC7 : IColumn
        {
            return ExceptionUtil.ResultOrUnwrappedException(GetColumnsTask<TC1, TC2, TC3, TC4, TC5, TC6, TC7>(
                self, name1, name2, name3, name4, name5, name6, name7));
        }
    }

    /// <summary>
    /// These extension methods turn a Task-returning method into a blocking method.
    /// </summary>
    public static class IQueryTable_TaskExtensions
    {
        public static ITableData GetTableData(this IQueryTable self)
        {
            return ExceptionUtil.ResultOrUnwrappedException(self.GetTableDataTask());
        }

        public static ITableData GetTableData(this IQueryTable self, params string[] columns)
        {
            return ExceptionUtil.ResultOrUnwrappedException(self.GetTableDataTask(columns));
        }

        public static ITableData GetTableData(this IQueryTable self, params IColumn[] columns)
        {
            return GetTableData(self, columns.ToIrisRepArray());
        }

        public static ITableData GetTableData(this IQueryTable self, long first, long last)
        {
            return ExceptionUtil.ResultOrUnwrappedException(self.GetTableDataTask(first, last));
        }

        public static ITableData GetTableData(this IQueryTable self, long first, long last, params string[] columns)
        {
            return ExceptionUtil.ResultOrUnwrappedException(self.GetTableDataTask(first, last, columns));
        }

        public static ITableData GetTableData(this IQueryTable self, long first, long last,
            params IColumn[] columns)
        {
            return GetTableData(self, first, last, columns.ToIrisRepArray());
        }

        public static ITableData GetTableData(this IQueryTable self, RowRangeSet rowRangeSet, params string[] columns)
        {
            return ExceptionUtil.ResultOrUnwrappedException(self.GetTableDataTask(rowRangeSet, columns));
        }

        public static ITableData GetTableData(this IQueryTable self, RowRangeSet rowRangeSet,
            params IColumn[] columns)
        {
            return GetTableData(self, rowRangeSet, columns.ToIrisRepArray());
        }

        public static void SubscribeAll(this IQueryTable self)
        {
            ExceptionUtil.WaitOrUnwrappedException(self.SubscribeAllTask());
        }

        public static void SubscribeAll(this IQueryTable self, params string[] columns)
        {
            ExceptionUtil.WaitOrUnwrappedException(self.SubscribeAllTask(columns));
        }

        public static void SubscribeAll(this IQueryTable self, params IColumn[] columns)
        {
            SubscribeAll(self, columns.ToIrisRepArray());
        }

        public static void Subscribe(this IQueryTable self, RowRangeSet rows, params string[] columns)
        {
            ExceptionUtil.WaitOrUnwrappedException(self.SubscribeTask(rows, columns));
        }

        public static void Subscribe(this IQueryTable self, RowRangeSet rows, params IColumn[] columns)
        {
            Subscribe(self, rows, columns.ToIrisRepArray());
        }

        public static void Unsubscribe(this IQueryTable self)
        {
            ExceptionUtil.WaitOrUnwrappedException(self.UnsubscribeTask());
        }

        public static void UpdateSubscription(this IQueryTable self, RowRangeSet rows, params string[] columns)
        {
            ExceptionUtil.WaitOrUnwrappedException(self.UpdateSubscriptionTask(rows, columns));
        }

        public static void UpdateSubscription(this IQueryTable self, RowRangeSet rows, params IColumn[] columns)
        {
            UpdateSubscription(self, rows, columns.ToIrisRepArray());
        }
    }
}
