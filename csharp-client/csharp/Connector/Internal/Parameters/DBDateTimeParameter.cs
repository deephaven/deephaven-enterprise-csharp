/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Client.Data;
using Deephaven.OpenAPI.Shared.Ide;

namespace Deephaven.Connector.Internal.Parameters
{
    internal class DBDateTimeParameter : AbstractBindParameter<long?>
    {
        private DBDateTimeParameter(long? value) : base(value)
        {
        }

        public static DBDateTimeParameter Of(object value)
        {
            if (value == null)
            {
                return new DBDateTimeParameter(null);
            }
            if (value is long l)
            {
                return new DBDateTimeParameter(l);
            }
            if (value is DateTime dateTimeValue)
            {
                return new DBDateTimeParameter(new DBDateTime(dateTimeValue).Nanos);
            }
            throw new ArgumentException($"Unable to bind value of type {value.GetType()} to DBDateTime parameter");
        }

        public override string GetLiteral(ConsoleSessionType sessionType)
        {
            if (!_value.HasValue)
                return GetNullObjectLiteral(sessionType);
            string literal;
            switch (sessionType)
            {
                case ConsoleSessionType.Groovy:
                    literal = $"new DBDateTime({_value}L)";
                    break;
                case ConsoleSessionType.Python:
                    literal = $"ADO_DBDateTime({_value}L)";
                    break;
                default:
                    throw new ArgumentException("Unable to bind variable, unsupported session type: " + sessionType);
            }
            return literal;
        }
    }
}
