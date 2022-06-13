/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Deephaven.Connector.Internal;
using Deephaven.Connector.Internal.Parameters;

namespace Deephaven.Connector
{
    /// <summary>
    /// Executes ad-hoc Deephaven queries against a Deephaven Open API server.
    /// </summary>
    /// <remarks>
    /// Unlike most ADO.NET implementations, this does not accept SQL queries/commands.
    /// Groovy and Python query expressions are accepted, depending on the session
    /// type specified when the <see cref="DeephavenConnection"></see> is created.
    /// When <see cref="ExecuteDbDataReader(CommandBehavior)"/> is called, the
    /// query or queries specied in <see cref="CommandText"/> are sent to the
    /// server for execution, and a snapshot of the result(s) produced.
    /// The query results can be iterated using the standard <see cref="DbDataReader"/> methods.
    /// </remarks>
    public sealed class DeephavenCommand : DbCommand
    {
        private DeephavenConnection _dbConnection;
        private DeephavenParameterCollection _parameters = new DeephavenParameterCollection();

        /// <summary>
        /// Constructs an instance of the <see cref="DeephavenCommand"/> object.
        /// </summary>
        public DeephavenCommand() : this(null, null)
        {
        }

        /// <summary>
        /// Constructs an instance of the <see cref="DeephavenCommand"/> object with the given connection.
        /// The connection should be an instance of <see cref="DeephavenConnection"/>.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DeephavenConnection"/> to use.</param>
        public  DeephavenCommand(DbConnection dbConnection) : this(null, dbConnection)
        {
        }

        /// <summary>
        /// Constructs an instance of the <see cref="DeephavenCommand"/> object with the given command text and connection.
        /// The connection should be an instance of <see cref="DeephavenConnection"/>.
        /// </summary>
        /// <param name="commandText">The command text in the appropriate language for the session type.</param>
        /// <param name="dbConnection">The <see cref="DeephavenConnection"/> to use.</param>
        public DeephavenCommand(string commandText, DbConnection dbConnection)
        {
            CommandText = commandText;
            DbConnection = dbConnection;
            CommandTimeout = 600;
        }

        /// <summary>
        /// The command text in the appropriate language for the session type.
        /// </summary>
        public override string CommandText { get; set; }

        /// <summary>
        /// The maximum time to wait when executing this query in seconds. Default is 600 (10 minutes).
        /// You may need to increase this parameter for complex or very large queries.
        /// </summary>
        public override int CommandTimeout { get; set; }

        /// <summary>
        /// The command type. Only text commands are supported at this time.
        /// </summary>
        public override CommandType CommandType
        {
            get
            {
                return CommandType.Text;
            }
            set
            {
                if (value != CommandType.Text)
                    throw new NotSupportedException("Only text commands supported");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the command object should be visible in a customized interface control.
        /// </summary>
        public override bool DesignTimeVisible { get; set; }

        /// <summary>
        /// Gets or sets how command results are applied to the DataRow when used by the Update method of a DbDataAdapter.
        /// Since updates are not presently supported, only <see cref="UpdateRowSource.None"/> is supported.
        /// </summary>
        public override UpdateRowSource UpdatedRowSource
        {
            get
            {
                return UpdateRowSource.None;
            }
            set
            {
                if (value != UpdateRowSource.None)
                    throw new NotSupportedException("Updates not supported via Deephaven ADO.NET interface.");
            }
        }

        /// <summary>
        /// Gets or sets the DbConnection used by this DbCommand.
        /// Must be a <see cref="DeephavenConnection"/> instance.
        /// </summary>
        protected override DbConnection DbConnection
        {
            get => _dbConnection;
            set
            {
                if(value is DeephavenConnection)
                {
                    _dbConnection = (DeephavenConnection)value;
                }
                else
                {
                    throw new InvalidOperationException("DeephavenCommand requires a DeephavenConnection");
                }
            }
        }

        /// <summary>
        /// Gets the collection of DbParameter objects.
        /// </summary>
        protected override DbParameterCollection DbParameterCollection => _parameters;

        /// <summary>
        /// Gets or sets the <see cref="DbTransaction"/> within which this <see cref="DbCommand"/> object executes.
        /// Since transactions are not presently supported, must be null.
        /// </summary>
        protected override DbTransaction DbTransaction
        {
            get
            {
                return null;
            }
            set
            {
                if (value != null)
                    throw new NotSupportedException("Transactions not supported");
            }
        }

        /// <summary>
        /// Attempts to cancel the execution of a <see cref="DbCommand"/>. Not presently supported.
        /// </summary>
        public override void Cancel()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Executes a statement against a connection object. Not presently supported.
        /// </summary>
        /// <returns></returns>
        public override int ExecuteNonQuery()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in
        /// the result set returned by the query. All other columns and rows are ignored.
        /// Not presently supported.
        /// </summary>
        /// <returns>The first column of the first row in the result set.</returns>
        public override object ExecuteScalar()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates a prepared (or compiled) version of the command on the data source.
        /// Not presently supported.
        /// </summary>
        public override void Prepare()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Creates a new instance of a <see cref="DeephavenParameter"/> object.
        /// </summary>
        /// <returns>A <see cref="DeephavenParameter"/> object.</returns>
        protected override DbParameter CreateDbParameter()
        {
            return new DeephavenParameter();
        }

        /// <summary>
        /// Executes the command text against the connection.
        /// </summary>
        /// <param name="behavior">An instance of <see cref="CommandBehavior"/>.</param>
        /// <returns>A task representing the operation.</returns>
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            if(_dbConnection.ConsoleConnection == null)
            {
                throw new InvalidOperationException("Cannot execute command, connection not open");
            }

            Dictionary<string, BindParameter> parameters = new Dictionary<string, BindParameter>();
            Dictionary<string, string> parameterNameMap = new Dictionary<string, string>();
            foreach(DeephavenParameter deephavenParameter in _parameters)
            {
                parameters.Add(deephavenParameter.BoundName, deephavenParameter.GetBindParameter());
                parameterNameMap.Add(deephavenParameter.ParameterName, deephavenParameter.BoundName);
            }
            _dbConnection.ConsoleConnection.BindVariables(parameters);
            ConsoleConnection.SnapshotQueryResult[] results = _dbConnection.ConsoleConnection.ExecuteSnapshotQuery(
                CommandText, parameterNameMap, CommandTimeout*1000);
            return new DeephavenDataReader(results);
        }
    }
}
