/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Deephaven.OpenAPI.Core.API.Util
{
    public static class EnumerableUtil
    {
        public static void AppendSeparatedList<T>(StringBuilder sb, IEnumerable<T> items,
            string separator, Action<StringBuilder, T> renderer)
        {
            var sepToUse = "";
            foreach (var item in items)
            {
                sb.Append(sepToUse);
                sepToUse = separator;
                renderer(sb, item);
            }
        }
    }

    public static class EnumerableExtensions
    {
        public static string MakeCommaSeparatedList<T>(this IEnumerable<T> items)
        {
            return items.MakeSeparatedList(", ", null);
        }

        public static string MakeSeparatedList<T>(this IEnumerable<T> items, string separator,
            Func<T, string> renderer = null)
        {
            renderer = renderer ?? (item => item.ToString());
            Action<StringBuilder, T> rendererToUse = (sb, item) => sb.Append(renderer(item));
            var result = new StringBuilder();
            EnumerableUtil.AppendSeparatedList(result, items, separator, rendererToUse);
            return result.ToString();
        }
    }
}
