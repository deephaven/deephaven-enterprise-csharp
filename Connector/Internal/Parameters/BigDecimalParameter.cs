/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Core.RPC.Serialization.Java.Math;
using Deephaven.OpenAPI.Shared.Ide;

namespace Deephaven.Connector.Internal.Parameters
{
    // bind as a string so we can represent the full range of java BigDecimal if necessary
    internal class BigDecimalParameter : AbstractBindParameter<string>
    {
        private BigDecimalParameter(string value) : base(value)
        {
        }

        public static BigDecimalParameter Of(object value)
        {
            if (value == null)
            {
                return new BigDecimalParameter(null);
            }
            if (value is string s)
            {
                return new BigDecimalParameter(s);
            }
            if (value is BigDecimal bd)
            {
                return new BigDecimalParameter(bd.ToString());
            }
            if (value is decimal d)
            {
                return new BigDecimalParameter(d.ToString());
            }
                throw new ArgumentException($"Unable to bind value of type {value.GetType()} to BigDecimal parameter");
        }

        public override string GetLiteral(ConsoleSessionType sessionType)
        {
            if (_value == null)
                return GetNullObjectLiteral(sessionType);
            string literal;
            switch (sessionType)
            {
                case ConsoleSessionType.Groovy:
                    literal = "new java.math.BigDecimal(\"" + _value + "\")"; // parse as a string to support full range of values
                    break;
                case ConsoleSessionType.Python:
                    literal = "ADO_BigDecimal('" + _value + "')";
                    break;
                default:
                    throw new ArgumentException("Unable to bind variable, unsupported session type: " + sessionType);
            }
            return literal;
        }
    }
}
