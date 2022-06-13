/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Numerics;
using Deephaven.OpenAPI.Client;
using Deephaven.OpenAPI.Client.Data;

namespace Examples
{
    public static class NewTableExample
    {
        private static readonly bool?[] boolData = { null, false, true, false, false, true };
		private static readonly sbyte[] byteData = { DeephavenConstants.NULL_BYTE, 0, 1, -1, DeephavenConstants.MIN_BYTE, DeephavenConstants.MAX_BYTE };
		private static readonly short[] shortData = { DeephavenConstants.NULL_SHORT, 0, 1, -1, DeephavenConstants.MIN_SHORT, DeephavenConstants.MAX_SHORT };
		private static readonly int[] intData = { DeephavenConstants.NULL_INT, 0, 1, -1, DeephavenConstants.MIN_INT, DeephavenConstants.MAX_INT };
		private static readonly long[] longData = { DeephavenConstants.NULL_LONG, 0L, 1L, -1L, DeephavenConstants.MIN_LONG, DeephavenConstants.MAX_LONG };
		private static readonly float[] floatData = { DeephavenConstants.NULL_FLOAT, 0.0f, 1.0f, -1.0f, -3.4e+38f, float.MaxValue };
		private static readonly double[] doubleData = { DeephavenConstants.NULL_DOUBLE, 0.0, 1.0, -1.0, -1.79e+308, double.MaxValue };
        private static readonly decimal?[] decimalData = { null, 0, 1, -1, decimal.MinValue, decimal.MaxValue };
        private static readonly BigInteger?[] bigIntegerData = { null, BigInteger.Zero, BigInteger.One, BigInteger.MinusOne, new BigInteger(decimal.MinValue), new BigInteger(decimal.MaxValue) };
		private static readonly string[] stringData = { null, "", "A really long string", "Negative One", "AAAAAA", "ZZZZZZ" };
		private static readonly DBDateTime[] dateTimeData =
        {
            null,
            new DBDateTime(1970, 1, 1),
            new DBDateTime(2019, 12, 31, 23, 59, 59, 999),
            new DBDateTime(1900, 1, 1, 0, 0, 0),
            new DBDateTime(1966, 3, 1, 12, 34, 56, 123),
            new DBDateTime(2021, 1, 20, 12, 0, 0)
        };
        private static readonly DHDate[] dateData = { null, new DHDate(1970, 1, 1), new DHDate(2019, 1, 1), new DHDate(1900, 1, 1), DHDate.MinValue, DHDate.MaxValue };
        private static readonly DHTime[] timeData = { null, new DHTime(0, 0, 0, 0), new DHTime(1, 59, 59, 999999999), new DHTime(1, 0, 0, 0), DHTime.MinValue, DHTime.MaxValue };

        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
	        using (var temp = scope.TempTable(new[]
	        {
		        new ColumnDataHolder("BoolValue", new BooleanColumnData(boolData)),
		        new ColumnDataHolder("ByteValue", new ByteColumnData(byteData)),
		        new ColumnDataHolder("ShortValue", new ShortColumnData(shortData)),
		        new ColumnDataHolder("IntValue", new IntColumnData(intData)),
		        new ColumnDataHolder("LongValue", new LongColumnData(longData)),
		        new ColumnDataHolder("FloatValue", new FloatColumnData(floatData)),
		        new ColumnDataHolder("DoubleValue", new DoubleColumnData(doubleData)),
		        new ColumnDataHolder("DecimalValue", new DecimalColumnData(decimalData)),
		        new ColumnDataHolder("BigIntegerValue", new BigIntegerColumnData(bigIntegerData)),
		        new ColumnDataHolder("StringValue", new StringColumnData(stringData)),
		        new ColumnDataHolder("DateTimeValue", new DBDateTimeColumnData(dateTimeData)),
		        new ColumnDataHolder("DateValue", new DateColumnData(dateData)),
		        new ColumnDataHolder("TimeValue", new TimeColumnData(timeData))
	        }))
	        {
		        PrintUtils.PrintTableData(temp);
	        }
        }
    }
}
