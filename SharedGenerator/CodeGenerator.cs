/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using Deephaven.OpenAPI.Core.API;
using Deephaven.OpenAPI.SharedGenerator.RPC.WebSockets.APT.Model;
using Deephaven.OpenAPI.SharedGenerator.SerializableTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Deephaven.OpenAPI.SharedGenerator
{
    public class CodeGenerator
    {
        /// <summary>
        /// Target directory for codegen
        /// </summary>
        private readonly string _outputRoot;

        /// <summary>
        /// Map of typeId to serializable type object (includes primitives and types sourced from JSON).
        /// </summary>
        private readonly Dictionary<string, SerializableType> _serializableTypes;

        /// <summary>
        /// List synthetic types for which to generate code (includes composites & enums). Child types are included as children of "root" types.
        /// </summary>
        private readonly List<SyntheticType> _syntheticTypes = new List<SyntheticType>();

        /// <summary>
        /// List of array types for which to generate code.
        /// </summary>
        private readonly List<ArraySerializableType> _arrayTypes = new List<ArraySerializableType>();

        private readonly ServerEndpoint _serverEndpoint;

        private readonly Endpoint _clientEndpoint;

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private class CompareLength : IComparer<string>
        {
            public int Compare(string a, string b)
            {
                return a.Length.CompareTo(b.Length);
            }
        }

        public CodeGenerator(string outputRoot, string serverEndpointJson, string serverTypesJson,
            string clientEndpointJson, string clientTypesJson, int defaultTimeoutMs) :
            this(outputRoot,
                JsonConvert.DeserializeObject<RpcEndpoint>(serverEndpointJson, SerializerSettings),
                JsonConvert.DeserializeObject<RpcSerializer>(serverTypesJson, SerializerSettings),
                JsonConvert.DeserializeObject<RpcEndpoint>(clientEndpointJson, SerializerSettings),
                JsonConvert.DeserializeObject<RpcSerializer>(clientTypesJson, SerializerSettings),
                defaultTimeoutMs)
        {
        }

        private class SerializableTypeInfo
        {
            public string TypeId { get; set; }
            public RpcSerializableType Type { get; set; }
        }

        public CodeGenerator(string outputRoot,
            RpcEndpoint rpcServerEndpoint, RpcSerializer serverSerializableTypes,
            RpcEndpoint rpcClientEndpoint, RpcSerializer clientSerializableTypes,
            int defaultTimeoutMs)
        {
            _outputRoot = outputRoot;

            // initialize types map with primitives
            _serializableTypes = new Dictionary<string, SerializableType>(PrimitiveSerializableType.PrimitiveTypes);

            // first sort types by length of the type name so that our nesting logic works (child types must come after parents)
            List<KeyValuePair<string, RpcSerializableType>> sortedTypeList =
                new List<KeyValuePair<string, RpcSerializableType>>();
            foreach (var entry in serverSerializableTypes.SerializableTypes)
            {
                sortedTypeList.Add(entry);
            }
            sortedTypeList.Sort((e1, e2) => e1.Value.Name.Length.CompareTo(e2.Value.Name.Length));

            // add each type from the JSON
            foreach (var entry in sortedTypeList)
            {
                SerializableType serializableType;
                SerializableTypeMapper.TypeInfo typeInfo;
                switch (entry.Value.Kind)
                {
                    case RpcSerializableTypeKind.Composite:
                    {
                        SyntheticType parentType;
                        var parentTypeName = FindParentType(entry.Value, out parentType);
                        typeInfo = SerializableTypeMapper.GetInstance()
                            .MapType(entry.Value.Name, parentTypeName);
                        serializableType = new CompositeSerializableType(entry.Value, typeInfo.FullTypeName, parentTypeName,
                            typeInfo.FieldSerializerPackage, typeInfo.FieldSerializerTypeName);
                        if (parentType != null)
                        {
                            Console.WriteLine("Adding composite type {0} as child of type {1}", serializableType.FullTypeName,
                                parentType.FullTypeName);
                            parentType.Children.Add((CompositeSerializableType)serializableType);
                        }
                        else
                        {
                            _syntheticTypes.Add((CompositeSerializableType)serializableType);
                        }
                        break;
                    }
                    case RpcSerializableTypeKind.Enum:
                    {
                        SyntheticType parentType;
                        var parentTypeName = FindParentType(entry.Value, out parentType);
                        typeInfo = SerializableTypeMapper.GetInstance()
                            .MapType(entry.Value.Name, parentTypeName);
                        serializableType = new EnumSerializableType(entry.Value, typeInfo.FullTypeName, parentTypeName,
                            typeInfo.FieldSerializerPackage, typeInfo.FieldSerializerTypeName);
                        if (parentType != null)
                        {
                            Console.WriteLine("Adding enum type {0} as child of type {1}", serializableType.FullTypeName,
                                parentType.FullTypeName);
                            parentType.Children.Add((EnumSerializableType)serializableType);
                        }
                        else
                        {
                            _syntheticTypes.Add((EnumSerializableType)serializableType);
                        }
                        break;
                    }
                    case RpcSerializableTypeKind.Array:
                    {
                        var componentTypeName = entry.Value.Name.Replace("[]", "");
                        var componentType = GetSerializableType(componentTypeName);
                        if (componentType == null)
                        {
                            throw new Exception("Unable to define array type, component type not defined for " + entry.Value);
                        }
                        string componentFullTypeName = componentType.FullTypeName;
                        Type actualComponentType = Type.GetType(componentType.FullTypeName);
                        if (actualComponentType != null)
                        {
                            Type componentUnderlyingType = Nullable.GetUnderlyingType(actualComponentType);
                            if (componentUnderlyingType != null)
                            {
                                componentFullTypeName = componentUnderlyingType.FullName + "?";
                            }
                        }
                        var fullTypeName =
                            componentFullTypeName + entry.Value.Name.Substring(entry.Value.Name.IndexOf("[]", StringComparison.InvariantCulture));

                        // we use the parent type of the component type for the array field serializer
                        string parentTypeName = null;
                        if (componentType is SyntheticType)
                        {
                            SyntheticType parentType = null;
                            parentTypeName = FindParentType(((SyntheticType)componentType).RpcSerializableType, out parentType);
                        }
                        typeInfo = new SerializableTypeMapper.TypeInfo(fullTypeName, parentTypeName);

                        serializableType = new ArraySerializableType(entry.Value, typeInfo.FullTypeName,
                            typeInfo.FieldSerializerPackage, typeInfo.FieldSerializerTypeName);
                        _arrayTypes.Add((ArraySerializableType)serializableType);
                        break;
                    }
                    default:
                        throw new Exception("Unknown serializable type kind: " + entry.Value.Kind);
                }

                Console.WriteLine("Creating type {0} -> {1} with field serializer {2} in package {3}",
                    entry.Value.Name, typeInfo.FullTypeName, typeInfo.FieldSerializerTypeName, typeInfo.FieldSerializerPackage);

                _serializableTypes.Add(entry.Key, serializableType);
            }
            foreach (var entry in clientSerializableTypes.SerializableTypes)
            {
                if (!_serializableTypes.ContainsKey(entry.Key))
                {
                    throw new Exception("Found type in client that doesn't exist in server: " + entry.Key);
                }
            }
            _serverEndpoint = new ServerEndpoint(serverSerializableTypes.SerializerHash,
                _serializableTypes, rpcServerEndpoint, defaultTimeoutMs,
                SerializableTypeMapper.GetInstance().MapPackageName(rpcServerEndpoint.EndpointPackage),
                rpcServerEndpoint.EndpointInterface, rpcClientEndpoint);

            _clientEndpoint = new Endpoint(_serializableTypes, rpcClientEndpoint, defaultTimeoutMs,
                SerializableTypeMapper.GetInstance().MapPackageName(rpcClientEndpoint.EndpointPackage),
                rpcClientEndpoint.EndpointInterface);
        }

        public void GenerateSyntheticTypes()
        {
            foreach(var entry in _syntheticTypes)
            {
                // generate DTO/bean object if needed (some types have custom definitions in which case this is a no-op)
                entry.GenerateTypeDeclaration(GetSerializableType, GenerateCSharpCode);

                // generate class to serialize/deserialize the type
                entry.GenerateFieldSerializer(GetSerializableType, GenerateCSharpCode);
            }
        }

        public void GenerateArrayTypes()
        {
            foreach (var entry in _arrayTypes)
            {
                // generate class to serialize/deserialize the type
                entry.GenerateFieldSerializer(GetSerializableType, GenerateCSharpCode);
            }
        }

        public void GenerateServerEndpoint()
        {
            // the interface the client will use to interact with the service
            _serverEndpoint.GenerateEndpointInterface(new CodeTypeReference(typeof(IServer<,>))
            {
                TypeArguments =
                {
                    _serverEndpoint.EndpointInterface,
                    _clientEndpoint.EndpointInterface
                }
            }, GetSerializableType, GenerateCSharpCode);


            // the field serializer factory for all custom types defined for this endpoint
            _serverEndpoint.GenerateTypeSerializer(GetSerializableType, GenerateCSharpCode);

            // generate the typed serializer class impl for all types in this endpoint
            _serverEndpoint.GenerateEndpointSerializer(GetSerializableType, GenerateCSharpCode);

            // generate the server impl
            _serverEndpoint.GenerateServerEndpointImpl(GetSerializableType, GenerateCSharpCode);
        }

        public void GenerateClientInterface()
        {
            // interface that receives calls from the server (we don't implement this)
            _clientEndpoint.GenerateEndpointInterface(new CodeTypeReference(typeof(IClient<,>))
            {
                TypeArguments =
                {
                    _clientEndpoint.EndpointInterface,
                    _serverEndpoint.EndpointInterface
                }
            }, GetSerializableType, GenerateCSharpCode);
        }

        /// <summary>
        /// Search types for which one is the parent of the given type, if any (this is recursive in SyntheticType).
        /// </summary>
        /// <param name="rpcSerializableType"></param>
        /// <param name="parentType"></param>
        /// <returns></returns>
        private string FindParentType(RpcSerializableType rpcSerializableType, out SyntheticType parentType)
        {
            foreach (var parentCandidateEntry in _serializableTypes)
            {
                if (parentCandidateEntry.Value is SyntheticType)
                {
                    var typePrefix = ((SyntheticType) parentCandidateEntry.Value).FindParent(rpcSerializableType, out parentType);
                    if (typePrefix != null)
                    {
                        return typePrefix;
                    }
                }
            }
            parentType = null;
            return null;
        }

        /// <summary>
        /// Generate the given code unit into a directory corresponding to the given package, with the given filename (.cs extension is added).
        /// </summary>
        /// <param name="package"></param>
        /// <param name="name"></param>
        /// <param name="targetUnit"></param>
        protected void GenerateCSharpCode(CodeCompileUnit targetUnit, string package, string name)
        {
            var dir = _outputRoot
                      + Path.DirectorySeparatorChar
                      + package.Replace('.', Path.DirectorySeparatorChar);
            Directory.CreateDirectory(dir);
            var fileName = dir + Path.DirectorySeparatorChar + name + ".cs";
            var provider = CodeDomProvider.CreateProvider("CSharp");
            var options = new CodeGeneratorOptions
            {
                BracingStyle = "C"
            };
            using (var sourceWriter = new StreamWriter(fileName))
            {
                provider.GenerateCodeFromCompileUnit(targetUnit, sourceWriter, options);
            }
        }

        /// <summary>
        /// Given a type id, return the corresponding serializable type
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        protected SerializableType GetSerializableType(string typeId)
        {
            SerializableType rpcSerializableType;
            if (_serializableTypes.TryGetValue(typeId, out rpcSerializableType))
            {
                return rpcSerializableType;
            }
            throw new ArgumentException("No serializable type with type id: " + typeId);
        }

        private const string FieldSerializerMapName = "_fieldSerializerMap";
        private const string SerializationSignatureMapName = "_serializationSignatureMap";
    }
}
