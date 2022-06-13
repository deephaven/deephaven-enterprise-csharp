/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Ide;

namespace Deephaven.Connector.Internal.Parameters
{
    internal class Int64Parameter : AbstractBindParameter<long?>
    {
        private Int64Parameter(long? value) : base(value)
        {
        }

        public static Int64Parameter Of(object value)
        {
            if (value == null)
            {
                return new Int64Parameter(null);
            }
            if (value is byte b)
            {
                return new Int64Parameter(b);
            }
            if (value is sbyte sb)
            {
                return new Int64Parameter(sb);
            }
            if (value is int i)
            {
                return new Int64Parameter(i);
            }
            if (value is uint u)
            {
                return new Int64Parameter(u);
            }
            if (value is long l)
            {
                return new Int64Parameter(l);
            }
            if (value is short s)
            {
                return new Int64Parameter(s);
            }
            if (value is ushort us)
            {
                return new Int64Parameter(us);
            }
            throw new ArgumentException($"Unable to bind value of type {value.GetType()} to Int64 parameter");
        }

        public override string GetLiteral(ConsoleSessionType sessionType)
        {
            string literal;
            switch (sessionType)
            {
                case ConsoleSessionType.Groovy:
                    // both groovy and python use "L" for long ints
                    literal = _value == null ? "NULL_LONG" : _value + "L";
                    break;
                case ConsoleSessionType.Python:
                    // both groovy and python use "L" for long ints
                    literal = _value == null ? "ADO_QueryConstants.NULL_LONG" : _value + "L";
                    break;
                default:
                    throw new ArgumentException("Unable to bind variable, unsupported session type: " + sessionType);
            }
            return literal;
        }
    }
}
