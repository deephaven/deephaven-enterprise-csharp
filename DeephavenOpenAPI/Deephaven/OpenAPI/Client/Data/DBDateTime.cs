/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;

namespace Deephaven.OpenAPI.Client.Data
{
    /// <summary>
    /// An object representing a Deephaven Date/Time. Its internal representation is nanoseconds since the epoch,
    /// which is 1970-01-01T00:00:00.000000000 UTC. Null database objects are represented by null DBDateTime objects.
    /// The internal database represenation of null is a special long value, DeephavenConstants.NULL_LONG.
    /// You can use the static methods ToNanos and FromNanos to convert back and forth to longs. These methods honor
    /// the special "null" convention. Note that conversions to the .NET DateTime object will be lossy, because
    /// the resolution of DateTime is 100ns.
    /// </summary>
    public class DBDateTime : IEquatable<DBDateTime>, IComparable<DBDateTime>, IComparable
    {
        public static readonly DateTime EpochTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public const long NanosPerTick = 1_000_000 / TimeSpan.TicksPerMillisecond;

        /// <summary>
        /// Converts nanos-since-UTC-epoch to DBDateTime. Understands Deephaven null value sentinel.
        /// </summary>
        public static DBDateTime FromNanos(long nanos)
        {
            return nanos == DeephavenConstants.NULL_LONG ? null : new DBDateTime(nanos);
        }

        /// <summary>
        /// Converts DBDateTime to a nanos-since-UTC-epoch. Understands null values.
        /// </summary>
        public static long ToNanos(DBDateTime dt)
        {
            return dt == null ? DeephavenConstants.NULL_LONG : dt.Nanos;
        }

        public long Nanos { get; }

        /// <summary>
        /// Constructor that makes a DBDateTime from a .NET DateTime. Assumes the DateTime is in UTC.
        /// </summary>
        public DBDateTime(DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Local)
            {
                throw new ArgumentException($"Can't accept argument in local time");
            }
            Nanos = (dt.Ticks - EpochTime.Ticks) * NanosPerTick;
        }

        /// <summary>
        /// Instance method to convert DBDateTime to the .NET DateTime type. Note that this is a
        /// lossy conversion, because .NET's DateTime object has a resolution of 100 ns.
        /// </summary>
        public DateTime ToDateTime()
        {
            return EpochTime.AddTicks(Nanos / NanosPerTick);
        }

        public DBDateTime(int year, int month, int day) : this(year, month, day, 0, 0, 0, 0)
        {
        }

        public DBDateTime(int year, int month, int day, int hour, int minute, int second) :
            this(year, month, day, hour, minute, second, 0)
        {
        }

        public DBDateTime(int year, int month, int day, int hour, int minute, int second, long nanos)
        {
            // Let the library do all the tricky calendar conversion work
            var baseDateTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
            Nanos = (baseDateTime.Ticks - EpochTime.Ticks) * NanosPerTick + nanos;
        }

        private DBDateTime(long nanos) => Nanos = nanos;

        public int CompareTo(DBDateTime other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Nanos.CompareTo(other.Nanos);
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as DBDateTime);
        }

        public bool Equals(DBDateTime other)
        {
            return CompareTo(other) == 0;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DBDateTime);
        }

        public override int GetHashCode()
        {
            return Nanos.GetHashCode();
        }

        public override string ToString()
        {
            // Leverage .NET to do the calendar stuff
            var dt = ToDateTime();
            var nanoPiece = Nanos % 1_000_000_000;
            if (nanoPiece < 0)
            {
                nanoPiece += 1_000_000_000;
            }
            return $"{dt:yyyy-MM-dd}T{dt:HH:mm:ss}.{nanoPiece:000000000} UTC";
        }
    }
}
