/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Numerics;
using Deephaven.OpenAPI.Core.RPC.Serialization.Java.Math;

namespace Deephaven.OpenAPI.Client.Data
{
    /// <summary>
    /// An object representing a Deephaven fixed point type. This type has
    /// essentially unlimited scale and precision.
    /// </summary>
    public struct DHDecimal
    {
        private readonly BigDecimal _value;

        internal DHDecimal(BigDecimal value)
        {
            _value = value;
        }

        internal BigDecimal GetBigDecimal()
        {
            return _value;
        }

        public DHDecimal(decimal value)
        {
            _value = new BigDecimal(value);
        }

        public int Scale => _value.Scale;

        public BigInteger Unscaled => _value.Unscaled;

        public decimal ToDecimal(bool throwOnDecimalOverflow)
        {
            return _value.ToDecimal(throwOnDecimalOverflow);
        }

        public decimal ToDecimal()
        {
            return _value.ToDecimal();
        }
    }
}
