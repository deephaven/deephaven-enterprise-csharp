/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Numerics;
using Deephaven.OpenAPI.Shared.Ide;

namespace Deephaven.Connector.Internal.Parameters
{
    internal class BigIntegerParameter : AbstractBindParameter<string>
    {
        private BigIntegerParameter(string value) : base(value)
        {
        }

        public static BigIntegerParameter Of(object value)
        {
            if (value == null)
            {
                return new BigIntegerParameter(null);
            }
            if (value is string s)
            {
                return new BigIntegerParameter(s);
            }
            if (value is BigInteger bi)
            {
                return new BigIntegerParameter(bi.ToString());
            }
            if (value is decimal decimalValue)
            {
                if (decimalValue % 1 != 0)
                    throw new InvalidOperationException("Cannot bind non-integer decimal value to BigInteger parameter");
                return new BigIntegerParameter(decimalValue.ToString());
            }
            throw new ArgumentException(
                $"Unable to bind value of type {value.GetType()} to BigIntegerParameter parameter");
        }

        public override string GetLiteral(ConsoleSessionType sessionType)
        {
            if(_value == null)
                return GetNullObjectLiteral(sessionType);
            string literal;
            switch (sessionType)
            {
                case ConsoleSessionType.Groovy:
                    literal = "new java.math.BigInteger(\"" + _value + "\")"; // parse as a string to support full range of values
                    break;
                case ConsoleSessionType.Python:
                    literal = "ADO_BigInteger('" + _value + "')";
                    break;
                default:
                    throw new ArgumentException("Unable to bind variable, unsupported session type: " + sessionType);
            }
            return literal;
        }
    }
}
