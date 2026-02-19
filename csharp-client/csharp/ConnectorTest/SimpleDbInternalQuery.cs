/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Data.Common;
using Deephaven.Connector;

namespace ConnectorTest
{
    public class SimpleDbInternalQuery
    {
        private string _connectionString;

        public SimpleDbInternalQuery(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Run()
        {
            // Provide the query string with a parameter placeholder.
            string queryString = "db.i(\"DbInternal\",\"ProcessEventLog\")"
                + ".where(\"Date = currentDateNy()\")"
                + ".head(100).view(\"Date\",\"Timestamp\",\"LogEntry\",\"I = i\""
                + ",\"ByteCol = (byte)1\",\"CharCol = 'A'\",\"ShortCol = (short)2\""
                               + ",\"LongCol = (long)3\",\"DoubleCol = 3.14d\",\"FloatCol = 3.14159f\",\"StringArrayCol = new String[] { `A`,`B`,`C` }\")";

            using (DeephavenConnection connection = new DeephavenConnection(_connectionString))
            {
                connection.OnTokenRefresh += (token) =>
                {
                    Console.WriteLine("Token refreshed at " + token.Expiry);
                };
                connection.OnError += (error) =>
                {
                    Console.WriteLine("Error refreshing token: " + error);
                };
                connection.Open();
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = queryString;
                    //command.Parameters.AddWithValue("@pricePoint", paramValue);
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
            }
        }
    }
}
