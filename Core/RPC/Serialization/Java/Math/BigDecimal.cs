/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Numerics;
using System.Threading;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.Java.Math
{
    /// <summary>
    /// A thin wrapper to represent java BigDecimal values, with some utilities
    /// for formatting and converting to the native decimal type.
	/// This permits a client to handle the full range of java BigDecimal
    /// without the limitations of the decimal type.
    /// </summary>
    public struct BigDecimal
    {
        private BigInteger _unscaled;
        private int _scale;

        public BigDecimal(BigInteger unscaled, int scale)
        {
            _unscaled = unscaled;
            _scale = scale;
        }

        private static void WriteInt32(int value, byte[] buffer, int offset)
        {
            buffer[offset] = (byte)value;
            buffer[offset+1] = (byte)(value >> 8);
            buffer[offset+2] = (byte)(value >> 0x10);
            buffer[offset+3] = (byte)(value >> 0x18);
        }

        public BigDecimal(decimal value)
        {
            int[] parts = decimal.GetBits(value);
            byte[] buffer = DecimalByteBuffer.Value;
            Array.Clear(buffer, 0, buffer.Length);
            WriteInt32(parts[0], buffer, 0);
            WriteInt32(parts[1], buffer, sizeof(int));
            WriteInt32(parts[2], buffer, sizeof(int)*2);
            _unscaled = new BigInteger(buffer);
            if ((parts[3] & 0x80000000) != 0)
            {
                _unscaled = BigInteger.Negate(_unscaled);
            }
            _scale = (byte)((parts[3] >> 16) & 0x7F);
        }

        public static BigDecimal Parse(string strValue)
        {
            int ix = strValue.IndexOf('.');
            int scale = ix == -1 ? 0 : (strValue.Length - ix - 1);
            BigInteger unscaled = BigInteger.Parse(strValue.Replace(".", ""));
            return new BigDecimal(unscaled, scale);
        }

        public int Scale { get { return _scale; } }
        public BigInteger Unscaled { get { return _unscaled; } }

        // use ThreadLocal for this to make the decoder thread-safe
        // we need an extra byte here since BigInteger interprets the byte buffer as two's-complement but
        // the 12 bytes of magnitude coming out of decimal.GetBits() are unsigned
        private static readonly ThreadLocal<byte[]> DecimalByteBuffer = new ThreadLocal<byte[]>(() => new byte[13]);

        [Pure]
        public decimal ToDecimal()
        {
            return ToDecimal(true);
        }

        // convert to .NET decimal, which has more limited capacity than BigDecimal, but should be fine for most applications
        [Pure]
        public decimal ToDecimal(bool throwOnDecimalOverflow)
        {
            // get the abs value bytes and sanity check the size
            BigInteger absValue = BigInteger.Abs(_unscaled);
            byte[] bytes = absValue.ToByteArray();

            // There is one 13-byte encoded value that is legal, equal to the min/max value
            // a decimal can store. The BigInteger version will have a trailing zero which we can
            // ignore because decimals have a separate sign bit.
            if (bytes.Length > 13 || _scale > 28 || (bytes.Length == 13 && bytes[12] != 0))
            {
                if (throwOnDecimalOverflow)
                {
                    throw new OverflowException("Decimal value too large for .NET decimal");
                }
                // if not throwing an exception, return min/max depending on the sign
                return _unscaled.Sign < 0 ? decimal.MinValue : decimal.MaxValue;
            }

            // copy bytes to our reusable 12 byte buffer
            Array.Clear(DecimalByteBuffer.Value, 0, DecimalByteBuffer.Value.Length);
            Array.Copy(bytes, 0, DecimalByteBuffer.Value, 0, System.Math.Min(12, bytes.Length));

            // manufacture a decimal using the unscaled abs value bytes, the sign, and scale
            return new decimal(BitConverter.ToInt32(DecimalByteBuffer.Value, 0),
                BitConverter.ToInt32(DecimalByteBuffer.Value, 4),
                BitConverter.ToInt32(DecimalByteBuffer.Value, 8),
                _unscaled.Sign < 0, (byte)_scale);
        }

        /// <summary>
        /// A simple ToString that should handle any size number, but does not provide niceities like commas and localization.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string absValueStr = BigInteger.Abs(_unscaled).ToString();
            string unscaledStr = string.Concat(new string('0', System.Math.Max(0, _scale - absValueStr.Length + 1)), absValueStr);
            string prefix = _unscaled.Sign < 0 ? NumberFormatInfo.CurrentInfo.NegativeSign : "";
            if (_scale == 0)
                return prefix + unscaledStr;
            else
                return prefix + unscaledStr.Insert(unscaledStr.Length - _scale, NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
        }
    }
}
