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
    public abstract class BooleanExpression : Expression
    {
        public static implicit operator BooleanExpression(bool v)
        {
            return BooleanLiteral.Create(v);
        }

        public static BooleanExpression operator &(BooleanExpression lhs, BooleanExpression rhs)
        {
            return AndExpression.Create(lhs, rhs);
        }

        public static BooleanExpression operator |(BooleanExpression lhs, BooleanExpression rhs)
        {
            return OrExpression.Create(lhs, rhs);
        }

        public static BooleanExpression operator !(BooleanExpression e)
        {
            return NotExpression.Create(e);
        }

        // These exist to disable shortcutting on overloaded operators && and ||

        public static bool operator true(BooleanExpression be)
        {
            return false;
        }

        public static bool operator false(BooleanExpression be)
        {
            return false;
        }

        public static BooleanExpression operator ==(BooleanExpression lhs, BooleanExpression rhs)
        {
            return new ComparisonOperator(lhs, "==", rhs);
        }

        public static BooleanExpression operator !=(BooleanExpression lhs, BooleanExpression rhs)
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

    public class BooleanLiteral : BooleanExpression
    {
        public static BooleanLiteral Create(bool value) => new BooleanLiteral(value);

        private readonly bool _value;

        public BooleanLiteral(bool value) => _value = value;

        public override void AppendIrisRepresentation(StringBuilder sb)
        {
            sb.Append(_value ? "true" : "false");
        }
    }

    public class AndExpression : BooleanExpression
    {
        public static BooleanExpression Create(BooleanExpression lhs, BooleanExpression rhs)
        {
            // Try to reduce tree depth if we are and-ing 'and' nodes.
            var children = MaybeFlatten(lhs).Concat(MaybeFlatten(rhs)).ToArray();
            return new AndExpression(children);
        }

        private static IEnumerable<BooleanExpression> MaybeFlatten(BooleanExpression be)
        {
            return be is AndExpression ae ? ae._children : new[] {be};
        }

        private readonly BooleanExpression[] _children;

        private AndExpression(BooleanExpression[] children) => _children = children;

        public override void AppendIrisRepresentation(StringBuilder sb)
        {
            Action<StringBuilder, BooleanExpression> renderer = (sb2, ne) => ne.AppendIrisRepresentation(sb2);
            sb.Append('(');
            EnumerableUtil.AppendSeparatedList(sb, _children, " && ", renderer);
            sb.Append(')');
        }
    }

    public class OrExpression : BooleanExpression
    {
        public static BooleanExpression Create(BooleanExpression lhs, BooleanExpression rhs)
        {
            // Try to reduce tree depth if we are or-ing 'or' nodes.
            var children = MaybeFlatten(lhs).Concat(MaybeFlatten(rhs)).ToArray();
            return new OrExpression(children);
        }

        private static IEnumerable<BooleanExpression> MaybeFlatten(BooleanExpression be)
        {
            return be is OrExpression oe ? oe._children : new[] {be};
        }

        private readonly BooleanExpression[] _children;

        private OrExpression(BooleanExpression[] children) => _children = children;

        public override void AppendIrisRepresentation(StringBuilder sb)
        {
            Action<StringBuilder, BooleanExpression> renderer = (sb2, ne) => ne.AppendIrisRepresentation(sb2);
            sb.Append('(');
            EnumerableUtil.AppendSeparatedList(sb, _children, " || ", renderer);
            sb.Append(')');
        }
    }

    public class NotExpression : BooleanExpression
    {
        public static BooleanExpression Create(BooleanExpression e)
        {
            if (e is NotExpression ne)
            {
                // !!x == x
                return ne._child;
            }
            return new NotExpression(e);
        }

        private readonly BooleanExpression _child;

        private NotExpression(BooleanExpression child) => _child = child;

        public override void AppendIrisRepresentation(StringBuilder sb)
        {
            sb.Append('(');
            sb.Append('!');
            _child.AppendIrisRepresentation(sb);
            sb.Append(')');
        }
    }

    public class IsNull : BooleanExpression
    {
        private readonly Expression _expr;

        public IsNull(Expression expr) => _expr = expr;

        public override void AppendIrisRepresentation(StringBuilder sb)
        {
            sb.Append("isNull(");
            _expr.AppendIrisRepresentation(sb);
            sb.Append(')');
        }
    }

    public class ComparisonOperator : BooleanExpression
    {
        private readonly Expression _lhs;
        private readonly string _op;
        private readonly Expression _rhs;

        internal ComparisonOperator(Expression lhs, string op, Expression rhs) => (_lhs, _op, _rhs) = (lhs, op, rhs);

        public override void AppendIrisRepresentation(StringBuilder sb)
        {
            sb.Append('(');
            _lhs.AppendIrisRepresentation(sb);
            sb.Append($" {_op} ");
            _rhs.AppendIrisRepresentation(sb);
            sb.Append(')');
        }
    }

    public class BooleanInstanceMethod : BooleanExpression
    {
        private readonly Expression _lhs;
        private readonly string _method;
        private readonly Expression[] _args;

        public BooleanInstanceMethod(Expression lhs, string method, params Expression[] args) =>
            (_lhs, _method, _args) = (lhs, method, args);

        public override void AppendIrisRepresentation(StringBuilder sb)
        {
            sb.Append('(');

            // lhs.method(rhs...)
            _lhs.AppendIrisRepresentation(sb);
            sb.Append('.');
            sb.Append(_method);
            sb.Append('(');
            Action<StringBuilder, Expression> renderer = (sb2, e) => e.AppendIrisRepresentation(sb2);
            EnumerableUtil.AppendSeparatedList(sb, _args, ", ", renderer);
            sb.Append(')');

            sb.Append(')');
        }
    }
}
