/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Ide;

namespace Deephaven.Connector.Internal.Parameters
{
    internal class DoubleParameter : AbstractBindParameter<double?>
    {
        private DoubleParameter(double? value) : base(value)
        {
        }

        public static DoubleParameter Of(object value)
        {
            // we are pretty forgiving on the type used to bind, a lot of things
            // can be cast to a double without being lossy
            if (value == null)
            {
                return new DoubleParameter(null);
            }
            if (value is double d)
            {
                return new DoubleParameter(d);
            }
            if (value is float f)
            {
                return new DoubleParameter(f);
            }
            if (value is byte b)
            {
                return new DoubleParameter(b);
            }
            if (value is int i)
            {
                return new DoubleParameter(i);
            }
            if (value is long l)
            {
                return new DoubleParameter(l);
            }
            if (value is short s)
            {
                return new DoubleParameter(s);
            }
            throw new ArgumentException($"Unable to bind value of type {value.GetType()} to double parameter");
        }

        public override string GetLiteral(ConsoleSessionType sessionType)
        {
            string literal;
            switch (sessionType)
            {
                case ConsoleSessionType.Groovy:
                    literal = _value == null ? "NULL_DOUBLE" : _value + "d";
                    break;
                case ConsoleSessionType.Python:
                    // python has no difference between float/double
                    literal = _value == null ? "ADO_QueryConstants.NULL_DOUBLE" : _value.ToString();
                    break;
                default:
                    throw new ArgumentException("Unable to bind variable, unsupported session type: " + sessionType);
            }
            return literal;
        }
    }
}
