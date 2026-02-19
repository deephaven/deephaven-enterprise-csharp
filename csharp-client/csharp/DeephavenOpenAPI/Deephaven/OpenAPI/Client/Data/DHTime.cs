/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;

namespace Deephaven.OpenAPI.Client.Data
{
    /// <summary>
    /// An object representing a Deephaven time-of-day. The
    /// <see cref="MinValue"/> and <see cref="MaxValue"/> constants provide
    /// the minimum and maximum values representable in Deephaven. This value
    /// has nanosecond precision.
    /// </summary>
    public class DHTime
    {
        public static readonly DHTime MinValue = new DHTime(0, 0, 0, 0);
        public static readonly DHTime MaxValue = new DHTime(23, 59, 59, 999999999);

        private Shared.Data.LocalTime _localTime;

        public DHTime(int hour, int minute, int second, int nano)
        {
            if (hour < 0 || hour > 23)
            {
                throw new ArgumentException("hour");
            }

            if (minute < 0 || minute > 59)
            {
                throw new ArgumentException("minute");
            }

            if (second < 0 || second > 59)
            {
                throw new ArgumentException("second");
            }

            if (nano < 0 || nano > 999999999)
            {
                throw new ArgumentException("nano");
            }

            _localTime = new Shared.Data.LocalTime(hour, minute, second, nano);
        }

        internal DHTime(Shared.Data.LocalTime localTime)
        {
            _localTime = localTime;
        }

        internal Shared.Data.LocalTime GetLocalTime()
        {
            return _localTime;
        }

        public int Hour
        {
            get => _localTime.Hour;
            set => _localTime.Hour = (sbyte) value;
        }

        public int Minute
        {
            get => _localTime.Minute;
            set => _localTime.Minute = (sbyte) value;
        }

        public int Second
        {
            get => _localTime.Second;
            set => _localTime.Second = (sbyte) value;
        }

        public int Nano
        {
            get => _localTime.Nano;
            set => _localTime.Nano = value;
        }

        public override string ToString()
        {
            return _localTime.ToString();
        }
    }
}
