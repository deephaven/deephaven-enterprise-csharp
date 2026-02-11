/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Core.API.Util;
using Deephaven.OpenAPI.Shared.Ide;

namespace Deephaven.Connector.Internal.Parameters
{
    internal class StringParameter : AbstractBindParameter<string>
    {
        private StringParameter(string value) : base(value)
        {
        }

        public static StringParameter Of(object value)
        {
            if (value == null)
            {
                return new StringParameter(null);
            }
            else if (value is string)
            {
                return new StringParameter((string)value);
            }
            else
            {
                return new StringParameter(value.ToString());
            }
        }

        public override string GetLiteral(ConsoleSessionType sessionType)
        {
            string literal;
            switch (sessionType)
            {
                case ConsoleSessionType.Groovy:
                    literal = _value == null
                        ? GetNullObjectLiteral(sessionType)
                        : "\"" + EscapeUtil.EscapeJava(_value) + "\"";
                    break;
                case ConsoleSessionType.Python:
                    literal = _value == null
                        ? GetNullObjectLiteral(sessionType)
                        : "\"" + EscapeUtil.EscapePython(_value) + "\"";
                    break;
                default:
                    throw new ArgumentException("Unable to bind variable, unsupported session type: " + sessionType);
            }
            return literal;
        }
    }
}
