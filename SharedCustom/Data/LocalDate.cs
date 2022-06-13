/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace Deephaven.OpenAPI.Shared.Data
{
    public class LocalDate
    {
        public int Year { get; set; }

        public sbyte MonthValue { get; set; }

        public sbyte DayOfMonth { get; set; }

        public LocalDate() { }

        public LocalDate(int year, int monthValue, int dayOfMonth)
        {
            Year = year;
            MonthValue = (sbyte)monthValue;
            DayOfMonth = (sbyte)dayOfMonth;
        }

        /// <summary>
        /// Return the date as a string in yyyy-mm-dd format.
        /// </summary>
        /// <returns>The date formatted as a string</returns>
        public override string ToString()
        {
            return $"{Year:D4}-{MonthValue:D2}-{DayOfMonth:D2}";
        }
    }
}
