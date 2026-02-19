/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Ide;

namespace Deephaven.Connector.Internal.Parameters
{
    abstract internal class AbstractBindParameter<T> : BindParameter
    {
        protected T _value;

        public AbstractBindParameter(T value)
        {
            _value = value;
        }

        protected string GetNullObjectLiteral(ConsoleSessionType sessionType)
        {
            switch(sessionType)
            {
                case ConsoleSessionType.Groovy:
                    return "null";
                case ConsoleSessionType.Python:
                    return "None";
                default:
                    throw new ArgumentException("Unsupported session type: " + sessionType);
            }
        }

        public abstract string GetLiteral(ConsoleSessionType sessionType);
    }
}
