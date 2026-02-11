/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Deephaven.OpenAPI.Core.Api.Impl;
using Deephaven.OpenAPI.Core.API;
using Deephaven.OpenAPI.Core.API.Impl;
using Deephaven.OpenAPI.Core.RPC.Serialization.API;
using Deephaven.OpenAPI.Core.RPC.Serialization.API.Impl;
using Deephaven.OpenAPI.Core.RPC.Serialization.Stream.Binary;
using Deephaven.OpenAPI.SharedGenerator.RPC.WebSockets.APT.Model;
using Deephaven.OpenAPI.SharedGenerator.SerializableTypes;

namespace Deephaven.OpenAPI.SharedGenerator
{
    /// <summary>
    /// Class for generating server endpoint interface and implementation.
    /// </summary>
    public class ServerEndpoint : Endpoint
    {
        private const string FieldSerializerMapName = "_fieldSerializerMap";
        private const string SerializationSignatureMapName = "_serializationSignatureMap";

        // namespace and type for the type serializer class
        private readonly string _typeSerializerPackage;
        private readonly string _typeSerializerTypeName;

        // namespace and type name for the server impl
        private readonly string _serverImplPackage;
        private readonly string _serverImplTypeName;

        // namespace and type name for the serializer impl
        private readonly string _serializerPackage;
        private readonly string _serializerTypeName;

        private readonly string _serializerHash;
        private readonly RpcEndpoint _rpcClientEndpoint;

        public ServerEndpoint(string serializerHash, Dictionary<string, SerializableType> serializableTypes,
            RpcEndpoint rpcServerEndpoint, int defaultTimeoutMs, string serverEndpointPackage, string serverEndpointInterface,
            RpcEndpoint rpcClientEndpoint) :
            base(serializableTypes, rpcServerEndpoint, defaultTimeoutMs, serverEndpointPackage, serverEndpointInterface)
        {
            _serializerHash = serializerHash;
            _typeSerializerPackage = serverEndpointPackage;
            _typeSerializerTypeName = serverEndpointInterface + "_Impl_TypeSerializer";
            _serverImplPackage = serverEndpointPackage;
            _serverImplTypeName = serverEndpointInterface + "_Impl";
            _serializerPackage = serverEndpointPackage;
            _serializerTypeName = serverEndpointInterface + "_SerializerImpl";
            _rpcClientEndpoint = rpcClientEndpoint;
        }

        /// <summary>
        /// Generate the TypeSerializer implementation for the given endpoint. This amounts to a map of type id to FieldSerializer
        /// implementation for that type.
        /// </summary>
        public string GenerateTypeSerializer(Func<string, SerializableType> getSerializableType,
            Action<CodeCompileUnit, string, string> generateCSharpCode)
        {
            // create the field serializer map field
            var fieldSerializerMapFieldMember = new CodeMemberField
            {
                Type = new CodeTypeReference(typeof(Dictionary<string, FieldSerializer>)),
                Name = FieldSerializerMapName,
                Attributes = MemberAttributes.Static | MemberAttributes.Private,
                InitExpression = new CodeObjectCreateExpression(typeof(Dictionary<string, FieldSerializer>))
            };

            // create the serialization signature map field
            var serializationSignatureMapFieldMember = new CodeMemberField
            {
                Type = new CodeTypeReference(typeof(Dictionary<Type, string>)),
                Name = SerializationSignatureMapName,
                Attributes = MemberAttributes.Static | MemberAttributes.Private,
                InitExpression = new CodeObjectCreateExpression(typeof(Dictionary<Type, string>))
            };

            // create a static constructor to initialize the map
            var staticCtorMember = new CodeTypeConstructor
            {
                Attributes = MemberAttributes.Static | MemberAttributes.Private | MemberAttributes.Final
            };

            // fill the field serializer and serializer signature maps for each complex type
            foreach (var serializableTypeEntry in _serializableTypes)
            {
                var typeId = serializableTypeEntry.Key;
                if (serializableTypeEntry.Value is ComplexType)
                {
                    var complexType = (ComplexType)serializableTypeEntry.Value;
                    staticCtorMember.Statements.Add(new CodeAssignStatement(
                        new CodeVariableReferenceExpression(FieldSerializerMapName + "[\"" + typeId + "\"]"),
                        new CodeObjectCreateExpression(complexType.FieldSerializerPackage
                                                       + "." + complexType.FieldSerializerTypeName
                                                       + "." + complexType.FieldSerializerImplTypeName)));

                    // map C# type to serialization signature
                    // note that we map the underlying primitive type for nullable types (ie double?/Nullable<double>),
                    // because when we serialize, value.GetType() returns the underlying type
                    Type type = SerializableTypeMapper.GetSerializableType(complexType.FullTypeName);
                    string fullTypeName;
                    if (type != null)
                    {
                        var underlyingType = Nullable.GetUnderlyingType(type);
                        if (underlyingType != null)
                        {
                            type = underlyingType;
                        }
                        fullTypeName = type.FullName;
                    }
                    else
                    {
                        fullTypeName = complexType.FullTypeName;
                    }

                    staticCtorMember.Statements.Add(new CodeAssignStatement(
                        new CodeArrayIndexerExpression(
                            new CodeVariableReferenceExpression(SerializationSignatureMapName),
                            new CodeTypeOfExpression(fullTypeName)),
                        new CodePrimitiveExpression(typeId)));
                }
            }

            var instanceCtor = new CodeConstructor
            {
                Attributes = MemberAttributes.Public
            };

            // implement the Serializer method, does a simple lookup in the serializer map and throws an exception if not found
            var serializerMethod = new CodeMemberMethod
            {
                Name = "Serializer",
                Attributes = MemberAttributes.Override | MemberAttributes.Family,
                ReturnType = new CodeTypeReference(typeof(FieldSerializer)),
                Parameters = { new CodeParameterDeclarationExpression(typeof(string), "typeId") },
                Statements =
                {
                    new CodeVariableDeclarationStatement(typeof(FieldSerializer), "serializer"),
                    new CodeConditionStatement(new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression(FieldSerializerMapName), "TryGetValue",
                        new CodeVariableReferenceExpression("typeId"), new CodeVariableReferenceExpression("out serializer")),
                        new CodeStatement[] { new CodeMethodReturnStatement(new CodeVariableReferenceExpression("serializer")) },
                        new CodeStatement[] { new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(ArgumentException),
                            new CodeVariableReferenceExpression("typeId"))) })
                }
            };

            // implement the Serializer method, does a simple lookup in the serializer map and throws an exception if not found
            var getSerializationSignatureMethod = new CodeMemberMethod
            {
                Name = "GetSerializationSignature",
                Attributes = MemberAttributes.Override | MemberAttributes.Public,
                ReturnType = new CodeTypeReference(typeof(string)),
                Parameters = { new CodeParameterDeclarationExpression(typeof(Type), "type") },
                Statements =
                {
                    new CodeVariableDeclarationStatement(typeof(string), "signature"),
                    new CodeConditionStatement(new CodeMethodInvokeExpression(
                            new CodeVariableReferenceExpression(SerializationSignatureMapName), "TryGetValue",
                            new CodeVariableReferenceExpression("type"), new CodeVariableReferenceExpression("out signature")),
                        new CodeStatement[] { new CodeMethodReturnStatement(new CodeVariableReferenceExpression("signature")) },
                        new CodeStatement[] { new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(SerializationException),
                            new CodeSnippetExpression("\"No serialization signature found for type: \" + type.FullName"))) })
                }
            };

            // create the class declaration extending TypeSerializerImpl
            var className = _rpcEndpoint.EndpointInterface + "_Impl_TypeSerializer";
            var codeTypeDeclaration = new CodeTypeDeclaration
            {
                Name = className,
                IsClass = true,
                BaseTypes = { typeof(TypeSerializerImpl) },
                Members = { fieldSerializerMapFieldMember, serializationSignatureMapFieldMember,
                    staticCtorMember, instanceCtor, serializerMethod, getSerializationSignatureMethod }
            };

            var targetUnit = new CodeCompileUnit();
            var codeNamespace = new CodeNamespace(_typeSerializerPackage);

            targetUnit.Namespaces.Add(codeNamespace);

            // add the type
            codeNamespace.Types.Add(codeTypeDeclaration);

            // write the code
            generateCSharpCode(targetUnit, _typeSerializerPackage, _typeSerializerTypeName);

            return className;
        }

        private void GenerateServerEndpointMethodImpl(CodeTypeDeclaration typeDeclaration,
            int index, RpcMethod rpcMethod,
            Func<string, SerializableType> getSerializableType)
        {
            // create a private class representing the message to be send (member for each field)
            var serializerTypeName = _rpcEndpoint.EndpointInterface + "_SerializerImpl";
            CodeTypeReference messageSenderTypeRef;
            if (rpcMethod.Callback != null)
            {
                messageSenderTypeRef = new CodeTypeReference(typeof(AbstractEndpointImpl.MessageSenderWithCallback<,,>));
                messageSenderTypeRef.TypeArguments.Add(serializerTypeName);
                var successType = getSerializableType(rpcMethod.Callback.SuccessTypeId);
                messageSenderTypeRef.TypeArguments.Add(successType.FullTypeName);
                var failureType = getSerializableType(rpcMethod.Callback.FailureTypeId);
                messageSenderTypeRef.TypeArguments.Add(failureType.FullTypeName);
            }
            else
            {
                messageSenderTypeRef = new CodeTypeReference(typeof(AbstractEndpointImpl.MessageSender<>));
                messageSenderTypeRef.TypeArguments.Add(serializerTypeName);
            }

            var messageSenderDecl = new CodeTypeDeclaration
            {
                Name = rpcMethod.Name + "Sender",
                Attributes = MemberAttributes.Private,
                BaseTypes = { messageSenderTypeRef }
            };

            messageSenderDecl.Comments.Add(
                new CodeCommentStatement("We declare a class enclosing the message details explicitly because CodeDom cannot generate lambdas"));
            var messageSenderCtor = new CodeConstructor
            {
                Attributes = MemberAttributes.Public,
                Parameters = { new CodeParameterDeclarationExpression(_rpcEndpoint.EndpointInterface + "_SerializerImpl", "s") },
                BaseConstructorArgs = { new CodeVariableReferenceExpression("s") }
            };
            messageSenderDecl.Members.Add(messageSenderCtor);
            var sendMethod = new CodeMemberMethod
            {
                Name = "Send",
                Attributes = MemberAttributes.Override | MemberAttributes.Public,
                Parameters =
                    {new CodeParameterDeclarationExpression(typeof(ISerializationStreamWriter), "activeWriter")}
            };
            messageSenderDecl.Members.Add(sendMethod);

            if (rpcMethod.Callback != null)
            {
                var successType = getSerializableType(rpcMethod.Callback.SuccessTypeId);
                messageSenderDecl.Members.Add(new CodeMemberMethod
                {
                    Name = "ReadSuccess",
                    Attributes = MemberAttributes.Override | MemberAttributes.Public,
                    ReturnType = new CodeTypeReference(successType.FullTypeName),
                    Parameters = { new CodeParameterDeclarationExpression(typeof(ISerializationStreamReader), "reader") },
                    Statements = { new CodeMethodReturnStatement(new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("_s"),
                        successType.GetReadMethodName(),
                        new CodeVariableReferenceExpression("reader"))) }
                });
                var failureType = getSerializableType(rpcMethod.Callback.FailureTypeId);
                messageSenderDecl.Members.Add(new CodeMemberMethod
                {
                    Name = "ReadFailure",
                    Attributes = MemberAttributes.Override | MemberAttributes.Public,
                    ReturnType = new CodeTypeReference(failureType.FullTypeName),
                    Parameters = { new CodeParameterDeclarationExpression(typeof(ISerializationStreamReader), "reader") },
                    Statements = { new CodeMethodReturnStatement(new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("_s"),
                        failureType.GetReadMethodName(),
                        new CodeVariableReferenceExpression("reader"))) }
                });
            }


            // create an RPC method (this will be blocking if there is a callback)
            var codeMemberMethod = new CodeMemberMethod
            {
                Name = Util.ToUpperCamel(rpcMethod.Name),
                Attributes = MemberAttributes.Public | MemberAttributes.Final
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

            // expression for creating the sender object
            var senderCreateExpr = new CodeObjectCreateExpression(new CodeTypeReference(messageSenderDecl.Name));
            senderCreateExpr.Parameters.Add(new CodeVariableReferenceExpression("_s"));

            // add the message arguments as members and send arguments
            foreach (var p in rpcMethod.Parameters)
            {
                var rpcSerializableType = getSerializableType(p.TypeId);
                var fieldName = "_" + p.Name;
                // create a field
                messageSenderDecl.Members.Add(new CodeMemberField(new CodeTypeReference(rpcSerializableType.FullTypeName), fieldName));
                // pass and assign in the ctor
                messageSenderCtor.Parameters.Add(
                    new CodeParameterDeclarationExpression(rpcSerializableType.FullTypeName, p.Name));
                messageSenderCtor.Statements.Add(new CodeAssignStatement(
                    new CodeVariableReferenceExpression(fieldName),
                    new CodeVariableReferenceExpression(p.Name)));
                // call the writer
                sendMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("_s"),
                    rpcSerializableType.GetWriteMethodName(),
                    new CodeVariableReferenceExpression(fieldName),
                    new CodeVariableReferenceExpression("activeWriter")));
                // pass param to sender create
                senderCreateExpr.Parameters.Add(new CodeVariableReferenceExpression(p.Name));
            }

            // statement creating the sender object
            codeMemberMethod.Statements.Add(
                new CodeVariableDeclarationStatement(new CodeTypeReference(messageSenderDecl.Name), "sender",
                    senderCreateExpr));

            // if there is a callback, define it as an Action with two generic type args matching the success and failure return types
            if (rpcMethod.Callback != null)
            {
                var successReturnType =
                    getSerializableType(rpcMethod.Callback.SuccessTypeId);
                var failureReturnType =
                    getSerializableType(rpcMethod.Callback.FailureTypeId);
                var asyncCodeMemberMethod = new CodeMemberMethod
                {
                    Name = codeMemberMethod.Name + "Async",
                    Attributes = MemberAttributes.Public | MemberAttributes.Final
                };

                // set the return type for the blocking method
                codeMemberMethod.ReturnType = new CodeTypeReference(successReturnType.FullTypeName);

                // async method takes same parameters plus the two callback lambdas
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

                // blocking method takes a timeout
                codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "timeoutMs"));

                // create the sender object
                asyncCodeMemberMethod.Statements.Add(
                    new CodeVariableDeclarationStatement(new CodeTypeReference(messageSenderDecl.Name), "sender",
                        senderCreateExpr));

                // create the blocking callback object in the blocking method
                var blockingCallbackTypeRef = new CodeTypeReference(typeof(AbstractEndpointImpl.BlockingCallback<,>));
                blockingCallbackTypeRef.TypeArguments.Add(new CodeTypeReference(successReturnType.FullTypeName));
                blockingCallbackTypeRef.TypeArguments.Add(new CodeTypeReference(failureReturnType.FullTypeName));
                codeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("var"),
                    "callback", new CodeObjectCreateExpression(blockingCallbackTypeRef,
                        new CodeMethodReferenceExpression(new CodeVariableReferenceExpression("sender"), "ReadSuccess"),
                        new CodeMethodReferenceExpression(new CodeVariableReferenceExpression("sender"), "ReadFailure"),
                        new CodeVariableReferenceExpression("timeoutMs"))));

                // create the async callback object for the async method
                var readingCallbackTypeRef = new CodeTypeReference(typeof(AbstractEndpointImpl.AsyncCallback<,>));
                readingCallbackTypeRef.TypeArguments.Add(new CodeTypeReference(successReturnType.FullTypeName));
                readingCallbackTypeRef.TypeArguments.Add(new CodeTypeReference(failureReturnType.FullTypeName));
                asyncCodeMemberMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("var"),
                    "callback", new CodeObjectCreateExpression(readingCallbackTypeRef,
                        new CodeMethodReferenceExpression(new CodeVariableReferenceExpression("sender"), "ReadSuccess"),
                        new CodeMethodReferenceExpression(new CodeVariableReferenceExpression("sender"), "ReadFailure"),
                        new CodeVariableReferenceExpression("successCallback"),
                        new CodeVariableReferenceExpression("failureCallback"),
                        new CodeVariableReferenceExpression("errorCallback"))));

                // call __Send in the async method
                var asyncSendMethodInvoke = new CodeMethodInvokeExpression(null, "__Send",
                    new CodeSnippetExpression(index.ToString()),
                    new CodeMethodReferenceExpression(new CodeVariableReferenceExpression("sender"), "Send")
                );
                asyncSendMethodInvoke.Parameters.Add(new CodeVariableReferenceExpression("callback"));
                asyncCodeMemberMethod.Statements.Add(asyncSendMethodInvoke);
                typeDeclaration.Members.Add(asyncCodeMemberMethod);

                // call __Send in the blocking/default method
                var sendMethodInvoke = new CodeMethodInvokeExpression(null, "__Send",
                    new CodeSnippetExpression(index.ToString()),
                    new CodeMethodReferenceExpression(new CodeVariableReferenceExpression("sender"), "Send")
                );
                sendMethodInvoke.Parameters.Add(new CodeVariableReferenceExpression("callback"));
                codeMemberMethod.Statements.Add(sendMethodInvoke);
            }
            else
            {
                // call __Send with no callback (no return value)
                var sendMethodInvoke = new CodeMethodInvokeExpression(null, "__Send",
                    new CodeSnippetExpression(index.ToString()),
                    new CodeMethodReferenceExpression(new CodeVariableReferenceExpression("sender"), "Send")
                );
                codeMemberMethod.Statements.Add(sendMethodInvoke);
            }

            // if there is a return value, block on it and return the value
            // ie "return callback.Wait();"
            if (rpcMethod.Callback != null)
            {
                codeMemberMethod.Statements.Add(new CodeMethodReturnStatement(
                    new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("callback"), "Wait")));
            }

            // add the message sender and
            typeDeclaration.Members.Add(codeMemberMethod);

            // add the message sender nested class declaration
            typeDeclaration.Members.Add(messageSenderDecl);
        }

        public string GenerateServerEndpointImpl(Func<string, SerializableType> getSerializableType,
            Action<CodeCompileUnit, string, string> generateCSharpCode)
        {
            var baseClassRef = new CodeTypeReference("AbstractWebSocketServerImpl");
            baseClassRef.TypeArguments.Add(new CodeTypeReference(_rpcEndpoint.EndpointInterface));
            baseClassRef.TypeArguments.Add(new CodeTypeReference(_rpcClientEndpoint.EndpointInterface));

            // create the class declaration extending TypeSerializerImpl
            var className = _rpcEndpoint.EndpointInterface + "_Impl";
            var typeDeclaration = new CodeTypeDeclaration
            {
                Name = className,
                IsClass = true,
                BaseTypes = { baseClassRef, _rpcEndpoint.EndpointInterface, new CodeTypeReference(typeof(IHasChecksum)) }
            };

            // serializer member field (initialized in constructor)
            var serializerField = new CodeMemberField(new CodeTypeReference(_rpcEndpoint.EndpointInterface + "_SerializerImpl"), "_s");
            typeDeclaration.Members.Add(serializerField);

            // ctor parameter declarations
            var writerFactoryDecl = new CodeParameterDeclarationExpression(
                new CodeTypeReference(typeof(Func<ITypeSerializer, BinarySerializationStreamWriter>)),
                "writerFactory");
            var sendDecl = new CodeParameterDeclarationExpression(
                new CodeTypeReference(typeof(Action<BinarySerializationStreamWriter>)), "send");
            var serializerDecl = new CodeParameterDeclarationExpression(new CodeTypeReference(_rpcEndpoint.EndpointInterface + "_SerializerImpl"), "serializers");
            var onMessageDecl = new CodeParameterDeclarationExpression(
                new CodeTypeReference(typeof(Action<Action<ISerializationStreamReader>, ITypeSerializer>)),
                    "onMessage");
            var onCloseDecl = new CodeParameterDeclarationExpression(
                new CodeTypeReference(typeof(Action)), "onClose");

            // create the constructors
            typeDeclaration.Members.Add(new CodeConstructor
            {
                Attributes = MemberAttributes.Public,
                Parameters =
                {
                    writerFactoryDecl,
                    sendDecl,
                    serializerDecl,
                    onMessageDecl,
                    onCloseDecl
                },
                BaseConstructorArgs =
                {
                    new CodeVariableReferenceExpression("writerFactory"),
                    new CodeVariableReferenceExpression("send"),
                    new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("serializers"),
                        "CreateSerializer"),
                    new CodeVariableReferenceExpression("onMessage"),
                    new CodeVariableReferenceExpression("onClose"),
                },
                Statements =
                {
                    new CodeAssignStatement(new CodeVariableReferenceExpression("_s"),
                        new CodeVariableReferenceExpression("serializers"))
                }
            });

            typeDeclaration.Members.Add(new CodeConstructor
            {
                Attributes = MemberAttributes.Public,
                Parameters =
                {
                    writerFactoryDecl,
                    sendDecl,
                    onMessageDecl,
                    onCloseDecl
                },
                ChainedConstructorArgs =
                {
                    new CodeVariableReferenceExpression("writerFactory"),
                    new CodeVariableReferenceExpression("send"),
                    new CodeObjectCreateExpression(new CodeTypeReference(_rpcEndpoint.EndpointInterface + "_SerializerImpl")),
                    new CodeVariableReferenceExpression("onMessage"),
                    new CodeVariableReferenceExpression("onClose"),
                }
            });

            // generate checksum property
            string checksum = _rpcEndpoint.EndpointHash + _serializerHash;
            typeDeclaration.Members.Add(new CodeMemberProperty
            {
                Name = "Checksum",
                Attributes = MemberAttributes.Public,
                Type = new CodeTypeReference(typeof(string)),
                HasGet = true,
                HasSet = false,
                GetStatements =
                {
                    new CodeMethodReturnStatement(new CodePrimitiveExpression(checksum))
                }
            });

            var idx = 0;
            foreach (var rpcMethod in _rpcEndpoint.Methods)
            {
                GenerateServerEndpointMethodImpl(typeDeclaration, idx++, rpcMethod, getSerializableType);
            }

            // implement __Invoke
            // we use some ugly stuff here to generate the switch statement, as CodeDom has no abstraction for creating these
            var invokeMethod = new CodeMemberMethod
            {
                Name = "__Invoke",
                Attributes = MemberAttributes.Override | MemberAttributes.Family,
                Parameters =
                {
                    new CodeParameterDeclarationExpression(typeof(int), "recipient"),
                    new CodeParameterDeclarationExpression(typeof(ISerializationStreamReader), "reader")
                }
            };
            invokeMethod.Statements.Add(new CodeSnippetStatement("            switch(recipient)"));
            invokeMethod.Statements.Add(new CodeSnippetStatement("            {"));
            for (var methodIndex = 0; methodIndex < _rpcClientEndpoint.Methods.Length; methodIndex++)
            {
                var rpcClientMethod = _rpcClientEndpoint.Methods[methodIndex];
                invokeMethod.Statements.Add(new CodeSnippetStatement("              case " + methodIndex + ":"));
                invokeMethod.Statements.Add(new CodeSnippetStatement("              {"));

                // build the client method invoke statement
                var paramReaders = new CodeExpression[rpcClientMethod.Parameters.Length];
                for (var pIdx = 0; pIdx < rpcClientMethod.Parameters.Length; pIdx++)
                {
                    var pType = getSerializableType(rpcClientMethod.Parameters[pIdx].TypeId);
                    paramReaders[pIdx] = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("_s"),
                        pType.GetReadMethodName(), new CodeVariableReferenceExpression("reader"));
                }
                var getClientInvoke = new CodeMethodInvokeExpression(null, "GetClient");
                var methodInvoke = new CodeMethodInvokeExpression(getClientInvoke, Util.ToUpperCamel(rpcClientMethod.Name), paramReaders);

                invokeMethod.Statements.Add(methodInvoke);

                // close this case
                invokeMethod.Statements.Add(new CodeSnippetStatement("                  break;"));
                invokeMethod.Statements.Add(new CodeSnippetStatement("              }"));
                idx++;
            }
            invokeMethod.Statements.Add(new CodeSnippetExpression("         }"));
            typeDeclaration.Members.Add(invokeMethod);

            // implement __OnError (does nothing atm)
            var onErrorMethod = new CodeMemberMethod
            {
                Name = "__OnError",
                Attributes = MemberAttributes.Override | MemberAttributes.Family,
                Parameters =
                {
                    new CodeParameterDeclarationExpression(typeof(Exception), "ex")
                }
            };
            onErrorMethod.Statements.Add(new CodeMethodInvokeExpression(
                new CodeMethodInvokeExpression(null, "GetClient"), "OnError",
                new CodeVariableReferenceExpression("ex")));
            typeDeclaration.Members.Add(onErrorMethod);

            // add the serializer interface as a nested type
            typeDeclaration.Members.Add(GenerateEndpointSerializerInterface(getSerializableType, generateCSharpCode));

            var targetUnit = new CodeCompileUnit();

            var codeNamespace = new CodeNamespace(_serverImplPackage);
            codeNamespace.Types.Add(typeDeclaration);
            codeNamespace.Imports.Add(new CodeNamespaceImport(typeof(AbstractWebSocketServerImpl<,>).Namespace));
            targetUnit.Namespaces.Add(codeNamespace);

            generateCSharpCode(targetUnit, _serverImplPackage, _serverImplTypeName);

            return className;
        }

        private HashSet<string> EnumerableToHashSet(IEnumerable<string> e)
        {
            var set = new HashSet<string>();
            foreach(var s in e)
            {
                set.Add(s);
            }
            return set;
        }

        private CodeTypeDeclaration GenerateEndpointSerializerInterface(Func<string, SerializableType> getSerializableType,
            Action<CodeCompileUnit, string, string> generateCSharpCode)
        {
            // create the class declaration extending TypeSerializerImpl
            var codeTypeDeclaration = new CodeTypeDeclaration
            {
                Name = "ISerializer",
                IsInterface = true
            };

            // the CreateSerializer factory method
            codeTypeDeclaration.Members.Add(new CodeMemberMethod
            {
                Name = "CreateSerializer",
                Attributes = MemberAttributes.Public,
                ReturnType = new CodeTypeReference(typeof(ITypeSerializer))
            });

            // for each method parameter type, generate a typed writer method
            var writableTypeIds = EnumerableToHashSet(
                _rpcEndpoint.Methods.SelectMany(s => s.Parameters).Select(p => p.TypeId));
            foreach (var writableTypeId in writableTypeIds)
            {
                var rpcSerializableType = getSerializableType(writableTypeId);
                codeTypeDeclaration.Members.Add(new CodeMemberMethod
                {
                    Name = rpcSerializableType.GetWriteMethodName(),
                    Attributes = MemberAttributes.Public,
                    Parameters =
                    {
                        new CodeParameterDeclarationExpression(new CodeTypeReference(rpcSerializableType.FullTypeName),
                            "instance"),
                        new CodeParameterDeclarationExpression(
                            new CodeTypeReference(typeof(ISerializationStreamWriter)), "writer")
                    }
                });
            }

            // for each return type, generate a typed reader method
            var readFailureTypeIds = EnumerableToHashSet(_rpcEndpoint.Methods.Where(m => m.Callback != null)
                .Select(m => m.Callback.FailureTypeId));
            var readSuccessTypeIds = EnumerableToHashSet(_rpcEndpoint.Methods.Where(m => m.Callback != null)
                .Select(m => m.Callback.SuccessTypeId));
            var readTypeIds = readFailureTypeIds.Union(readSuccessTypeIds);
            foreach (var readableTypeId in readTypeIds)
            {
                var rpcSerializableType = getSerializableType(readableTypeId);
                codeTypeDeclaration.Members.Add(new CodeMemberMethod
                {
                    Name = rpcSerializableType.GetReadMethodName(),
                    Attributes = MemberAttributes.Public,
                    ReturnType = new CodeTypeReference(rpcSerializableType.FullTypeName),
                    Parameters =
                    {
                        new CodeParameterDeclarationExpression(
                            new CodeTypeReference(typeof(ISerializationStreamReader)), "reader")
                    }
                });
            }

            return codeTypeDeclaration;
        }

        public void GenerateEndpointSerializer(Func<string, SerializableType> getSerializableType,
            Action<CodeCompileUnit, string, string> generateCSharpCode)
        {
            // create the class declaration extending TypeSerializerImpl
            var codeTypeDeclaration = new CodeTypeDeclaration
            {
                Name = _serializerTypeName,
                IsClass = true,
                BaseTypes = { new CodeTypeReference(_serverImplTypeName + ".ISerializer") }
            };

            // implement the CreateSerializer factory method
            codeTypeDeclaration.Members.Add(new CodeMemberMethod
            {
                Name = "CreateSerializer",
                Attributes = MemberAttributes.Public,
                ReturnType = new CodeTypeReference(typeof(ITypeSerializer)),
                Statements =
                {
                    new CodeMethodReturnStatement(new CodeObjectCreateExpression(new CodeTypeReference(_typeSerializerTypeName)))
                }
            });

            // for each server method parameter type, generate a typed writer method
            var serverMethodParameterTypeIds = EnumerableToHashSet(_rpcEndpoint.Methods.SelectMany(s => s.Parameters)
                .Select(p => p.TypeId));
            var clientReadFailureTypeIds = EnumerableToHashSet(_rpcEndpoint.Methods.Where(m => m.Callback != null)
                .Select(m => m.Callback.FailureTypeId));
            var clientReadSuccessTypeIds = EnumerableToHashSet(_rpcEndpoint.Methods.Where(m => m.Callback != null)
                .Select(m => m.Callback.SuccessTypeId));
            var writeTypeIds = serverMethodParameterTypeIds.Union(clientReadFailureTypeIds)
                .Union(clientReadSuccessTypeIds);
            foreach (var writableTypeId in writeTypeIds)
            {
                var rpcSerializableType = getSerializableType(writableTypeId);
                codeTypeDeclaration.Members.Add(new CodeMemberMethod
                {
                    Name = rpcSerializableType.GetWriteMethodName(),
                    Attributes = MemberAttributes.Public,
                    Parameters =
                    {
                        new CodeParameterDeclarationExpression(new CodeTypeReference(
                            rpcSerializableType.FullTypeName),
                            "instance"),
                        new CodeParameterDeclarationExpression(
                            new CodeTypeReference(typeof(ISerializationStreamWriter)), "writer")
                    },
                    Statements =
                    {
                        new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("writer"),
                            rpcSerializableType.GetStreamWriterMethodName(), new CodeVariableReferenceExpression("instance"))
                    }
                });
            }

            // for each client method parameter and server return type, generate a typed reader method
            var readFailureTypeIds = EnumerableToHashSet(_rpcEndpoint.Methods.Where(m => m.Callback != null)
                .Select(m => m.Callback.FailureTypeId));
            var readSuccessTypeIds = EnumerableToHashSet(_rpcEndpoint.Methods.Where(m => m.Callback != null)
                .Select(m => m.Callback.SuccessTypeId));
            var clientMethodParameterTypeIds = EnumerableToHashSet(_rpcClientEndpoint.Methods.SelectMany(s => s.Parameters)
                .Select(p => p.TypeId));
            var readTypeIds = readFailureTypeIds.Union(readSuccessTypeIds).Union(clientMethodParameterTypeIds);
            foreach (var readableTypeId in readTypeIds)
            {
                var rpcSerializableType = getSerializableType(readableTypeId);
                codeTypeDeclaration.Members.Add(new CodeMemberMethod
                {
                    Name = rpcSerializableType.GetReadMethodName(),
                    Attributes = MemberAttributes.Public,
                    ReturnType = new CodeTypeReference(rpcSerializableType.FullTypeName),
                    Parameters =
                    {
                        new CodeParameterDeclarationExpression(
                            new CodeTypeReference(typeof(ISerializationStreamReader)), "reader")
                    },
                    Statements =
                    {
                        new CodeMethodReturnStatement(new CodeCastExpression(new CodeTypeReference(
                            rpcSerializableType.FullTypeName),
                            new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("reader"),
                            rpcSerializableType.GetStreamReaderMethodName())))
                    }
                });
            }

            var targetUnit = new CodeCompileUnit();
            var codeNamespace = new CodeNamespace(_serializerPackage);

            targetUnit.Namespaces.Add(codeNamespace);

            // add the type
            codeNamespace.Types.Add(codeTypeDeclaration);

            // write the code
            generateCSharpCode(targetUnit, _serializerPackage, _serializerTypeName);
        }
    }
}
