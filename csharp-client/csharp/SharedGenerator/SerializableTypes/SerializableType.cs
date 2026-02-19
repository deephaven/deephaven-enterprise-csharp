/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Deephaven.OpenAPI.SharedGenerator.RPC.WebSockets.APT.Model;

namespace Deephaven.OpenAPI.SharedGenerator.SerializableTypes
{
    public abstract class SerializableType
    {
        public string FullTypeName { get; set; }

        protected readonly string _package;
        protected readonly string _typeName;

        public SerializableType(string fullTypeName, string parentTypeName)
        {
            FullTypeName = fullTypeName;

            // extract just the type name
            if (fullTypeName.Contains("."))
            {
                _typeName = fullTypeName.Substring(fullTypeName.LastIndexOf('.') + 1);
            }
            else
            {
                _typeName = fullTypeName;
            }

            // extract the namespace/package name
            if (parentTypeName != null)
            {
                _package = fullTypeName.Substring(0, fullTypeName.LastIndexOf('.') - parentTypeName.Length);
            }
            else
            {
                _package = fullTypeName.Substring(0, fullTypeName.LastIndexOf('.'));
            }
        }

        public SerializableType(string fullTypeName) : this(fullTypeName, null)
        {
        }

        public string GetFieldSerializerImplClassName(bool read, bool write, bool create)
        {
            var classNameBuilder = new StringBuilder();
            if (read)
            {
                classNameBuilder.Append("Read");
            }
            if (write)
            {
                classNameBuilder.Append("Write");
            }
            if (create)
            {
                classNameBuilder.Append("Instantiate");
            }
            return classNameBuilder.ToString();
        }

        public virtual string GetWriteMethodName() => "Write_" + FullTypeName.Replace("[]", "Array").Replace(".", "_");

        public virtual string GetReadMethodName() => "Read_" + FullTypeName.Replace("[]", "Array").Replace(".", "_");

        public abstract string GetStreamReaderMethodName();

        public abstract string GetStreamWriterMethodName();
    }
}
