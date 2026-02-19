/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Ide;

namespace Deephaven.Connector.Internal.Parameters
{
    internal class Int16Parameter : AbstractBindParameter<short?>
    {
        private Int16Parameter(short? value) : base(value)
        {
        }

        public static Int16Parameter Of(object value)
        {
            if (value == null)
            {
                return new Int16Parameter(null);
            }
            if (value is byte b)
            {
                return new Int16Parameter(b);
            }
            if (value is short s)
            {
                return new Int16Parameter(s);
            }
            throw new ArgumentException($"Unable to bind value of type {value.GetType()} to Int16 parameter");
        }

        public override string GetLiteral(ConsoleSessionType sessionType)
        {
            string literal;
            switch (sessionType)
            {
                case ConsoleSessionType.Groovy:
                    literal = _value == null ? "NULL_SHORT" : "(short)" + _value;
                    break;
                case ConsoleSessionType.Python:
                    // python has no short int, we bind as regular integer
                    literal = _value == null ? "ADO_QueryConstants.NULL_SHORT" : _value.ToString();
                    break;
                default:
                    throw new ArgumentException("Unable to bind variable, unsupported session type: " + sessionType);
            }
            return literal;
        }
    }
}
