/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using Deephaven.OpenAPI.SharedGenerator.RPC.WebSockets.APT.Model;

namespace Deephaven.OpenAPI.SharedGenerator.SerializableTypes
{
    public class ArraySerializableType : ComplexType
    {
        public ArraySerializableType(RpcSerializableType rpcSerializableType, string fullTypeName,
            string fieldSerializerPackage, string fieldSerializerTypeName) :
            base(rpcSerializableType, fullTypeName, null, fieldSerializerPackage, fieldSerializerTypeName)
        {
        }

        protected override void GenerateChildFieldSerializers(Func<string, SerializableType> getSerializableType,
            Action<CodeCompileUnit, string, string> generateCSharpCode)
        {
            // no-op, arrays cannot have children
        }

        /// <summary>
        /// Generate boilerplate code to read and write arrays to a stream.
        /// </summary>
        /// <param name="deserializeMethod"></param>
        /// <param name="serializeMethod"></param>
        /// <param name="instantiateMethod"></param>
        protected override void GenerateSerializerBody(CodeMemberMethod deserializeMethod,
            CodeMemberMethod serializeMethod, CodeMemberMethod instantiateMethod,
            Func<string, SerializableType> getSerializableType)
        {
            var componentType = getSerializableType(RpcSerializableType.ComponentTypeId);
            if (componentType == null)
            {
                throw new Exception("Unknown type id: " + RpcSerializableType.ComponentTypeId);
            }

            // could probably do this without snippets...
            var initStatement = new CodeSnippetStatement("int i = 0");
            var testExpression = new CodeSnippetExpression("i < instance.Length");
            var incrementStatement = new CodeSnippetStatement("++i");

            // implement Deserialize
            var readStatement = new CodeAssignStatement(new CodeVariableReferenceExpression("instance[i]"),
                new CodeCastExpression(componentType.FullTypeName, new CodeMethodInvokeExpression(
                    new CodeVariableReferenceExpression("reader"),
                    componentType.GetStreamReaderMethodName())));
            deserializeMethod.Statements.Add(new CodeIterationStatement(initStatement, testExpression,
                incrementStatement, readStatement));

            // implement Serialize
            var writeStatement = new CodeExpressionStatement(new CodeMethodInvokeExpression(
                new CodeVariableReferenceExpression("writer"), componentType.GetStreamWriterMethodName(),
                new CodeVariableReferenceExpression("instance[i]")));
            serializeMethod.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(
                new CodeVariableReferenceExpression("writer"), "Write",
                new CodeVariableReferenceExpression("instance.Length"))));
            serializeMethod.Statements.Add(new CodeIterationStatement(initStatement, testExpression,
                incrementStatement, writeStatement));

            if (instantiateMethod != null)
            {
                // implement Instantiate
                instantiateMethod.Statements.Add(new CodeAssignStatement(
                    new CodeSnippetExpression("int length"),
                    new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("reader"), "ReadInt32")));
                instantiateMethod.Statements.Add(new CodeMethodInvokeExpression(
                    new CodeVariableReferenceExpression("reader"),
                    "ClaimItems", new CodeVariableReferenceExpression("length")));

                // if it's a nested array type, there is no way to easily generate the "new Type[length][]" sugar so we use
                // Array.CreateInstance instead
                // otherwise we can use CodeArrayCreateExpression
                if (componentType is ArraySerializableType)
                {
                    instantiateMethod.Statements.Add(new CodeMethodReturnStatement(
                        new CodeCastExpression(FullTypeName,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(Array)),
                                "CreateInstance",
                                new CodeVariableReferenceExpression("typeof(" + componentType.FullTypeName + ")"),
                                new CodeVariableReferenceExpression("length")))));
                }
                else
                {
                    instantiateMethod.Statements.Add(new CodeMethodReturnStatement(new CodeArrayCreateExpression(
                        new CodeTypeReference(componentType.FullTypeName),
                        new CodeVariableReferenceExpression("length"))));
                }
            }
        }
    }
}
