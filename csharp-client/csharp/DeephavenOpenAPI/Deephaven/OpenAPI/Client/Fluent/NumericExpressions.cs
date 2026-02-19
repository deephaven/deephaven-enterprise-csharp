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
    public abstract class NumericExpression : Expression
    {
        public static NumericExpression operator +(NumericExpression e)
        {
            return NumericUnaryOperator.Create("+", e);
        }

        public static NumericExpression operator -(NumericExpression e)
        {
            return NumericUnaryOperator.Create("-", e);
        }

        public static implicit operator NumericExpression(sbyte v)
        {
            return NumericLiteral.Create(v);
        }

        public static implicit operator NumericExpression(short v)
        {
            return NumericLiteral.Create(v);
        }

        public static implicit operator NumericExpression(int v)
        {
            return NumericLiteral.Create(v);
        }

        public static implicit operator NumericExpression(long v)
        {
            return NumericLiteral.Create(v);
        }

        public static implicit operator NumericExpression(float v)
        {
            return NumericLiteral.Create(v);
        }

        public static implicit operator NumericExpression(double v)
        {
            return NumericLiteral.Create(v);
        }

        public static NumericExpression operator +(NumericExpression lhs, NumericExpression rhs)
        {
            return NumericBinaryOperator.Create(lhs, "+", rhs);
        }

        public static NumericExpression operator -(NumericExpression lhs, NumericExpression rhs)
        {
            return NumericBinaryOperator.Create(lhs, "-", rhs);
        }

        public static NumericExpression operator *(NumericExpression lhs, NumericExpression rhs)
        {
            return NumericBinaryOperator.Create(lhs, "*", rhs);
        }

        public static NumericExpression operator /(NumericExpression lhs, NumericExpression rhs)
        {
            return NumericBinaryOperator.Create(lhs, "/", rhs);
        }

        public static NumericExpression operator %(NumericExpression lhs, NumericExpression rhs)
        {
            return NumericBinaryOperator.Create(lhs, "%", rhs);
        }

        public static NumericExpression operator &(NumericExpression lhs, NumericExpression rhs)
        {
            return NumericBinaryOperator.Create(lhs, "&", rhs);
        }

        public static NumericExpression operator |(NumericExpression lhs, NumericExpression rhs)
        {
            return NumericBinaryOperator.Create(lhs, "|", rhs);
        }

        public static NumericExpression operator ^(NumericExpression lhs, NumericExpression rhs)
        {
            return NumericBinaryOperator.Create(lhs, "^", rhs);
        }

        public static BooleanExpression operator <(NumericExpression lhs, NumericExpression rhs)
        {
            return new ComparisonOperator(lhs, "<", rhs);
        }

        public static BooleanExpression operator <=(NumericExpression lhs, NumericExpression rhs)
        {
            return new ComparisonOperator(lhs, "<=", rhs);
        }

        public static BooleanExpression operator ==(NumericExpression lhs, NumericExpression rhs)
        {
            return new ComparisonOperator(lhs, "==", rhs);
        }

        public static BooleanExpression operator >=(NumericExpression lhs, NumericExpression rhs)
        {
            return new ComparisonOperator(lhs, ">=", rhs);
        }

        public static BooleanExpression operator >(NumericExpression lhs, NumericExpression rhs)
        {
            return new ComparisonOperator(lhs, ">", rhs);
        }

        public static BooleanExpression operator !=(NumericExpression lhs, NumericExpression rhs)
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

    public abstract class NumericLiteral : NumericExpression
    {
        public static NumericLiteral<T> Create<T>(T value) => new NumericLiteral<T>(value);
    }

    public class NumericLiteral<T> : NumericLiteral
    {
        private readonly T _value;

        public NumericLiteral(T value) => _value = value;

        public override void AppendIrisRepresentation(StringBuilder sb)
        {
            sb.Append(_value);
        }
    }

    public class NumericUnaryOperator : NumericExpression
    {
        public static NumericExpression Create(string op, NumericExpression ne)
        {
            return op == "+" ? ne : new NumericUnaryOperator(op, ne);
        }

        private readonly string _op;
        private readonly NumericExpression _child;

        private NumericUnaryOperator(string op, NumericExpression child) => (_op, _child) = (op, child);

        public override void AppendIrisRepresentation(StringBuilder sb)
        {
            sb.Append(_op);
            _child.AppendIrisRepresentation(sb);
        }
    }

    public class NumericBinaryOperator : NumericExpression
    {
        public static NumericExpression Create(NumericExpression lhs, string op, NumericExpression rhs)
        {
            var children = MaybeFlatten(op, lhs).Concat(MaybeFlatten(op, rhs)).ToArray();
            return new NumericBinaryOperator(op, children);
        }

        private static IEnumerable<NumericExpression> MaybeFlatten(string op, NumericExpression ne)
        {
            return ne is NumericBinaryOperator nbo && nbo._op == op ? nbo._children : new[] {ne};
        }

        private readonly string _op;
        private readonly NumericExpression[] _children;

        private NumericBinaryOperator(string op, NumericExpression[] children) => (_op, _children) = (op, children);

        public override void AppendIrisRepresentation(StringBuilder sb)
        {
            Action<StringBuilder, NumericExpression> renderer = (sb2, ne) => ne.AppendIrisRepresentation(sb2);
            sb.Append('(');
            EnumerableUtil.AppendSeparatedList(sb, _children, $" {_op} ", renderer);
            sb.Append(')');
        }
    }
}
