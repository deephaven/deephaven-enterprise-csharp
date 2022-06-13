/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Data;
using System.Data.Common;
using Deephaven.Connector;

namespace ConnectorTest
{
    public class TestTypesQuery : AbstractTestQuery
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

        public TestTypesQuery(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public void Run()
        {
            using (DbConnection dbConnection = new DeephavenConnection(_connectionString))
            {
                dbConnection.Open();
                using (DbCommand dbCommand = dbConnection.CreateCommand())
                {
                    dbCommand.CommandText = "db.i(\"ADOTestNS\",\"ADOTestTypes\")";
                    using (DbDataReader dbDataReader = dbCommand.ExecuteReader())
                    {
                        int row = 0;
                        while(dbDataReader.Read())
                        {
                            // we test each type with the getters that should work for it (the exact type and anything "larger")
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("BooleanColumn"), boolData[row],
                                (ordinal) => dbDataReader.GetBoolean(ordinal));
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("ByteColumn"), sbyteData[row],
                                (ordinal) => (sbyte)dbDataReader.GetByte(ordinal),
                                (ordinal) => (sbyte)dbDataReader.GetInt16(ordinal),
                                (ordinal) => (sbyte)dbDataReader.GetInt32(ordinal),
                                (ordinal) => (sbyte)dbDataReader.GetInt64(ordinal));
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("ShortColumn"), shortData[row],
                                (ordinal) => dbDataReader.GetInt16(ordinal),
                                (ordinal) => (short)dbDataReader.GetInt32(ordinal),
                                (ordinal) => (short)dbDataReader.GetInt64(ordinal));
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("IntColumn"), intData[row],
                                (ordinal) => dbDataReader.GetInt32(ordinal),
                                (ordinal) => (int)dbDataReader.GetInt64(ordinal));
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("LongColumn"), longData[row],
                                (ordinal) => dbDataReader.GetInt64(ordinal));
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("CharColumn"), charData[row],
                                (ordinal) => dbDataReader.GetChar(ordinal),
                                (ordinal) => dbDataReader.GetString(ordinal)[0]);
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("FloatColumn"), floatData[row],
                                (ordinal) => dbDataReader.GetFloat(ordinal),
                                (ordinal) => (float)dbDataReader.GetDouble(ordinal));
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("DoubleColumn"), doubleData[row],
                                (ordinal) => dbDataReader.GetDouble(ordinal));
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("StringColumn"), stringData[row],
                                (ordinal) => dbDataReader.GetString(ordinal));
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("DateTimeColumn"), dateTimeData[row],
                                (ordinal) => dbDataReader.GetDateTime(ordinal));
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("BigDecimalColumn"), decimalData[row],
                                (ordinal) => dbDataReader.GetDecimal(ordinal));
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("BigIntegerColumn"), bigIntegerData[row],
                                (ordinal) => dbDataReader.GetDecimal(ordinal));
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("LocalDateColumn"), localDateData[row],
                                (ordinal) => dbDataReader.GetDateTime(ordinal));
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("LocalTimeColumn"), localTimeData[row],
                                (ordinal) => dbDataReader.GetDateTime(ordinal));

                            row++;
                        }
                    }
                }
            }
            Console.Out.WriteLine("TestTypesQuery ran successfully!");
        }
    }
}
