/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System.Text;

namespace Deephaven.OpenAPI.SharedGenerator
{
    public class Util
    {
        public static string ToUpperCamel(string input)
        {
            var tokens = input.Split('.');
            var b = new StringBuilder();
            for (var i = 0; i < tokens.Length; i++)
            {
                if (i > 0)
                    b.Append('.');
                b.Append(char.ToUpper(tokens[i][0]) + tokens[i].Substring(1));
            }
            return b.ToString();
        }
    }
}
