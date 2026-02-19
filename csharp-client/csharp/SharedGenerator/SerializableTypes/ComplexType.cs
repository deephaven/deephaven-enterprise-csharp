/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.CodeDom;
using Deephaven.OpenAPI.Core.RPC.Serialization.API;
using Deephaven.OpenAPI.SharedGenerator.RPC.WebSockets.APT.Model;

namespace Deephaven.OpenAPI.SharedGenerator.SerializableTypes
{
    /// <summary>
    /// Abstract base class for all complex serializable types (those for which we much generate serializers).
    /// </summary>
    public abstract class ComplexType : SerializableType
    {
        public readonly RpcSerializableType RpcSerializableType;

        public string FieldSerializerImplTypeName = "ReadWriteInstantiate";

        private readonly string _fieldSerializerPackage;
        private readonly string _fieldSerializerTypeName;

        private readonly string _parentType;

        public string FieldSerializerPackage => _fieldSerializerPackage;
        public string FieldSerializerTypeName => _fieldSerializerTypeName;

        public ComplexType(RpcSerializableType rpcSerializableType, string fullTypeName,
            string parentType, string fieldSerializerPackage, string fieldSerializerTypeName) : base(fullTypeName)
        {
            RpcSerializableType = rpcSerializableType;
            _parentType = parentType;
            _fieldSerializerPackage = fieldSerializerPackage;
            _fieldSerializerTypeName = fieldSerializerTypeName;
        }

        public override string GetWriteMethodName() => "Write_" + RpcSerializableType.Name.Replace("[]", "Array").Replace(".", "_");

        public override string GetReadMethodName() => "Read_" + RpcSerializableType.Name.Replace("[]", "Array").Replace(".", "_");

        /// <summary>
        /// An abstract method that should implement the serializer methods according to the concrete type.
        /// </summary>
        /// <param name="deserializeMethod"></param>
        /// <param name="serializeMethod"></param>
        /// <param name="instantiateMethod"></param>
        /// <param name="getSerializableType"></param>
        protected abstract void GenerateSerializerBody(CodeMemberMethod deserializeMethod,
            CodeMemberMethod serializeMethod, CodeMemberMethod instantiateMethod,
            Func<string, SerializableType> getSerializableType);

        private CodeTypeReferenceExpression GetCustomSerializerCodeTypeReferenceExpression(string typeName)
        {
            return new CodeTypeReferenceExpression(SerializableTypeMapper.GetInstance().MapType(typeName, null).FullTypeName);
        }

        public override string GetStreamReaderMethodName()
        {
            if (string.Compare(RpcSerializableType.Name, "java.lang.String") == 0)
            {
                return "ReadString";
            }
            return "ReadObject";
        }

        public override string GetStreamWriterMethodName()
        {
            if (string.Compare(RpcSerializableType.Name, "java.lang.String") == 0)
            {
                return "Write";
            }
            return "WriteObject";
        }

        protected abstract void GenerateChildFieldSerializers(Func<string, SerializableType> getSerializableType,
            Action<CodeCompileUnit, string, string> generateCSharpCode);

        public void GenerateFieldSerializer(Func<string, SerializableType> getSerializableType,
            Action<CodeCompileUnit, string, string> generateCSharpCode)
        {
            var targetUnit = new CodeCompileUnit();
            var codeNamespace = new CodeNamespace(_fieldSerializerPackage);
            targetUnit.Namespaces.Add(codeNamespace);

            // create the class declaration implementing ITypeSerializer
            var codeTypeDeclaration = new CodeTypeDeclaration
            {
                Name = _fieldSerializerTypeName,
                IsClass = true,
                BaseTypes = { typeof(FieldSerializer) }
            };

            // create the static deserialize method
            var deserializeMethod = new CodeMemberMethod
            {
                Name = "Deserialize",
                Attributes = MemberAttributes.Public | MemberAttributes.Static,
                Parameters =
                {
                    new CodeParameterDeclarationExpression(typeof(ISerializationStreamReader), "reader"),
                    new CodeParameterDeclarationExpression(FullTypeName, "instance")
                }
            };
            codeTypeDeclaration.Members.Add(deserializeMethod);

            // create the static serialize method
            var serializeMethod = new CodeMemberMethod
            {
                Name = "Serialize",
                Attributes = MemberAttributes.Public | MemberAttributes.Static,
                Parameters =
                {
                    new CodeParameterDeclarationExpression(typeof(ISerializationStreamWriter), "writer"),
                    new CodeParameterDeclarationExpression(FullTypeName, "instance")
                }
            };
            codeTypeDeclaration.Members.Add(serializeMethod);

            CodeMemberMethod instantiateMethod = null;
            if (!RpcSerializableType.IsAbstract)
            {
                // create the static instantiate method
                instantiateMethod = new CodeMemberMethod
                {
                    Name = "Instantiate",
                    Attributes = MemberAttributes.Public | MemberAttributes.Static,
                    ReturnType = new CodeTypeReference(FullTypeName),
                    Parameters =
                    {
                        new CodeParameterDeclarationExpression(typeof(ISerializationStreamReader), "reader")
                    }
                };
                codeTypeDeclaration.Members.Add(instantiateMethod);
            }

            // if a custom serializer, just reference it
            if (RpcSerializableType.CustomFieldSerializer != null)
            {
                deserializeMethod.Statements.Add(new CodeMethodInvokeExpression(
                    GetCustomSerializerCodeTypeReferenceExpression(RpcSerializableType.CustomFieldSerializer), "Deserialize",
                    new CodeVariableReferenceExpression("reader"),
                    new CodeVariableReferenceExpression("instance")));

                serializeMethod.Statements.Add(new CodeMethodInvokeExpression(
                    GetCustomSerializerCodeTypeReferenceExpression(RpcSerializableType.CustomFieldSerializer), "Serialize",
                    new CodeVariableReferenceExpression("writer"),
                    new CodeVariableReferenceExpression("instance")));

                if (instantiateMethod != null)
                {
                    instantiateMethod.Statements.Add(new CodeMethodReturnStatement(
                        new CodeMethodInvokeExpression(
                            GetCustomSerializerCodeTypeReferenceExpression(RpcSerializableType.CustomFieldSerializer), "Instantiate",
                            new CodeVariableReferenceExpression("reader"))));
                }
            }
            else
            {
                // delegate serializer work to subclass
                GenerateSerializerBody(deserializeMethod, serializeMethod, instantiateMethod, getSerializableType);
            }

            // add serializer implementation(s)
            codeTypeDeclaration.Members.Add(GenerateFieldSerializerImpl());

            codeNamespace.Types.Add(codeTypeDeclaration);

            // generate the code
            generateCSharpCode.Invoke(targetUnit, _fieldSerializerPackage, _fieldSerializerTypeName);

            // generate children
            GenerateChildFieldSerializers(getSerializableType, generateCSharpCode);
        }

        /// <summary>
        /// Create the field serializer class, with a nested class that actually implements the guts of the serialization.
        /// </summary>
        /// <param name="typePrefix"></param>
        /// <returns></returns>
        private CodeTypeDeclaration GenerateFieldSerializerImpl()
        {
            var typeDeclaration = new CodeTypeDeclaration
            {
                Name = FieldSerializerImplTypeName,
                IsClass = true,
                Attributes = MemberAttributes.Final,
                BaseTypes = { _fieldSerializerTypeName }
            };

            // create the Deserial method which delegates to the Deserialize static
            typeDeclaration.Members.Add(new CodeMemberMethod
            {
                Name = "Deserial",
                Attributes = MemberAttributes.Public | MemberAttributes.Override,
                Parameters =
                {
                    new CodeParameterDeclarationExpression(typeof(ISerializationStreamReader), "reader"),
                    new CodeParameterDeclarationExpression(typeof(object), "instance")
                },
                Statements = { new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(null, "Deserialize"),
                    new CodeVariableReferenceExpression("reader"),
                    new CodeCastExpression(FullTypeName, new CodeVariableReferenceExpression("instance")))}
            });

            // create the Serial method which delegates to the Serialize static
            typeDeclaration.Members.Add(new CodeMemberMethod
            {
                Name = "Serial",
                Attributes = MemberAttributes.Public | MemberAttributes.Override,
                Parameters =
                {
                    new CodeParameterDeclarationExpression(typeof(ISerializationStreamWriter), "writer"),
                    new CodeParameterDeclarationExpression(typeof(object), "instance")
                },
                Statements = { new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(null, "Serialize"),
                    new CodeVariableReferenceExpression("writer"),
                    new CodeCastExpression(FullTypeName, new CodeVariableReferenceExpression("instance")))}
            });

            if (!RpcSerializableType.IsAbstract)
            {
                // create the Create method which delegates to the Instantiate static
                typeDeclaration.Members.Add(new CodeMemberMethod
                {
                    Name = "Create",
                    Attributes = MemberAttributes.Public | MemberAttributes.Override,
                    ReturnType = new CodeTypeReference(typeof(object)),
                    Parameters =
                    {
                        new CodeParameterDeclarationExpression(typeof(ISerializationStreamReader), "reader")
                    },
                    Statements = { new CodeMethodReturnStatement(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(null, "Instantiate"),
                        new CodeVariableReferenceExpression("reader"))) }
                });
            }
            return typeDeclaration;
        }
    }
}
