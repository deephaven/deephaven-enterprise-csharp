/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Data;
using Deephaven.OpenAPI.Shared.Ide;

namespace Deephaven.Connector.Internal.Parameters
{
    internal class LocalDateParameter : AbstractBindParameter<LocalDate>
    {
        private LocalDateParameter(LocalDate value) : base(value)
        {
        }

        public static LocalDateParameter Of(object value)
        {
            if (value == null)
            {
                return new LocalDateParameter(null);
            }
            if (value is LocalDate ld)
            {
                return new LocalDateParameter(ld);
            }
            if (value is DateTime dateTimeValue)
            {
                return new LocalDateParameter(
                    new LocalDate(dateTimeValue.Year, (byte)dateTimeValue.Month, (byte)dateTimeValue.Day));
            }
            throw new ArgumentException($"Unable to bind value of type {value.GetType()} to LocalDate parameter");
        }

        public override string GetLiteral(ConsoleSessionType sessionType)
        {
            if (_value == null)
                return GetNullObjectLiteral(sessionType);
            switch (sessionType)
            {
                case ConsoleSessionType.Groovy:
                    return $"java.time.LocalDate.of({_value.Year},{_value.MonthValue},{_value.DayOfMonth})";
                case ConsoleSessionType.Python:
                    return $"ADO_LocalDate.of({_value.Year},{_value.MonthValue},{_value.DayOfMonth})";
                default:
                    throw new ArgumentException("Unable to bind variable, unsupported session type: " + sessionType);
            }
        }
    }
}
