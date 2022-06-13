/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using Deephaven.OpenAPI.Core.RPC.Serialization.API;
using Deephaven.OpenAPI.Core.RPC.Serialization.Java.Math;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.SharedGenerator.SerializableTypes
{
    /// <summary>
    /// Singleton utility for mapping java packages and types to C#
    /// </summary>
    public class SerializableTypeMapper
    {
        /// <summary>
        /// Map java package names to C# namespaces.
        /// </summary>
        private Dictionary<string, string> _namespaceDictionary = null;

        public class TypeInfo
        {
            private const string IntrinsicTypeSerializerNamespace = "Deephaven.OpenAPI.RPC.Serialization.Intrinsic";

            public string FullTypeName { get; set; }
            public string FieldSerializerTypeName { get; set; }
            public string FieldSerializerPackage { get; set; }

            public TypeInfo(string fullTypeName, string parentTypeName)
            {
                FullTypeName = fullTypeName;

                // extract just the type name for the field serializer type name
                if (fullTypeName.Contains('.'))
                {
                    FieldSerializerTypeName = FullTypeName.Replace("?", "Nullable")
                        .Replace("[]","Array").Substring(fullTypeName.LastIndexOf('.') + 1) + "_FieldSerializer";
                }
                else
                {
                    FieldSerializerTypeName = FullTypeName.Replace("?", "Nullable")
                        .Replace("[]", "Array") + "_FieldSerializer";
                }

                if (parentTypeName != null)
                {
                    FieldSerializerTypeName = parentTypeName.Replace(".", "_") + "_" + FieldSerializerTypeName;
                }

                // extract the namespace/package name
                if (parentTypeName != null)
                {
                    FieldSerializerPackage = fullTypeName.Substring(0, fullTypeName.LastIndexOf('.') - parentTypeName.Length - 1);
                }
                else
                {
                    if (fullTypeName.StartsWith("System", StringComparison.InvariantCulture))
                    {
                        FieldSerializerPackage = IntrinsicTypeSerializerNamespace;
                    }
                    else
                    {
                        FieldSerializerPackage = fullTypeName.Substring(0, fullTypeName.LastIndexOf('.'));
                    }
                }
            }

            public TypeInfo(Type type) : this(type,
                type.Name+"_FieldSerializer", "Deephaven.OpenAPI.RPC.Serialization.Intrinsic")
            {
            }

            public TypeInfo(Type type, string fieldSerializerTypeName)
                : this(type, fieldSerializerTypeName, "Deephaven.OpenAPI.RPC.Serialization.Intrinsic")
            {
            }

            public TypeInfo(Type type, string fieldSerializerTypeName, string fieldSerializerPackage)
            {
                FullTypeName = type.FullName;
                FieldSerializerPackage = fieldSerializerPackage;
                FieldSerializerTypeName = fieldSerializerTypeName;
            }
        }

        private Dictionary<string, TypeInfo> _intrinsicTypeDictionary
            = new Dictionary<string, TypeInfo>
        {
            { "byte", new TypeInfo(typeof(byte)) },
            { "boolean", new TypeInfo(typeof(Boolean)) },
            { "long", new TypeInfo(typeof(Int64)) },
            { "int", new TypeInfo(typeof(Int32)) },
            { "short", new TypeInfo(typeof(Int16)) },
            { "char", new TypeInfo(typeof(Char)) },
            { "double", new TypeInfo(typeof(Double)) },
            { "float", new TypeInfo(typeof(Single)) },
            { "void", new TypeInfo(typeof(Core.RPC.Serialization.Java.Lang.Void)) },
            { "java.lang.Byte", new TypeInfo(typeof(sbyte?),  "Byte_FieldSerializer") },
            { "java.lang.Boolean", new TypeInfo(typeof(bool?), "Boolean_FieldSerializer") },
            { "java.lang.Long", new TypeInfo(typeof(long?), "Long_FieldSerializer") },
            { "java.lang.Integer", new TypeInfo(typeof(int?), "Integer_FieldSerializer") },
            { "java.lang.Short", new TypeInfo(typeof(short?), "Short_FieldSerializer") },
            { "java.lang.Character", new TypeInfo(typeof(char?), "Character_FieldSerializer") },
            { "java.lang.Double", new TypeInfo(typeof(double?), "Double_FieldSerializer") },
            { "java.lang.Float", new TypeInfo(typeof(float?), "Float_FieldSerializer") },
            { "java.util.BitSet", new TypeInfo(typeof(BitArray), "BitSet_FieldSerializer") },
            { "java.lang.String", new TypeInfo(typeof(string), "String_FieldSerializer") },
            { "java.lang.Void", new TypeInfo(typeof(Core.RPC.Serialization.Java.Lang.Void?), "NullableVoid_FieldSerializer") },
            { "java.lang.Object", new TypeInfo(typeof(object), "Object_FieldSerializer") },
            { "java.lang.Number", new TypeInfo(typeof(object), "Number_FieldSerializer") }, // we have no base class for numeric types in C# so treat this like an object
            { "java.math.BigDecimal", new TypeInfo(typeof(BigDecimal?), "BigDecimal_FieldSerializer") },
            { "java.math.BigInteger", new TypeInfo(typeof(BigInteger?), "BigInteger_FieldSerializer") }
        };

        private SerializableTypeMapper()
        {
            // load package->namespace mappings
            _namespaceDictionary = new Dictionary<string, string>();
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith("package_namespace.txt"));
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var tokens = line.Split(null)
                            .Select(x => x.Trim())
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .ToArray();
                        if (tokens.Length == 2)
                        {
                            _namespaceDictionary[tokens[0].Trim()] = tokens[1].Trim();
                        }
                        else if(tokens.Length != 0) // we permit blank lines
                        {
                            throw new InvalidOperationException(string.Format("Package-namespace mapping specified {0} tokens, " +
                                "2 tokens expected on line {1}.", tokens.Length, line));
                        }
                    }
                }
            }
        }

        private static SerializableTypeMapper _serializableTypeMapper = null;

        public static SerializableTypeMapper GetInstance()
        {
            if (_serializableTypeMapper == null)
            {
                _serializableTypeMapper = new SerializableTypeMapper();
            }
            return _serializableTypeMapper;
        }

        public static bool IsTypeDefined(string fullTypeName)
        {
            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetType(fullTypeName) != null)
                    return true;
            }
            var coreAssembly = Assembly.GetAssembly(typeof(FieldSerializer));
            var sharedCustomAssembly = Assembly.GetAssembly(typeof(TableHandle));
            // we need the general "Type.GetType" for types that have the assembly specified in the full type name
            return (Type.GetType(fullTypeName) != null
                   || coreAssembly.GetType(fullTypeName) != null
                   || sharedCustomAssembly.GetType(fullTypeName) != null);
        }

        public static Type GetSerializableType(string fullTypeName)
        {
            Type type = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if ((type = assembly.GetType(fullTypeName)) != null)
                    return type;
            }
            var coreAssembly = Assembly.GetAssembly(typeof(FieldSerializer));
            var sharedCustomAssembly = Assembly.GetAssembly(typeof(TableHandle));
            // we need the general "Type.GetType" for types that have the assembly specified in the full type name
            type = Type.GetType(fullTypeName);
            if (type != null)
            {
                return type;
            }
            type = coreAssembly.GetType(fullTypeName);
            if (type != null)
            {
                return type;
            }
            type = sharedCustomAssembly.GetType(fullTypeName);
            if (type != null)
            {
                return type;
            }
            return null;
        }

        public TypeInfo MapType(string className, string parentTypeName)
        {
            TypeInfo typeInfo;
            if (_intrinsicTypeDictionary.TryGetValue(className, out typeInfo))
            {
                return typeInfo;
            }
            else
            {
                if (className.Contains('.'))
                {
                    var package = parentTypeName == null
                        ? className.Substring(0, className.LastIndexOf('.'))
                        : className.Substring(0, className.LastIndexOf('.') - parentTypeName.Length - 1);
                    var typeName = className.Substring(package.Length);
                    if (parentTypeName != null)
                    {
                        var subTypeName = typeName.Substring(typeName.LastIndexOf('.') + 1);

                        // if the type is nested, add the parent type to the name of the child to avoid name clashes with properties that have the same name
                        typeName = typeName.Substring(0, typeName.Length - subTypeName.Length)
                                   + parentTypeName
                                   + subTypeName;
                    }

                    KeyValuePair<string, string>? mapping = null;
                    foreach (var entry in _namespaceDictionary)
                    {
                        // if the original package is contained, use the longest replacement (this way we can specify sub-packages to replace if we want)
                        if (package.Contains(entry.Key) &&
                            (!mapping.HasValue || entry.Value.Length > mapping.Value.Value.Length))
                        {
                            mapping = entry;
                        }
                    }

                    if (mapping.HasValue)
                    {
                        package = package.Replace(mapping.Value.Key, mapping.Value.Value);
                    }

                    // convert any remaining java parts of the package name to C# style
                    return new TypeInfo(ToUpperCamel(package) + typeName, parentTypeName);
                }
                else
                {
                    throw new Exception(string.Format("Type {0} contains no package name and is not mapped to intrinsic type.",
                        className));
                }
            }
        }

        public string MapPackageName(string package)
        {
            KeyValuePair<string, string>? mapping = null;
            foreach (var entry in _namespaceDictionary)
            {
                // if the original package is contained, use the longest replacement (this way we can specify sub-packages to replace if we want)
                if (package.Contains(entry.Key) &&
                    (!mapping.HasValue || entry.Value.Length > mapping.Value.Value.Length))
                {
                    mapping = entry;
                }
            }

            if (mapping.HasValue)
            {
                package = package.Replace(mapping.Value.Key, mapping.Value.Value);
            }

            // convert any remaining java parts of the package name to C# style
            return ToUpperCamel(package);
        }

        private string ToUpperCamel(string input)
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
