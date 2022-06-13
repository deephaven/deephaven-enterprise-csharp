/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.CodeDom;
using System.Collections.Generic;
using Deephaven.OpenAPI.SharedGenerator.RPC.WebSockets.APT.Model;
using Deephaven.OpenAPI.SharedGenerator.SerializableTypes;

namespace Deephaven.OpenAPI.SharedGenerator
{
    /// <summary>
    /// Class for generating Endpoint interface code.
    /// </summary>
    public class Endpoint
    {
        protected readonly Dictionary<string, SerializableType> _serializableTypes;

        protected RpcEndpoint _rpcEndpoint;
        private int _defaultTimeoutMs;

        public readonly string EndpointPackage;
        public readonly string EndpointInterface;

        public Endpoint(Dictionary<string, SerializableType> serializableTypes,
            RpcEndpoint rpcEndpoint, int defaultTimeoutMs, string endpointPackage, string endpointInterface)
        {
            _serializableTypes = serializableTypes;
            _rpcEndpoint = rpcEndpoint;
            _defaultTimeoutMs = defaultTimeoutMs;
            EndpointPackage = endpointPackage;
            EndpointInterface = endpointInterface;
        }

        public void GenerateEndpointInterface(CodeTypeReference baseInterface,
            Func<string, SerializableType> getSerializableType,
            Action<CodeCompileUnit, string, string> generateCSharpCode)
        {
            // generate the endpoint interface
            var targetUnit = new CodeCompileUnit();
            var codeNamespace = new CodeNamespace(EndpointPackage);
            targetUnit.Namespaces.Add(codeNamespace);

            var endpointTypeDeclaration = new CodeTypeDeclaration
            {
                Name = _rpcEndpoint.EndpointInterface,
                IsInterface = true,
                BaseTypes = { baseInterface }
            };

            foreach (var rpcMethod in _rpcEndpoint.Methods)
            {
                // create an RPC method
                var codeMemberMethod = new CodeMemberMethod
                {
                    Name = Util.ToUpperCamel(rpcMethod.Name)
                };

                // specify the input parameters for the method
                foreach (var rpcMethodParameter in rpcMethod.Parameters)
                {
                    var parameterType = getSerializableType(rpcMethodParameter.TypeId);
                    if (parameterType == null)
                    {
                        throw new Exception("Unknown type name: " + rpcMethodParameter.TypeId);
                    }

                    var codeTypeReference = new CodeTypeReference(parameterType.FullTypeName);
                    codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(
                        codeTypeReference, rpcMethodParameter.Name));
                }

                // if there is a callback, define an Async method with two additional parameters - the success and failure callback lambdas
                if (rpcMethod.Callback != null)
                {
                    var successReturnType = getSerializableType(rpcMethod.Callback.SuccessTypeId);
                    var failureReturnType = getSerializableType(rpcMethod.Callback.FailureTypeId);

                    // set the return type for the blocking method
                    codeMemberMethod.ReturnType = new CodeTypeReference(successReturnType.FullTypeName);

                    var asyncCodeMemberMethod = new CodeMemberMethod
                    {
                        Name = Util.ToUpperCamel(rpcMethod.Name + "Async")
                    };
                    foreach (CodeParameterDeclarationExpression parameter in codeMemberMethod.Parameters)
                    {
                        asyncCodeMemberMethod.Parameters.Add(parameter);
                    }
                    var successCallbackType = new CodeTypeReference(typeof(Action<>));
                    var failureCallbackType = new CodeTypeReference(typeof(Action<>));
                    var errorCallbackType = new CodeTypeReference(typeof(Action<>));
                    successCallbackType.TypeArguments.Add(successReturnType.FullTypeName);
                    failureCallbackType.TypeArguments.Add(failureReturnType.FullTypeName);
                    errorCallbackType.TypeArguments.Add(typeof(string));
                    asyncCodeMemberMethod.Parameters.Add(
                        new CodeParameterDeclarationExpression(successCallbackType, "successCallback"));
                    asyncCodeMemberMethod.Parameters.Add(
                        new CodeParameterDeclarationExpression(failureCallbackType, "failureCallback"));
                    asyncCodeMemberMethod.Parameters.Add(
                        new CodeParameterDeclarationExpression(errorCallbackType, "errorCallback"));
                    endpointTypeDeclaration.Members.Add(asyncCodeMemberMethod);

                    codeMemberMethod.Parameters.Add(
                            new CodeParameterDeclarationExpression(typeof(int), "timeoutMs = " + _defaultTimeoutMs));
                }

                endpointTypeDeclaration.Members.Add(codeMemberMethod);
            }
            codeNamespace.Types.Add(endpointTypeDeclaration);

            generateCSharpCode(targetUnit, EndpointPackage, EndpointInterface);
        }
    }
}
