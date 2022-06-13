/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Deephaven.OpenAPI.Core.API.Util;

namespace Deephaven.OpenAPI.Client.Fluent
{
    public abstract class StringExpression : Expression
    {
        public static implicit operator StringExpression(string s)
        {
            return new StringLiteral(s);
        }

        public static StringExpression operator +(StringExpression lhs, StringExpression rhs)
        {
            return Concat.Create(lhs, rhs);
        }

        public static BooleanExpression operator <(StringExpression lhs, StringExpression rhs)
        {
            return new ComparisonOperator(lhs, "<", rhs);
        }

        public static BooleanExpression operator <=(StringExpression lhs, StringExpression rhs)
        {
            return new ComparisonOperator(lhs, "<=", rhs);
        }

        public static BooleanExpression operator ==(StringExpression lhs, StringExpression rhs)
        {
            return new ComparisonOperator(lhs, "==", rhs);
        }

        public static BooleanExpression operator >=(StringExpression lhs, StringExpression rhs)
        {
            return new ComparisonOperator(lhs, ">=", rhs);
        }

        public static BooleanExpression operator >(StringExpression lhs, StringExpression rhs)
        {
            return new ComparisonOperator(lhs, ">", rhs);
        }

        public static BooleanExpression operator !=(StringExpression lhs, StringExpression rhs)
        {
            return new ComparisonOperator(lhs, "!=", rhs);
        }

        public BooleanExpression StartsWith(StringExpression e)
        {
            return new BooleanInstanceMethod(this, "startsWith", e);
        }

        public BooleanExpression EndsWith(StringExpression e)
        {
            return new BooleanInstanceMethod(this, "endsWith", e);
        }

        public BooleanExpression Contains(StringExpression e)
        {
            return new BooleanInstanceMethod(this, "contains", e);
        }

        public BooleanExpression Matches(StringExpression e)
        {
            return new BooleanInstanceMethod(this, "matches", e);
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

    public class StringLiteral : StringExpression
    {
        private readonly string _value;

        public StringLiteral(string value) => _value = value;

        public override void AppendIrisRepresentation(StringBuilder sb)
        {
            sb.Append('`');
            sb.Append(EscapeUtil.EscapeJava(_value));
            sb.Append('`');
        }
    }


    public class Concat : StringExpression
    {
        public static StringExpression Create(StringExpression lhs, StringExpression rhs)
        {
            var children = MaybeFlatten(lhs).Concat(MaybeFlatten(rhs)).ToArray();
            return new Concat(children);
        }

        private static IEnumerable<StringExpression> MaybeFlatten(StringExpression e)
        {
            return e is Concat c ? c._children : new[] {e};
        }

        private readonly StringExpression[] _children;

        private Concat(StringExpression[] children) => _children = children;

        public override void AppendIrisRepresentation(StringBuilder sb)
        {
            Action<StringBuilder, StringExpression> renderer = (sb2, se) => se.AppendIrisRepresentation(sb2);
            sb.Append('(');
            EnumerableUtil.AppendSeparatedList(sb, _children, " + ", renderer);
            sb.Append(')');
        }
    }
}
