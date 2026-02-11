/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Data.Common;
using Deephaven.Connector;

namespace ConnectorTest
{
    public class TestSimpleBoundParameterQuery
    {
        private string _connectionString;

        public TestSimpleBoundParameterQuery(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Run()
        {
            using (DeephavenConnection connection = new DeephavenConnection(_connectionString))
            {
                connection.Open();

                // try a simple query that should work
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "db.i(\"DbInternal\",\"ProcessEventLog\").where(\"Date = @date\").head(100)";
                    DbParameter dateParam = command.CreateParameter();
                    dateParam.DbType = System.Data.DbType.String;
                    dateParam.Value = "2019-08-13";
                    dateParam.ParameterName = "@date";
                    command.Parameters.Add(dateParam);
                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write("\t");
                                Console.Write(reader[i]);
                            }
                            Console.WriteLine();
                        }
                    }
                }

                // embed a "@" character, make sure that works ok
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "db.i(\"DbInternal\",\"ProcessEventLog\").where(\"Date = @date\").head(100).view(\"MyCol=`@@`\")";
                    DbParameter dateParam = command.CreateParameter();
                    dateParam.DbType = System.Data.DbType.String;
                    dateParam.Value = "2019-08-13";
                    dateParam.ParameterName = "@date";
                    command.Parameters.Add(dateParam);
                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string s = (string)reader[reader.GetOrdinal("MyCol")];
                            if(!Object.Equals(s, "@"))
                            {
                                throw new Exception("Expected value of '@' from data reader");
                            }
                        }
                    }
                }

                // thos should fail since we don't bind @missing
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "db.i(\"DbInternal\",\"ProcessEventLog\")"
                        + ".where(\"Date = @date && LogEntry = @missing\")"
                        + ".head(100).view(\"Date\",\"Timestamp\",\"LogEntry\",\"I = i\""
                        + ",\"ByteCol = (byte)1\",\"CharCol = 'A'\",\"ShortCol = (short)2\""
                        + ",\"LongCol = (long)3\",\"DoubleCol = 3.14d\",\"FloatCol = 3.14159f\",\"StringArrayCol = new String[] { `A`,`B`,`C` }\")";
                    DbParameter dateParam = command.CreateParameter();
                    dateParam.DbType = System.Data.DbType.String;
                    dateParam.Value = "2019-08-13";
                    dateParam.ParameterName = "@date";
                    command.Parameters.Add(dateParam);
                    try
                    {
                        using (DbDataReader reader = command.ExecuteReader()) { }
                        throw new Exception("Query with unbound parameter should fail!");
                    }
                    catch (InvalidOperationException)
                    {
                        // expected
                    }
                }

                using (DbCommand command = connection.CreateCommand())
                {
                    DbParameter badParam = command.CreateParameter();
                    try
                    {
                        badParam.ParameterName = "something";
                        throw new Exception("Parameter without a leading '@' should fail!");
                    }
                    catch (ArgumentException)
                    {
                        // expected
                    }

                    try
                    {
                        badParam.ParameterName = "@";
                        throw new Exception("Parameter without name should fail!");
                    }
                    catch (ArgumentException)
                    {
                        // expected
                    }
                }
            }
        }
    }
}
