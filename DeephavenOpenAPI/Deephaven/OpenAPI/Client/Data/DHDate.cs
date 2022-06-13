/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;

namespace Deephaven.OpenAPI.Client.Data
{
    /// <summary>
    /// An object representing a local date (year, month, day of month).
    /// The <see cref="MinValue"/> and <see cref="MaxValue"/> constants provide
    /// the minimum and maximum dates representable in Deephaven.
    /// </summary>
    public class DHDate
    {
        public const int MinYear = -999999999;
        public const int MaxYear = 999999999;

        public static readonly DHDate MinValue = new DHDate(MinYear, 1, 1);
        public static readonly DHDate MaxValue = new DHDate(MaxYear, 12, 31);

        private readonly Shared.Data.LocalDate _localDate;

        internal DHDate(Shared.Data.LocalDate localDate)
        {
            _localDate = localDate;
        }

        public DHDate(int year, int monthValue, int dayOfMonth)
        {
            if (year < MinYear || year > MaxYear)
            {
                throw new ArgumentException("year");
            }

            if (monthValue < 1 || monthValue > 12)
            {
                throw new ArgumentException("monthValue");
            }

            if (dayOfMonth < 1 || dayOfMonth > 31)
            {
                throw new ArgumentException("dayOfMonth");
            }

            _localDate = new Shared.Data.LocalDate(year, monthValue, dayOfMonth);
        }

        internal Shared.Data.LocalDate GetLocalDate()
        {
            return _localDate;
        }

        public int Year
        {
            get => _localDate.Year;
            set => _localDate.Year = value;
        }

        public int MonthValue
        {
            get => _localDate.MonthValue;
            set => _localDate.MonthValue = (sbyte) value;
        }

        public int DayOfMonth
        {
            get => _localDate.DayOfMonth;
            set => _localDate.DayOfMonth = (sbyte) value;
        }

        public override string ToString()
        {
            return _localDate.ToString();
        }
    }
}
