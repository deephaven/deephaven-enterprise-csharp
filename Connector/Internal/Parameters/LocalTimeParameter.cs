/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Data;
using Deephaven.OpenAPI.Shared.Ide;

namespace Deephaven.Connector.Internal.Parameters
{
    internal class LocalTimeParameter : AbstractBindParameter<LocalTime>
    {
        private LocalTimeParameter(LocalTime value) : base(value)
        {
        }

        public static LocalTimeParameter Of(object value)
        {
            if (value == null)
            {
                return new LocalTimeParameter(null);
            }
            if (value is LocalTime lt)
            {
                return new LocalTimeParameter(lt);
            }
            if (value is DateTime dateTimeValue)
            {
                return new LocalTimeParameter(
                    new LocalTime((byte)dateTimeValue.Hour, (byte)dateTimeValue.Minute,
                    (byte)dateTimeValue.Second, dateTimeValue.Millisecond * 1_000_000));
            }
            throw new ArgumentException($"Unable to bind value of type {value.GetType()} to byte parameter");
        }

        public override string GetLiteral(ConsoleSessionType sessionType)
        {
            if (_value == null)
                return GetNullObjectLiteral(sessionType);
            switch (sessionType)
            {
                case ConsoleSessionType.Groovy:
                    return $"java.time.LocalTime.of({_value.Hour},{_value.Minute},{_value.Second},{_value.Nano})";
                case ConsoleSessionType.Python:
                    return $"ADO_LocalTime.of({_value.Hour},{_value.Minute},{_value.Second},{_value.Nano})";
                default:
                    throw new ArgumentException("Unable to bind variable, unsupported session type: " + sessionType);
            }
        }
    }
}
