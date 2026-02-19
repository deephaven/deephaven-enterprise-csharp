/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Data;
using System.Data.Common;
using Deephaven.Connector;

namespace ConnectorTest
{
    public class MultiQueryTest : AbstractTestQuery
    {
        private string _connectionString;

        public MultiQueryTest(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Run()
        {
            // Provide the query string with a parameter placeholder.
            string queryString = "db.i(\"DbInternal\",\"ProcessEventLog\").where(\"Date = @date1\").head(10).view(\"Date\")\n"
                + "db.i(\"DbInternal\",\"ProcessEventLog\").where(\"Date = @date2\").head(10).view(\"Date\")\n"
                + "db.i(\"DbInternal\",\"ProcessEventLog\").where(\"Date = @date3\").head(10).view(\"Date\")";

            string[] dates =
            {
                "2019-08-13", "2019-08-14", "2019-08-15"
            };

            using (DeephavenConnection connection = new DeephavenConnection(_connectionString))
            {
                connection.Open();
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = queryString;
                    AddDbParameter(command, DbType.String, "@date1", dates[0]);
                    AddDbParameter(command, DbType.String, "@date2", dates[1]);
                    AddDbParameter(command, DbType.String, "@date3", dates[2]);

                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        int rows = 0;
                        while (reader.Read())
                        {
                            string date = reader.GetString(0);
                            if(!Object.Equals(date,dates[0]))
                            {
                                throw new Exception("Unexpected date from first query: " + date);
                            }
                            rows++;
                        }
                        Console.WriteLine("Got {0} rows from first query", rows);
                        rows = 0;
                        if(reader.NextResult())
                        {
                            while(reader.Read())
                            {
                                string date = reader.GetString(0);
                                if (!Object.Equals(date, dates[1]))
                                {
                                    throw new Exception("Unexpected date from first query: " + date);
                                }
                                rows++;
                            }
                        }
                        else
                        {
                            throw new Exception("Expected a 2nd result!");
                        }
                        Console.WriteLine("Got {0} rows from second query", rows);
                        rows = 0;
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                string date = reader.GetString(0);
                                if (!Object.Equals(date, dates[2]))
                                {
                                    throw new Exception("Unexpected date from first query: " + date);
                                }
                                rows++;
                            }
                        }
                        else
                        {
                            throw new Exception("Expected a 3rd result!");
                        }
                        Console.WriteLine("Got {0} rows from 3rd query", rows);
                        if(reader.NextResult())
                        {
                            throw new Exception("Got unexpected 4th result!");
                        }
                    }
                }
            }
        }
    }
}
