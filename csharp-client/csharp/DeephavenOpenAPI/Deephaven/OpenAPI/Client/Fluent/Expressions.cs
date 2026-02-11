/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Deephaven.OpenAPI.Client.Data;

namespace Deephaven.OpenAPI.Client.Fluent
{
    public interface IIrisRepresentable
    {
        void AppendIrisRepresentation(StringBuilder sb);
    }

    public static class IrisRepresentable_Extensions
    {
        public static string ToIrisRepresentation(this IIrisRepresentable i)
        {
            var sb = new StringBuilder();
            i.AppendIrisRepresentation(sb);
            return sb.ToString();
        }

        public static string[] ToIrisRepArray(this IEnumerable<IIrisRepresentable> columns)
        {
            return columns.Select(c => c.ToIrisRepresentation()).ToArray();
        }
    }

    public abstract class Expression : IIrisRepresentable
    {
        public BooleanExpression IsNull()
        {
            return new IsNull(this);
        }

        public abstract void AppendIrisRepresentation(StringBuilder sb);
    }


    public static class Expression_Extensions
    {
        public static ISelectColumn As(this bool value, string columnName)
        {
            return As((BooleanLiteral) value, columnName);
        }

        public static ISelectColumn As(this sbyte value, string columnName)
        {
            return As((NumericLiteral) value, columnName);
        }

        public static ISelectColumn As(this short value, string columnName)
        {
            return As((NumericLiteral) value, columnName);
        }

        public static ISelectColumn As(this int value, string columnName)
        {
            return As((NumericLiteral) value, columnName);
        }

        public static ISelectColumn As(this long value, string columnName)
        {
            return As((NumericLiteral) value, columnName);
        }

        public static ISelectColumn As(this float value, string columnName)
        {
            return As((NumericLiteral) value, columnName);
        }

        public static ISelectColumn As(this double value, string columnName)
        {
            return As((NumericLiteral) value, columnName);
        }

        public static ISelectColumn As(this DBDateTime value, string columnName)
        {
            return As((DateTimeExpression) value, columnName);
        }

        public static AssignedColumn As(this Expression self, string columnName)
        {
            return new AssignedColumn(self, columnName);
        }

        public static MatchWithColumn MatchWith(this IColumn self, string other)
        {
            return new MatchWithColumn(self, other);
        }

        public static MatchWithColumn MatchWith(this IColumn self, IColumn other)
        {
            return new MatchWithColumn(self, other);
        }
    }
}
