/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.CodeDom;
using Deephaven.OpenAPI.SharedGenerator.RPC.WebSockets.APT.Model;

namespace Deephaven.OpenAPI.SharedGenerator.SerializableTypes
{
    public class EnumSerializableType : SyntheticType
    {
        public EnumSerializableType(RpcSerializableType rpcSerializableType, string fullTypeName, string parentTypeName,
            string fieldSerializerPackage, string fieldSerializerTypeName) :
            base(rpcSerializableType, fullTypeName, parentTypeName, fieldSerializerPackage, fieldSerializerTypeName)
        {
        }

        protected override bool IsEnum()
        {
            return true;
        }

        protected override void GenerateTypeDeclarationMembers(CodeTypeDeclaration codeTypeDeclaration,
            Func<string, SerializableType> getSerializableType)
        {
            // for enums add each enum value
            if (RpcSerializableType.EnumValues != null)
            {
                foreach (var rpcTypeEnumValue in RpcSerializableType.EnumValues)
                {
                    codeTypeDeclaration.Members.Add(new CodeMemberField(this._typeName, rpcTypeEnumValue));
                }
            }
        }

        protected override void GenerateSerializerBody(CodeMemberMethod deserializeMethod,
            CodeMemberMethod serializeMethod, CodeMemberMethod instantiateMethod,
            Func<string, SerializableType> getSerializableType)
        {
            // Note we do not implement deserialize!
            // write the value as an int (in C# we can just cast an enum value to an int to get the ordinal/assigned value)
            serializeMethod.Statements.Add(new CodeMethodInvokeExpression(
                    new CodeVariableReferenceExpression("writer"), "Write",
                    new CodeCastExpression(typeof(int), new CodeVariableReferenceExpression("instance"))));

            // read an int32 off the stream and cast to the enum type
            if (instantiateMethod != null)
            {
                instantiateMethod.Statements.Add(new CodeMethodReturnStatement(new CodeCastExpression(
                    new CodeTypeReference(FullTypeName),
                    new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("reader"), "ReadInt32"))));
            }
        }
    }
}
