/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Data.Common;
using Deephaven.Connector;

namespace ConnectorTest
{
    public class TestOptionsQuery
    {
        private string _host;
        private string _username;
        private string _password;

        public TestOptionsQuery(string host, string username, string password)
        {
            _host = host;
            _username = username;
            _password = password;
        }

        private void RunQuery(DeephavenConnectionStringBuilder connectionStringBuilder)
        {
            RunQuery(connectionStringBuilder, 0);
        }

        private void RunQuery(DeephavenConnectionStringBuilder connectionStringBuilder, int fetchSize)
        {
            using (DeephavenConnection connection = new DeephavenConnection(connectionStringBuilder))
            {
                connection.Open();
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "db.i(\"DbInternal\",\"ProcessEventLog\").where(\"Date = currentDateNy()\").head(100)";
                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        // there doesn't seem to be a standard interface for fetch size so we have to use our impl
                        if(fetchSize > 0)
                        {
                            ((DeephavenDataReader)reader).FetchSize = fetchSize;
                        }
                        int rows = 0;
                        while (reader.Read())
                        {
                            rows++;
                        }
                        Console.WriteLine("Read {0} rows", rows);
                    }
                }
            }
        }

        public void RunWithBasicOptions()
        {
            DeephavenConnectionStringBuilder connectionStringBuilder = new DeephavenConnectionStringBuilder();
            connectionStringBuilder.Host = _host;
            connectionStringBuilder.Username = _username;
            connectionStringBuilder.Password = _password;
            Console.WriteLine($"Connecting with {connectionStringBuilder}...");
            RunQuery(connectionStringBuilder);
        }

        public void RunWithMaxHeap(int maxHeapMb)
        {
            DeephavenConnectionStringBuilder connectionStringBuilder = new DeephavenConnectionStringBuilder();
            connectionStringBuilder.Host = _host;
            connectionStringBuilder.Username = _username;
            connectionStringBuilder.Password = _password;
            connectionStringBuilder.MaxHeapMb = maxHeapMb;
            Console.WriteLine("Connecting with {0}...", connectionStringBuilder);
            RunQuery(connectionStringBuilder);
        }

        public void RunWithOperateAs(string operateAs)
        {
            DeephavenConnectionStringBuilder connectionStringBuilder = new DeephavenConnectionStringBuilder();
            connectionStringBuilder.Host = _host;
            connectionStringBuilder.Username = _username;
            connectionStringBuilder.Password = _password;
            connectionStringBuilder.OperateAs = operateAs;
            Console.WriteLine("Connecting with {0}...", connectionStringBuilder);
            RunQuery(connectionStringBuilder);
        }

        public void RunWithDebugOption(int remoteDebugPort, bool suspendWorker)
        {
            DeephavenConnectionStringBuilder connectionStringBuilder = new DeephavenConnectionStringBuilder();
            connectionStringBuilder.Host = _host;
            connectionStringBuilder.Username = _username;
            connectionStringBuilder.Password = _password;
            connectionStringBuilder.RemoteDebugPort = remoteDebugPort;
            connectionStringBuilder.SuspendWorker = suspendWorker;
            Console.WriteLine("Connecting with {0}...", connectionStringBuilder);
            RunQuery(connectionStringBuilder);
        }

        public void RunWithSessionType(SessionType sessionType)
        {
            DeephavenConnectionStringBuilder connectionStringBuilder = new DeephavenConnectionStringBuilder();
            connectionStringBuilder.Host = _host;
            connectionStringBuilder.Username = _username;
            connectionStringBuilder.Password = _password;
            connectionStringBuilder.SessionType = sessionType;
            Console.WriteLine("Connecting with {0}...", connectionStringBuilder);
            RunQuery(connectionStringBuilder);
        }

        public void RunWithFetchSize(int fetchSize)
        {
            DeephavenConnectionStringBuilder connectionStringBuilder = new DeephavenConnectionStringBuilder();
            connectionStringBuilder.Host = _host;
            connectionStringBuilder.Username = _username;
            connectionStringBuilder.Password = _password;
            Console.WriteLine("Connecting with {0} and fetch size {1}...", connectionStringBuilder, fetchSize);
            RunQuery(connectionStringBuilder, fetchSize);
        }
    }
}
