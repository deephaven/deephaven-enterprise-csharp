/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Text;
using Deephaven.OpenAPI.Client.Data;

namespace Deephaven.OpenAPI.Client.Fluent
{
    public abstract class DateTimeExpression : Expression
    {
        public static implicit operator DateTimeExpression(string s)
        {
            return new DateTimeStringLiteral(s);
        }

        public static implicit operator DateTimeExpression(DBDateTime v)
        {
            return new DateTimeDBDateTimeValue(v);
        }

        public static BooleanExpression operator <(DateTimeExpression lhs, DateTimeExpression rhs)
        {
            return new ComparisonOperator(lhs, "<", rhs);
        }

        public static BooleanExpression operator <=(DateTimeExpression lhs, DateTimeExpression rhs)
        {
            return new ComparisonOperator(lhs, "<=", rhs);
        }

        public static BooleanExpression operator ==(DateTimeExpression lhs, DateTimeExpression rhs)
        {
            return new ComparisonOperator(lhs, "==", rhs);
        }

        public static BooleanExpression operator >=(DateTimeExpression lhs, DateTimeExpression rhs)
        {
            return new ComparisonOperator(lhs, ">=", rhs);
        }

        public static BooleanExpression operator >(DateTimeExpression lhs, DateTimeExpression rhs)
        {
            return new ComparisonOperator(lhs, ">", rhs);
        }

        public static BooleanExpression operator !=(DateTimeExpression lhs, DateTimeExpression rhs)
        {
            return new ComparisonOperator(lhs, "!=", rhs);
        }

        // The compiler will complain if we don't have .Equals and .GetHashCode, but they really
        // don't mean much in our overloaded operator world.
        public override bool Equals(object obj)
        {
            throw new NotSupportedException(".Equals() and .GetHashCode() are not supported for this type");
        }

        public override int GetHashCode()
        {
            throw new NotSupportedException(".Equals() and .GetHashCode() are not supported for this type");
        }
    }

    public sealed class DateTimeStringLiteral : DateTimeExpression
    {
        private readonly string _value;

        public DateTimeStringLiteral(string value) => _value = value;

        public override void AppendIrisRepresentation(StringBuilder sb)
        {
            sb.Append('\'');
            sb.Append(_value);
            sb.Append('\'');
        }
    }

    public sealed class DateTimeDBDateTimeValue : DateTimeExpression
    {
        private readonly DBDateTime _value;

        public DateTimeDBDateTimeValue(DBDateTime value) => _value = value;

        public override void AppendIrisRepresentation(StringBuilder sb)
        {
            sb.Append('\'');
            sb.Append(_value);
            sb.Append('\'');
        }
    }
}
