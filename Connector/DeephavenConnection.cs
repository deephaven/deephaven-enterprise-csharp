/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Data;
using System.Data.Common;
using Deephaven.Connector.Internal;
using Deephaven.OpenAPI.Shared.Data;
using Deephaven.OpenAPI.Shared.Ide;

namespace Deephaven.Connector
{
    /// <summary>
    /// Manages a connection to a Deephaven Open API server.
    /// </summary>
    /// <remarks>
    /// This class is used to create command objects which can then be executed
    /// to run queries against a running Deephaven system.
    /// </remarks>
    public sealed class DeephavenConnection : DbConnection
    {
        public const int DefaultPort = 8123;
        public const int DefaultMaxHeapMb = 2048;
        public const SessionType DefaultSessionType = SessionType.Groovy;
        public const int DefaultTimeoutMs = 30_000;

        /// <summary>
        /// Listener for when auth token is refreshed for this connection.
        /// </summary>
        public Action<RefreshToken> OnTokenRefresh;

        /// <summary>
        /// Listener called when an error occurs on this connection.
        /// </summary>
        public Action<string> OnError;

        private OpenAPIConnection _openAPIConnection;

        /// <summary>
        /// The current connection string
        /// </summary>
        private string _connectionString;

        /// <summary>
        /// The parsed connection string set by the user
        /// </summary>
        internal DeephavenConnectionStringBuilder Settings { get; private set; } = DefaultSettings;

        private static readonly DeephavenConnectionStringBuilder DefaultSettings = new DeephavenConnectionStringBuilder();

        /// <summary>
        /// Constructs an instance of the <see cref="DeephavenConnection"/> object using using options provided by the given connection string builder.
        /// </summary>
        /// <param name="connectionStringBuilder"></param>
        public DeephavenConnection(DeephavenConnectionStringBuilder connectionStringBuilder)
        {
            Settings = connectionStringBuilder;
            _connectionString = Settings.ConnectionString;
        }

        /// <summary>
        /// Constructs an instance of the <see cref="DeephavenConnection"/> object using options provided by the given connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        public DeephavenConnection(string connectionString)
        {
            _connectionString = connectionString;
            Settings = new DeephavenConnectionStringBuilder(connectionString);
        }

        /// <summary>
        /// Constructs an instance of the <see cref="DeephavenConnection"/> object.
        /// </summary>
        public DeephavenConnection()
        {
        }

        /// <summary>
        /// Provides the internal console connection that this object wraps.
        /// </summary>
        internal ConsoleConnection ConsoleConnection { get; private set; }

        /// <summary>
        /// Gets or sets the string used to open the connection. May be set only
        /// when the connection is not open.
        /// </summary>
        public override string ConnectionString
        {
            get => _connectionString;
            set
            {
                CheckConnectionClosed();
                _connectionString = value;
                Settings = new DeephavenConnectionStringBuilder(value);
            }
        }

        private void CheckConnectionClosed()
        {
            if (_openAPIConnection != null)
            {
                throw new InvalidOperationException("Connection already open");
            }
        }

        /// <summary>
        /// Gets the name of the current database after a connection is opened,
        /// or the database name specified in the connection string before the
        /// connection is opened.
        /// Since Deephaven does not support more than one database instance
        /// per connection, always returns an empty string.
        /// </summary>
        public override string Database => "";

        /// <summary>
        /// Gets the name of the database server to which to connect.
        /// </summary>
        public override string DataSource => Settings.DataSource;

        /// <summary>
        /// Gets a string that represents the version of the server to which the object is connected.
        /// Always returns an empty string.
        /// </summary>
        public override string ServerVersion => "";

        /// <summary>
        /// Gets a value that describes the state of the connection (open/closed).
        /// </summary>
        public override ConnectionState State
        {
            get
            {
                if(_openAPIConnection == null && ConsoleConnection == null)
                {
                    return ConnectionState.Closed;
                }
                else
                {
                    return ConnectionState.Open;
                }
            }
        }

        /// <summary>
        /// Changes the current database for an open connection.
        /// Since Deephaven does not support multiple databases per connection,
        /// throws a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="databaseName">The name of the database for the connection to use.</param>
        public override void ChangeDatabase(string databaseName)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Closes the connection to the database.
        /// </summary>
        public override void Close()
        {
            ConsoleConnection.Dispose();
            _openAPIConnection.Dispose();
            ConsoleConnection = null;
            _openAPIConnection = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_openAPIConnection != null)
                {
                    this.Close();
                }
            }
            base.Dispose(disposing);
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }

        protected override DbCommand CreateDbCommand()
        {
            return new DeephavenCommand(this);
        }

        private void HandleTokenRefreshed(RefreshToken refreshToken)
        {
            OnTokenRefresh?.Invoke(refreshToken);
        }

        private void HandleError(string error)
        {
            OnError?.Invoke(error);
        }

        /// <summary>
        /// Opens a database connection with the settings specified by the <see cref="ConnectionString"/>.
        /// If the connection is already open, will throw a <see cref="InvalidOperationException"/> exception.
        /// </summary>
        public override void Open()
        {
            CheckConnectionClosed();
            _openAPIConnection = new OpenAPIConnection(Settings.DataSource, Settings.Username,
                Settings.Password, Settings.OperateAs);
            _openAPIConnection.OnError += HandleError;
            _openAPIConnection.OnTokenRefresh += HandleTokenRefreshed;

            ConsoleSessionType consoleSessionType =
                (ConsoleSessionType)Enum.Parse(typeof(ConsoleSessionType), Settings.SessionType.ToString());

            ConsoleConnection = _openAPIConnection.GetConsoleConnection(consoleSessionType,
                Settings.RemoteDebugPort, Settings.SuspendWorker, Settings.MaxHeapMb,
                Settings.TimeoutMs, Settings.LocalDateAsString, Settings.LocalTimeAsString);
        }
    }
}
