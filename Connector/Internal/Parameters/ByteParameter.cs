/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Ide;

namespace Deephaven.Connector.Internal.Parameters
{
    internal class ByteParameter : AbstractBindParameter<sbyte?>
    {
        private ByteParameter(sbyte? value) : base(value)
        {
        }

        public static ByteParameter Of(object value)
        {
            if (value == null)
            {
                return new ByteParameter(null);
            }
            if (value is byte b)
            {
                return new ByteParameter((sbyte)b);
            }
            if (value is sbyte sb)
            {
                return new ByteParameter(sb);
            }
            throw new ArgumentException($"Unable to bind value of type {value.GetType()} to byte parameter");
        }

        public override string GetLiteral(ConsoleSessionType sessionType)
        {
            string literal;
            switch (sessionType)
            {
                case ConsoleSessionType.Groovy:
                    literal = _value == null ? "NULL_BYTE" : "(byte)" + _value;
                    break;
                case ConsoleSessionType.Python:
                    // python has no byte literal, we bind as integer
                    literal = _value == null ? "ADO_QueryConstants.NULL_BYTE" : _value.ToString();
                    break;
                default:
                    throw new ArgumentException("Unable to bind variable, unsupported session type: " + sessionType);
            }
            return literal;
        }
    }
}
