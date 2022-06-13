/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Core.API.Util;
using Deephaven.OpenAPI.Shared.Ide;

namespace Deephaven.Connector.Internal.Parameters
{
    internal class CharParameter : AbstractBindParameter<char?>
    {
        private CharParameter(char? value) : base(value)
        {
        }

        public static CharParameter Of(object value)
        {
            if (value == null)
            {
                return new CharParameter(null);
            }

            if (value is char ch)
            {
                return new CharParameter(ch);
            }

            if (value is string strValue)
            {
                if (strValue.Length != 1)
                    throw new InvalidOperationException("Cannot bind string of length <> 1 to char parameter");
                return new CharParameter(strValue[0]);
            }

            throw new ArgumentException($"Unable to bind value of type {value.GetType()} to char parameter");
        }

        public override string GetLiteral(ConsoleSessionType sessionType)
        {
            string literal;
            switch (sessionType)
            {
                case ConsoleSessionType.Groovy:
                    literal = _value == null ? "NULL_CHAR" : "'" + EscapeUtil.EscapeJava(_value.ToString()) + "'";
                    break;
                case ConsoleSessionType.Python:
                    // python has no single character literal, use a string
                    literal = _value == null ? "None" : "\"" + EscapeUtil.EscapePython(_value.ToString()) + "\"";
                    break;
                default:
                    throw new ArgumentException("Unable to bind variable, unsupported session type: " + sessionType);
            }
            return literal;
        }
    }
}
