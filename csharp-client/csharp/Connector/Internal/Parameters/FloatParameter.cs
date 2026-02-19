/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Ide;

namespace Deephaven.Connector.Internal.Parameters
{
    internal class FloatParameter : AbstractBindParameter<float?>
    {
        private FloatParameter(float? value) : base(value)
        {
        }

        public static FloatParameter Of(object value)
        {
            // we are pretty forgiving on the type used to bind, a lot of things
            // can be cast to a double without being lossy
            if (value == null)
            {
                return new FloatParameter(null);
            }
            if (value is float f)
            {
                return new FloatParameter(f);
            }
            if (value is byte b)
            {
                return new FloatParameter(b);
            }
            if (value is int i)
            {
                return new FloatParameter(i);
            }
            if (value is long l)
            {
                return new FloatParameter(l);
            }
            if (value is short s)
            {
                return new FloatParameter(s);
            }
            throw new ArgumentException($"Unable to bind value of type {value.GetType()} to float parameter");
        }

        public override string GetLiteral(ConsoleSessionType sessionType)
        {
            string literal;
            switch (sessionType)
            {
                case ConsoleSessionType.Groovy:
                    literal = _value == null ? "NULL_FLOAT" : _value + "f";
                    break;
                case ConsoleSessionType.Python:
                    // python has no difference between float/double
                    literal = _value == null ? "ADO_QueryConstants.NULL_FLOAT" : _value.ToString();
                    break;
                default:
                    throw new ArgumentException("Unable to bind variable, unsupported session type: " + sessionType);
            }
            return literal;
        }
    }
}
