/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System.Text;

namespace Deephaven.OpenAPI.Client.Fluent
{
    public interface ISelectColumn : IIrisRepresentable
    {
    }

    public interface IMatchWithColumn : IIrisRepresentable
    {
    }

    public class AssignedColumn : ISelectColumn
    {
        private readonly Expression _expr;
        private readonly string _asLabel;

        public AssignedColumn(Expression expr, string asLabel) => (_expr, _asLabel) = (expr, asLabel);

        public void AppendIrisRepresentation(StringBuilder sb)
        {
            sb.Append(_asLabel);
            sb.Append(" = ");
            _expr.AppendIrisRepresentation(sb);
        }
    }

    public class MatchWithColumn : IMatchWithColumn
    {
        private readonly IColumn _lhs;
        private readonly string _rhs;

        public MatchWithColumn(IColumn lhs, string rhs) => (_lhs, _rhs) = (lhs, rhs);

        public MatchWithColumn(IColumn lhs, IColumn rhs) : this(lhs, rhs.ToIrisRepresentation())
        {
        }

        public void AppendIrisRepresentation(StringBuilder sb)
        {
            _lhs.AppendIrisRepresentation(sb);
            sb.Append(" = ");
            sb.Append(_rhs);
        }
    }

    public interface IColumn : ISelectColumn, IMatchWithColumn
    {
        string Name { get; }
    }

    interface IColumn<T> : IColumn
    {

    }

    // Columns

    public abstract class NumCol : NumericExpression, IColumn
    {
        public string Name { get; }

        public NumCol(string name) => Name = name;

        public override void AppendIrisRepresentation(StringBuilder sb)
        {
            sb.Append(Name);
        }
    }

    public class NumCol<T> : NumCol, IColumn<T>
    {
        public NumCol(string name) : base(name)
        {
        }

        public override string ToString()
        {
            var genericArgType = GetType().GetGenericArguments()[0];
            return $"{Name} ({nameof(NumCol<T>)}<{genericArgType.Name}>)";
        }
    }

    public class StrCol : StringExpression, IColumn<string>
    {
        public string Name { get; }

        public StrCol(string name) => Name = name;

        public override void AppendIrisRepresentation(StringBuilder sb)
        {
            sb.Append(Name);
        }

        public override string ToString()
        {
            return $"{Name} ({nameof(StrCol)})";
        }
    }

    public class BoolCol : BooleanExpression, IColumn<bool>
    {
        public string Name { get; }

        public BoolCol(string name) => Name = name;

        public override void AppendIrisRepresentation(StringBuilder sb)
        {
            sb.Append(Name);
        }

        public override string ToString()
        {
            return $"{Name} ({nameof(BoolCol)})";
        }
    }

    public abstract class DateTimeCol : DateTimeExpression, IColumn
    {
        public string Name { get; }

        public DateTimeCol(string name) => Name = name;
    }

    public class DateTimeCol<T> : DateTimeCol, IColumn<T>
    {
        public DateTimeCol(string name) : base(name)
        {
        }

        public override void AppendIrisRepresentation(StringBuilder sb)
        {
            sb.Append(Name);
        }

        public override string ToString()
        {
            var type = GetType();
            var genericArgType = type.GetGenericArguments()[0];
            return $"{Name} ({nameof(DateTimeCol)}<{genericArgType.Name}>)";
        }
    }
}
