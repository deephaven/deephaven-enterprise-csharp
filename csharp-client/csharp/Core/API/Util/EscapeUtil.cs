/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Text;

namespace Deephaven.OpenAPI.Core.API.Util
{
    public static class EscapeUtil
    {
        public static string EscapeJava(string s) => EscapeJavaOrPython(s);

        public static string EscapePython(string s) => EscapeJavaOrPython(s);

        private static string EscapeJavaOrPython(string s)
        {
            var sb = new StringBuilder();
            foreach (var ch in s)
            {
                switch (ch)
                {
                    case '\b':
                        sb.Append("\\b");
                        continue;
                    case '\f':
                        sb.Append("\\f");
                        continue;
                    case '\n':
                        sb.Append("\\n");
                        continue;
                    case '\r':
                        sb.Append("\\r");
                        continue;
                    case '\t':
                        sb.Append("\\t");
                        continue;
                    case '"':
                    case '\'':
                    case '\\':
                        sb.Append('\\');
                        sb.Append(ch);
                        continue;
                }

                if (ch < 32 || ch > 0x7f)
                {
                    sb.Append($"\\u{(uint) ch:X4}");
                    continue;
                }
                sb.Append(ch);
            }

            return sb.ToString();
        }
    }
}
