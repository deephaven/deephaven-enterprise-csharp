/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Data;
using System.Data.Common;
using Deephaven.Connector;

namespace ConnectorTest
{
    public class TestBoundTypesQuery : AbstractTestQuery
    {
        private static readonly bool?[] boolData = { false, true, false, null, false, false };
        private static readonly sbyte?[] sbyteData = { 0, 1, -1, null, SByte.MaxValue, SByte.MinValue + 1 };
        private static readonly short?[] shortData = { 0, 1, -1, null, Int16.MaxValue, Int16.MinValue + 1 };
        private static readonly int?[] intData = { 0, 1, -1, null, Int32.MaxValue, Int32.MinValue + 1 };
        private static readonly long?[] longData = { 0, 1, -1, null, Int64.MaxValue, Int64.MinValue + 1 };
        private static readonly char?[] charData = { 'A', 'B', 'C', null, Char.MaxValue, Char.MinValue };
        private static readonly float?[] floatData = { 0.0f, 1.0f, -1.0f, null, Single.MaxValue, -3.4e+38f };
        private static readonly double?[] doubleData = { 0.0, 1.0, -1.0, null, Double.MaxValue, -1.79e+308 };
        private static readonly string[] stringData = { "Hello", "World", "!", null, "Some", "Strings" };
        private static readonly DateTime?[] dateTimeData = {
            new DateTime(2019, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2019, 3, 11, 12, 59, 59, DateTimeKind.Utc),
            null, null, null, null
        };
        private static readonly decimal?[] decimalData =
        {
            0,
            1,
            -1,
            null,
            decimal.MaxValue,
            decimal.MinValue
        };
        private static readonly decimal?[] bigIntegerData = { 0, 1, -1, null, (decimal)long.MaxValue, (decimal)long.MinValue };
        private static readonly DateTime?[] localDateData =
        {
           new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc),
           new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc),
           null,
           null,
           new DateTime(9999, 12, 31, 0, 0, 0, DateTimeKind.Utc),
           new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        private static readonly DateTime?[] localTimeData = {
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(1970, 1, 1, 1, 0, 0, DateTimeKind.Utc),
            new DateTime(1970, 1, 1, 13, 59, 59, DateTimeKind.Utc),
            null,
            new DateTime(1970, 1, 1, 23, 59, 59, 999, DateTimeKind.Utc), // although the docs say 100-nano precision, it seems like in truth it is only millis
            new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
        };

        private string _connectionString;

        public TestBoundTypesQuery(string connectionString)
        {
            this._connectionString = connectionString;
        }

        protected void TestSingleBoundParameter<T>(DbConnection dbConnection, string columnName, DbType dbType, T value,
            T[] data, int index, int length, bool matchFilterOk)
        {
            Console.WriteLine("Running query with single bound {0} parameter with value {1}...", dbType,
                Object.Equals(value, null) ? "(null)" : value.ToString());
            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = "db.i(\"ADOTestNS\",\"ADOTestTypes\")";
                if (matchFilterOk)
                {
                    dbCommand.CommandText += string.Format(".where(\"{0}=@value\")", columnName);
                }
                else
                {
                    dbCommand.CommandText += string.Format(".where(\"!isNull({0}) && {0}.equals(@value)\")", columnName);
                }
                AddDbParameter(dbCommand, dbType, "@value", value);
                T[] testData = data.SubArray(index, length);
                using (DbDataReader dbDataReader = dbCommand.ExecuteReader())
                {
                    int row = 0;
                    while (dbDataReader.Read() && row < length)
                    {
                        // we test each type with the getters that should work for it (the exact type and anything "larger")
                        TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal(columnName), testData[row]);
                        row++;
                    }
                }
            }
        }

        protected void TestMultipleBoundParameters(DbConnection dbConnection)
        {
            Console.WriteLine("Running query with multiple bound parameters");
            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = "db.i(\"ADOTestNS\",\"ADOTestTypes\").where(\"BooleanColumn=@bValue && (IntColumn=@iValue1 || IntColumn=@iValue2)\")";
                AddDbParameter(dbCommand, DbType.Boolean, "@bValue", false);
                AddDbParameter(dbCommand, DbType.Int32, "@iValue1", 0);
                AddDbParameter(dbCommand, DbType.Int32, "@iValue2", -1);

                using (DbDataReader dbDataReader = dbCommand.ExecuteReader())
                {
                    int row = 0;
                    while (dbDataReader.Read())
                    {
                        // we test each type with the getters that should work for it (the exact type and anything "larger")
                        TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("BooleanColumn"), false);
                        int expectedIntValue = row == 0 ? 0 : -1;
                        TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("IntColumn"), expectedIntValue);
                        row++;
                    }
                    if(row != 2)
                    {
                        throw new Exception("Unexpected # of rows in result set");
                    }
                }
            }
        }

        public void Run()
        {
            using (DbConnection dbConnection = new DeephavenConnection(_connectionString))
            {
                dbConnection.Open();

                // make sure we can bind multiple parameters
                TestMultipleBoundParameters(dbConnection);

                // non-null binds
                TestSingleBoundParameter(dbConnection, "BooleanColumn", DbType.Boolean, true, boolData, 1, 1, true);
                TestSingleBoundParameter(dbConnection, "ByteColumn", DbType.SByte, (sbyte)-1, sbyteData, 2, 1, true);
                TestSingleBoundParameter(dbConnection, "ShortColumn", DbType.Int16, (short)-1, shortData, 2, 1, true);
                TestSingleBoundParameter(dbConnection, "IntColumn", DbType.Int32, -1, intData, 2, 1, true);
                TestSingleBoundParameter(dbConnection, "LongColumn", DbType.Int64, -1, longData, 2, 1, true);
                //TestSingleBoundParameter(dbConnection, "CharColumn", DbType.StringFixedLength, (sbyte)-1, sbyteData, 1, 1);
                TestSingleBoundParameter(dbConnection, "FloatColumn", DbType.Single, -1.0f, floatData, 2, 1, true);
                TestSingleBoundParameter(dbConnection, "DoubleColumn", DbType.Double, -1.0d, doubleData, 2, 1, true);
                TestSingleBoundParameter(dbConnection, "StringColumn", DbType.String, "!", stringData, 2, 1, true);
                TestSingleBoundParameter(dbConnection, "DateTimeColumn", DbType.DateTime,
                    new DateTime(2019, 3, 11, 12, 59, 59, DateTimeKind.Utc), dateTimeData, 1, 1, true);
                TestSingleBoundParameter(dbConnection, "BigDecimalColumn", DbType.Decimal, -1, decimalData, 2, 1, false);
                //TestSingleBoundParameter(dbConnection, "BigIntegerColumn", DbType.Decimal, (sbyte)-1, sbyteData, 1, 1);
                TestSingleBoundParameter(dbConnection, "LocalDateColumn", DbType.Date,
                    new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc), localDateData, 1, 1, false);
                TestSingleBoundParameter(dbConnection, "LocalTimeColumn", DbType.Time,
                    new DateTime(1970, 1, 1, 13, 59, 59, DateTimeKind.Utc), localTimeData, 2, 1, false);

                // bind null values
                TestSingleBoundParameter(dbConnection, "BooleanColumn", DbType.Boolean, null, boolData, 3, 1, true);
                TestSingleBoundParameter(dbConnection, "ByteColumn", DbType.SByte, null, sbyteData, 3, 1, true);
                TestSingleBoundParameter(dbConnection, "ShortColumn", DbType.Int16, null, shortData, 3, 1, true);
                TestSingleBoundParameter(dbConnection, "IntColumn", DbType.Int32, null, intData, 3, 1, true);
                TestSingleBoundParameter(dbConnection, "LongColumn", DbType.Int64, null, longData, 3, 1, true);
                //TestSingleBoundParameter(dbConnection, "CharColumn", DbType.StringFixedLength, (sbyte)-1, sbyteData, 3, 1);
                TestSingleBoundParameter(dbConnection, "FloatColumn", DbType.Single, null, floatData, 3, 1, true);
                TestSingleBoundParameter(dbConnection, "DoubleColumn", DbType.Double, null, doubleData, 3, 1, true);
                TestSingleBoundParameter(dbConnection, "StringColumn", DbType.String, null, stringData, 3, 1, true);
                TestSingleBoundParameter(dbConnection, "DateTimeColumn", DbType.DateTime, null, dateTimeData, 3, 1, true);
                TestSingleBoundParameter(dbConnection, "BigDecimalColumn", DbType.Decimal, null, decimalData, 3, 1, false);
                //TestSingleBoundParameter(dbConnection, "BigIntegerColumn", DbType.Decimal, (sbyte)-1, sbyteData, 3, 1);
                TestSingleBoundParameter(dbConnection, "LocalDateColumn", DbType.Date, null, localDateData, 3, 1, false);
                TestSingleBoundParameter(dbConnection, "LocalTimeColumn", DbType.Time, null, localTimeData, 3, 1, false);
            }
            Console.Out.WriteLine("TestTypesQuery ran successfully!");
        }

        public void RunWithPython()
        {
            using (DbConnection dbConnection = new DeephavenConnection(_connectionString + ";SessionType=Python"))
            {
                dbConnection.Open();

                // make sure we can bind multiple parameters
                TestMultipleBoundParameters(dbConnection);

                // non-null binds
                TestSingleBoundParameter(dbConnection, "BooleanColumn", DbType.Boolean, true, boolData, 1, 1, true);
                TestSingleBoundParameter(dbConnection, "ByteColumn", DbType.SByte, (sbyte)-1, sbyteData, 2, 1, true);
                TestSingleBoundParameter(dbConnection, "ShortColumn", DbType.Int16, (short)-1, shortData, 2, 1, true);
                TestSingleBoundParameter(dbConnection, "IntColumn", DbType.Int32, -1, intData, 2, 1, true);
                TestSingleBoundParameter(dbConnection, "LongColumn", DbType.Int64, -1, longData, 2, 1, true);
                //TestSingleBoundParameter(dbConnection, "CharColumn", DbType.StringFixedLength, (sbyte)-1, sbyteData, 1, 1);
                TestSingleBoundParameter(dbConnection, "FloatColumn", DbType.Single, -1.0f, floatData, 2, 1, true);
                TestSingleBoundParameter(dbConnection, "DoubleColumn", DbType.Double, -1.0d, doubleData, 2, 1, true);
                TestSingleBoundParameter(dbConnection, "StringColumn", DbType.String, "!", stringData, 2, 1, true);
                TestSingleBoundParameter(dbConnection, "DateTimeColumn", DbType.DateTime,
                    new DateTime(2019, 3, 11, 12, 59, 59, DateTimeKind.Utc), dateTimeData, 1, 1, true);
                TestSingleBoundParameter(dbConnection, "BigDecimalColumn", DbType.Decimal, -1, decimalData, 2, 1, false);
                //TestSingleBoundParameter(dbConnection, "BigIntegerColumn", DbType.Decimal, (sbyte)-1, sbyteData, 1, 1);
                TestSingleBoundParameter(dbConnection, "LocalDateColumn", DbType.Date,
                    new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc), localDateData, 1, 1, false);
                TestSingleBoundParameter(dbConnection, "LocalTimeColumn", DbType.Time,
                    new DateTime(1970, 1, 1, 13, 59, 59, DateTimeKind.Utc), localTimeData, 2, 1, false);

                // bind null values
                TestSingleBoundParameter(dbConnection, "BooleanColumn", DbType.Boolean, null, boolData, 3, 1, true);
                TestSingleBoundParameter(dbConnection, "ByteColumn", DbType.SByte, null, sbyteData, 3, 1, true);
                TestSingleBoundParameter(dbConnection, "ShortColumn", DbType.Int16, null, shortData, 3, 1, true);
                TestSingleBoundParameter(dbConnection, "IntColumn", DbType.Int32, null, intData, 3, 1, true);
                // the following line commented out because there appears to be a bug where LongColumn==NULL_LONG does not work in Python (isNull does)
                //TestSingleBoundParameter(dbConnection, "LongColumn", DbType.Int64, null, longData, 3, 1, true);
                //TestSingleBoundParameter(dbConnection, "CharColumn", DbType.StringFixedLength, (sbyte)-1, sbyteData, 3, 1);
                TestSingleBoundParameter(dbConnection, "FloatColumn", DbType.Single, null, floatData, 3, 1, true);
                TestSingleBoundParameter(dbConnection, "DoubleColumn", DbType.Double, null, doubleData, 3, 1, true);
                TestSingleBoundParameter(dbConnection, "StringColumn", DbType.String, null, stringData, 3, 1, true);
                TestSingleBoundParameter(dbConnection, "DateTimeColumn", DbType.DateTime, null, dateTimeData, 3, 1, true);
                TestSingleBoundParameter(dbConnection, "BigDecimalColumn", DbType.Decimal, null, decimalData, 3, 1, false);
                //TestSingleBoundParameter(dbConnection, "BigIntegerColumn", DbType.Decimal, (sbyte)-1, sbyteData, 3, 1);
                TestSingleBoundParameter(dbConnection, "LocalDateColumn", DbType.Date, null, localDateData, 3, 1, false);
                TestSingleBoundParameter(dbConnection, "LocalTimeColumn", DbType.Time, null, localTimeData, 3, 1, false);
            }
            Console.Out.WriteLine("TestTypesQuery ran successfully!");
        }
    }
}
