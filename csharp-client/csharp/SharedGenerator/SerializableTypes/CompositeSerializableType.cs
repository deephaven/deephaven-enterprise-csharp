/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.CodeDom;
using Deephaven.OpenAPI.SharedGenerator.RPC.WebSockets.APT.Model;

namespace Deephaven.OpenAPI.SharedGenerator.SerializableTypes
{
    public class CompositeSerializableType : SyntheticType
    {
        public CompositeSerializableType(RpcSerializableType rpcSerializableType, string fullTypeName, string parentTypeName,
            string fieldSerializerPackage, string fieldSerializerTypeName) :
            base(rpcSerializableType, fullTypeName, parentTypeName, fieldSerializerPackage, fieldSerializerTypeName)
        {
        }

        protected override bool IsEnum()
        {
            return false;
        }

        /// <summary>
        /// Generate a field and property for each composite type field.
        /// </summary>
        /// <param name="codeTypeDeclaration"></param>
        /// <param name="getSerializableType"></param>
        protected override void GenerateTypeDeclarationMembers(CodeTypeDeclaration codeTypeDeclaration,
            Func<string, SerializableType> getSerializableType)
        {
            // add properties as fields with property accessors
            if (RpcSerializableType.Properties != null)
            {
                foreach (var rpcTypeField in RpcSerializableType.Properties)
                {
                    var fieldType = getSerializableType(rpcTypeField.TypeId);
                    if (fieldType == null)
                    {
                        throw new Exception("Unknown type name: " + rpcTypeField.TypeId);
                    }

                    // add field
                    var codeMemberField = new CodeMemberField
                    {
                        Name = "_" + rpcTypeField.Name,
                        Type = new CodeTypeReference(fieldType.FullTypeName),
                        Attributes = MemberAttributes.Private
                    };
                    codeTypeDeclaration.Members.Add(codeMemberField);

                    // add public property for the field
                    var codeMemberProperty = new CodeMemberProperty
                    {
                        Name = Util.ToUpperCamel(rpcTypeField.Name),
                        Type = new CodeTypeReference(fieldType.FullTypeName),
                        Attributes = MemberAttributes.Public | MemberAttributes.Final,
                        HasGet = true,
                        HasSet = true,
                        GetStatements =
                        {
                            new CodeMethodReturnStatement(
                                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
                                    codeMemberField.Name))
                        },
                        SetStatements =
                        {
                            new CodeAssignStatement(
                                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
                                    codeMemberField.Name),
                                new CodePropertySetValueReferenceExpression())
                        }
                    };
                    codeTypeDeclaration.Members.Add(codeMemberProperty);
                }
            }
        }

        /// <summary>
        /// Implement composite serializer by reading/writing each field in order. The instantiate method is optional (abstract types can't be instantiated).
        /// </summary>
        /// <param name="deserializeMethod"></param>
        /// <param name="serializeMethod"></param>
        /// <param name="instantiateMethod"></param>
        /// <param name="getSerializableType"></param>
        protected override void GenerateSerializerBody(CodeMemberMethod deserializeMethod,
            CodeMemberMethod serializeMethod, CodeMemberMethod instantiateMethod,
            Func<string, SerializableType> getSerializableType)
        {
            if (RpcSerializableType.Properties != null)
            {
                foreach (var rpcTypeField in RpcSerializableType.Properties)
                {
                    var fieldType = getSerializableType(rpcTypeField.TypeId);
                    if (fieldType == null)
                    {
                        throw new Exception("Unknown type name: " + rpcTypeField.TypeId);
                    }

                    var propertyName = Util.ToUpperCamel(rpcTypeField.Name);

                    // add statement to read field from stream (e.g. "instance.MyProperty = reader.GetInt()")
                    deserializeMethod.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(
                            new CodeVariableReferenceExpression("instance"), propertyName),
                        new CodeCastExpression(fieldType.FullTypeName, new CodeMethodInvokeExpression(
                            new CodeVariableReferenceExpression("reader"),
                            fieldType.GetStreamReaderMethodName()))));

                    // add a statement to write the field to a stream (e.g. "writer.Write(instance.MyProperty)")
                    serializeMethod.Statements.Add(new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("writer"), fieldType.GetStreamWriterMethodName(),
                        new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("instance"),
                            propertyName)));
                }
            }

            if (instantiateMethod != null)
            {
                // the instantiate method impl just calls the no-arg constructor (e.g. "return new MyBean()")
                instantiateMethod.Statements.Add(new CodeMethodReturnStatement(
                    new CodeObjectCreateExpression(FullTypeName)));
            }
        }
    }
}
