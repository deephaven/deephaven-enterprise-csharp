/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using NUnit.Framework;

namespace Deephaven.OpenAPI.Core.API.Util
{
    public class EscapeUtilTest
    {
        [Test]
        public void TestJavaEscape()
        {
            // test that escaped versions strings are equal to the string literal that would work in java
            Assert.AreEqual("A simple string", EscapeUtil.EscapeJava("A simple string"));
            Assert.AreEqual("A simple string with newline\\n", EscapeUtil.EscapeJava("A simple string with newline\n"));
        }

        [Test]
        public void TestJavaEscapeCtrlChars()
        {
            // We want to test all the control characters from https://docs.oracle.com/javase/tutorial/java/data/characters.html
            // \t, \b, \n, \r, \f, \", \', and \\
            Assert.AreEqual("\\tTab", EscapeUtil.EscapeJava("\tTab"));
            Assert.AreEqual("\\bBackspace", EscapeUtil.EscapeJava("\bBackspace"));
            Assert.AreEqual("\\nNewline", EscapeUtil.EscapeJava("\nNewline"));
            Assert.AreEqual("\\rCarriage Return", EscapeUtil.EscapeJava("\rCarriage Return"));
            Assert.AreEqual("\\fForm Feed", EscapeUtil.EscapeJava("\fForm Feed"));
            Assert.AreEqual("\\'SingleQuotes\\'", EscapeUtil.EscapeJava("'SingleQuotes'"));
            Assert.AreEqual("\\'SingleQuotes\\'", EscapeUtil.EscapeJava("\'SingleQuotes\'"));
            Assert.AreEqual("\\\"Double Quote", EscapeUtil.EscapeJava("\"Double Quote"));
            Assert.AreEqual("\\\\Backslash", EscapeUtil.EscapeJava("\\Backslash"));
        }

        [Test]
        public void TestJavaEscapeUnicode()
        {
            // single character unicode
            Assert.AreEqual("\\u03A9 - Omega", EscapeUtil.EscapeJava("\u03a9 - Omega"));

            // control character
            Assert.AreEqual("\\u0007 - Bell", EscapeUtil.EscapeJava("\u0007 - Bell"));

            // surrogate pair unicode
            string surrogatePairStr = "\ud83c\udf09 - Bridge at Night";
            Assert.AreEqual("\\uD83C\\uDF09 - Bridge at Night", EscapeUtil.EscapeJava(surrogatePairStr));
        }
    }
}
