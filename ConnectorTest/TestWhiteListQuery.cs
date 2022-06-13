/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Data.Common;
using Deephaven.Connector;

namespace ConnectorTest
{
    // test reading date and time data in different ways (string vs encoded byte[]->DateTime)
    public class TestWhiteListQuery : AbstractTestQuery
    {
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

        private static readonly string[] localDateStringData =
        {
            "2019-01-01",
            "2019-12-31",
           null,
           null,
           "9999-12-31",
           "0001-01-01"
        };
        private static readonly string[] localTimeStringData = {
            "00:00",
            "01:00",
            "13:59:59",
            null,
            "23:59:59.999999999",
            "00:00"
        };
        // our own ToString is a little different than the server
        private static readonly string[] localTimeStringData2 = {
            "00:00:00.000000000",
            "01:00:00.000000000",
            "13:59:59.000000000",
            null,
            "23:59:59.999999999",
            "00:00:00.000000000"
        };

        private string _connectionString;

        public TestWhiteListQuery(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public void Run()
        {
            // run with defaults (encoded)
            using (DbConnection dbConnection = new DeephavenConnection(_connectionString))
            {
                dbConnection.Open();
                using (DbCommand dbCommand = dbConnection.CreateCommand())
                {
                    dbCommand.CommandText = "db.i(\"ADOTestNS\",\"ADOTestTypes\")";
                    using (DbDataReader dbDataReader = dbCommand.ExecuteReader())
                    {
                        int row = 0;
                        while (dbDataReader.Read())
                        {
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("LocalDateColumn"), localDateData[row],
                                (ordinal) => dbDataReader.GetDateTime(ordinal));
                            string localDateStr = dbDataReader.GetString(dbDataReader.GetOrdinal("LocalDateColumn"));
                            if (!Object.Equals(localDateStr, localDateStringData[row]))
                                throw new Exception(string.Format("String version of LocalDate column does not match, expected {0} but got {1}",
                                    localDateStringData[row], localDateStr));
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("LocalTimeColumn"), localTimeData[row],
                                (ordinal) => dbDataReader.GetDateTime(ordinal));
                            string localTimeStr = dbDataReader.GetString(dbDataReader.GetOrdinal("LocalTimeColumn"));
                            if (!Object.Equals(localTimeStr, localTimeStringData2[row]))
                                throw new Exception(string.Format("String version of LocalTime column does not match, expected {0} but got {1}",
                                    localTimeStringData[row], localTimeStr));

                            row++;
                        }
                    }
                }
            }
            Console.WriteLine("Ran with defaults successfully");

            // run with values specified same as defaults
            using (DbConnection dbConnection = new DeephavenConnection(_connectionString + ";LocalDateAsString=false;LocalTimeAsString=false"))
            {
                dbConnection.Open();
                using (DbCommand dbCommand = dbConnection.CreateCommand())
                {
                    dbCommand.CommandText = "db.i(\"ADOTestNS\",\"ADOTestTypes\")";
                    using (DbDataReader dbDataReader = dbCommand.ExecuteReader())
                    {
                        int row = 0;
                        while (dbDataReader.Read())
                        {
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("LocalDateColumn"), localDateData[row],
                                (ordinal) => dbDataReader.GetDateTime(ordinal));
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("LocalTimeColumn"), localTimeData[row],
                                (ordinal) => dbDataReader.GetDateTime(ordinal));

                            row++;
                        }
                    }
                }
            }
            Console.WriteLine("Ran with defaults specified successfully");

            // just local date as string
            using (DbConnection dbConnection = new DeephavenConnection(_connectionString + ";LocalDateAsString=true"))
            {
                dbConnection.Open();
                using (DbCommand dbCommand = dbConnection.CreateCommand())
                {
                    dbCommand.CommandText = "db.i(\"ADOTestNS\",\"ADOTestTypes\")";
                    using (DbDataReader dbDataReader = dbCommand.ExecuteReader())
                    {
                        int row = 0;
                        while (dbDataReader.Read())
                        {
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("LocalDateColumn"), localDateStringData[row],
                                (ordinal) => dbDataReader.GetString(ordinal));
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("LocalTimeColumn"), localTimeData[row],
                                (ordinal) => dbDataReader.GetDateTime(ordinal));
                            row++;
                        }
                    }
                }
            }
            Console.WriteLine("Ran with LocalDate as string successfully");

            // just LocalTime as string
            using (DbConnection dbConnection = new DeephavenConnection(_connectionString + ";LocalTimeAsString=true"))
            {
                dbConnection.Open();
                using (DbCommand dbCommand = dbConnection.CreateCommand())
                {
                    dbCommand.CommandText = "db.i(\"ADOTestNS\",\"ADOTestTypes\")";
                    using (DbDataReader dbDataReader = dbCommand.ExecuteReader())
                    {
                        int row = 0;
                        while (dbDataReader.Read())
                        {
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("LocalDateColumn"), localDateData[row],
                                (ordinal) => dbDataReader.GetDateTime(ordinal));
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("LocalTimeColumn"), localTimeStringData[row],
                                (ordinal) => dbDataReader.GetString(ordinal));

                            row++;
                        }
                    }
                }
            }
            Console.WriteLine("Ran with LocalTime as string successfully");

            // run with defaults (encoded)
            using (DbConnection dbConnection = new DeephavenConnection(_connectionString + ";LocalDateAsString=true;LocalTimeAsString=true"))
            {
                dbConnection.Open();
                using (DbCommand dbCommand = dbConnection.CreateCommand())
                {
                    dbCommand.CommandText = "db.i(\"ADOTestNS\",\"ADOTestTypes\")";
                    using (DbDataReader dbDataReader = dbCommand.ExecuteReader())
                    {
                        int row = 0;
                        while (dbDataReader.Read())
                        {
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("LocalDateColumn"), localDateStringData[row],
                                (ordinal) => dbDataReader.GetString(ordinal));
                            TestValueMatches(dbDataReader, row, dbDataReader.GetOrdinal("LocalTimeColumn"), localTimeStringData[row],
                                (ordinal) => dbDataReader.GetString(ordinal));

                            row++;
                        }
                    }
                }
            }

            Console.WriteLine("Ran with LocalDate and LocalTime as strings successfully");

            Console.Out.WriteLine("TestWhiteListQuery ran successfully!");
        }
    }
}
