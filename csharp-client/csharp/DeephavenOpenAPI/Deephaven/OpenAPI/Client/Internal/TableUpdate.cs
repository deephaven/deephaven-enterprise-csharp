/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Client.Data;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client.Internal
{
    internal class TableUpdate : ITableUpdate
    {
        public DeltaUpdates DeltaUpdates { get; }

        internal TableUpdate(ColumnDefinition[] columnDefinitions, DeltaUpdates deltaUpdates)
            => DeltaUpdates = deltaUpdates;
    }
}
