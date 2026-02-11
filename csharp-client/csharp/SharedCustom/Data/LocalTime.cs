/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace Deephaven.OpenAPI.Shared.Data
{
    public class LocalTime
    {
        public sbyte Hour { get; set; }

        public sbyte Minute { get; set; }

        public sbyte Second { get; set; }

        public int Nano { get; set; }

        public LocalTime() { }

        public LocalTime(int hour, int minute, int second, int nano)
        {
            Hour = (sbyte)hour;
            Minute = (sbyte)minute;
            Second = (sbyte)second;
            Nano = nano;
        }

        /// <summary>
        /// Return the time as a string in HH:MM:SS.FFFFFFFFF format.
        /// The fractional second is in nanoseconds.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Hour:D2}:{Minute:D2}:{Second:D2}.{Nano:D9}";
        }
    }
}
