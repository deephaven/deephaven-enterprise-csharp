/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Ide;

namespace Deephaven.Connector.Internal.Parameters
{
    internal class Int32Parameter : AbstractBindParameter<int?>
    {
        private Int32Parameter(int? value) : base(value)
        {
        }

        public static Int32Parameter Of(object value)
        {
            if (value == null)
            {
                return new Int32Parameter(null);
            }
            if (value is byte b)
            {
                return new Int32Parameter(b);
            }
            if (value is int i)
            {
                return new Int32Parameter(i);
            }
            if (value is short s)
            {
                return new Int32Parameter(s);
            }
            throw new ArgumentException($"Unable to bind value of type {value.GetType()} to Int32 parameter");
        }

        public override string GetLiteral(ConsoleSessionType sessionType)
        {
            string literal;
            switch (sessionType)
            {
                case ConsoleSessionType.Groovy:
                    literal = _value == null ? "NULL_INT" : _value.ToString();
                    break;
                case ConsoleSessionType.Python:
                    literal = _value == null ? "ADO_QueryConstants.NULL_INT" : _value.ToString();
                    break;
                default:
                    throw new ArgumentException("Unable to bind variable, unsupported session type: " + sessionType);
            }
            return literal;
        }
    }
}
