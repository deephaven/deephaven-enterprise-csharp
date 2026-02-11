/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Data.Common;

namespace Deephaven.Connector
{
    /// <summary>
    /// Represents a set of methods for creating instances of Deephaven's implementation of the data source classes.
    /// </summary>
    public class DeephavenProviderFactory : DbProviderFactory
    {
        /// <summary>
        /// Returns a new instance of the Deephaven implementation of the <see cref="DbCommand"/> class.
        /// </summary>
        /// <returns>A new instance of <see cref="DbCommand"/>.</returns>
        public override DbCommand CreateCommand()
        {
            return new DeephavenCommand();
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <returns>A new instance of <see cref="DbCommandBuilder"/>.</returns>
        public override DbCommandBuilder CreateCommandBuilder()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a new instance of the Deephaven implementation of the <see cref="DbConnection"/> class.
        /// </summary>
        /// <returns>A new instance of <see cref="DbConnection"/>.</returns>
        public override DbConnection CreateConnection()
        {
            return new DeephavenConnection();
        }

        /// <summary>
        /// Returns a new instance of the Deephaven implementation of the <see cref="DbConnectionStringBuilder"/> class.
        /// </summary>
        /// <returns>A new instance of <see cref="DbConnectionStringBuilder"/>.</returns>
        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new DeephavenConnectionStringBuilder();
        }

        /// <summary>
        /// Returns a new instance of the Deephaven implementation of the <see cref="DbParameter"/> class.
        /// </summary>
        /// <returns>A new instance of <see cref="DeephavenParameter"/>.</returns>
        public override DbParameter CreateParameter()
        {
            return new DeephavenParameter();
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <returns>A new instance of <see cref="DbDataAdapter"/>.</returns>
        public override DbDataAdapter CreateDataAdapter()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <returns>A new instance of <see cref="DbDataSourceEnumerator"/>.</returns>
        public override DbDataSourceEnumerator CreateDataSourceEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a value that indicates whether this <see cref="DbProviderFactory"/> instance supports the <see cref="DbDataSourceEnumerator"/> class.
        /// Always returns false.
        /// </summary>
        public override bool CanCreateDataSourceEnumerator => false;
    }
}
