/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Ide;

namespace Deephaven.Connector.Internal.Parameters
{
    internal class BooleanParameter : AbstractBindParameter<bool?>
    {
        private BooleanParameter(bool? value) : base(value)
        {
        }

        public static BooleanParameter Of(object value)
        {
            if (value == null)
            {
                return new BooleanParameter(null);
            }
            if (value is bool b)
            {
                return new BooleanParameter(b);
            }
            throw new ArgumentException($"Unable to bind value of type {value.GetType()} to boolean parameter");
        }

        public override string GetLiteral(ConsoleSessionType sessionType)
        {
            string literal;
            switch (sessionType)
            {
                case ConsoleSessionType.Groovy:
                    literal = _value == null ? "null" : _value.ToString().ToLower();   // java is lower case for boolean literals
                    break;
                case ConsoleSessionType.Python:
                    literal = _value == null ? "None" : _value.ToString();
                    break;
                default:
                    throw new ArgumentException("Unable to bind variable, unsupported session type: " + sessionType);
            }
            return literal;
        }
    }
}
