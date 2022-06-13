/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.CodeDom;
using System.Collections.Generic;
using Deephaven.OpenAPI.SharedGenerator.RPC.WebSockets.APT.Model;

namespace Deephaven.OpenAPI.SharedGenerator.SerializableTypes
{
    /// <summary>
    /// Abstract base class for "synthetic" types, for which we must generate DTO/bean objects as well as serializers.
    /// With nested types, synthetic types form a tree. The nested types are stored in the Children property.
    /// </summary>
    public abstract class SyntheticType : ComplexType
    {
        /// <summary>
        /// Nested types
        /// </summary>
        public List<SyntheticType> Children { get; set; }

        public SyntheticType(RpcSerializableType rpcSerializableType, string fullTypeName, string parentTypeName,
            string fieldSerializerPackage, string fieldSerializerTypeName) :
            base(rpcSerializableType, fullTypeName, parentTypeName, fieldSerializerPackage, fieldSerializerTypeName)
        {
            Children = new List<SyntheticType>();
        }

        protected override void GenerateChildFieldSerializers(Func<string, SerializableType> getSerializableType,
            Action<CodeCompileUnit, string, string> generateCSharpCode)
        {
            foreach (SyntheticType childType in Children)
            {
                childType.GenerateFieldSerializer(getSerializableType, generateCSharpCode);
            }
        }

        /// <summary>
        /// Recursively search this type and it's children for the direct parent of the given type (nested types).
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string FindParent(RpcSerializableType child, out SyntheticType parent)
        {
            // if the child name matches this type we are either the parent or one of our children is
            if (child.Name.StartsWith(RpcSerializableType.Name + "."))
            {
                foreach(var childSyntheticType in Children)
                {
                    var typePrefix = childSyntheticType.FindParent(child, out parent);
                    if(typePrefix != null)
                    {
                        return _typeName + "_" + typePrefix;
                    }
                }
                parent = this;
                return _typeName;
            }
            parent = null;
            return null;
        }

        /// <summary>
        /// Subclasses must implement to generate the type declaration "body" (properties, etc).
        /// </summary>
        /// <param name="codeTypeDeclaration"></param>
        protected abstract void GenerateTypeDeclarationMembers(CodeTypeDeclaration codeTypeDeclaration,
            Func<string, SerializableType> getSerializableType);

        /// <summary>
        /// Return true if this type should be generated as enum (instead of class)
        /// </summary>
        /// <returns></returns>
        protected abstract bool IsEnum();

        /// <summary>
        /// Generate the DTO/"bean" type declaration
        /// </summary>
        /// <param name="getSerializableType"></param>
        /// <param name="generateCSharpCode"></param>
        public void GenerateTypeDeclaration(Func<string, SerializableType> getSerializableType,
            Action<CodeCompileUnit,string,string> generateCSharpCode)
        {
            var beanDecl = GenerateCodeTypeDeclaration(getSerializableType);
            if (beanDecl != null)
            {
                var targetUnit = new CodeCompileUnit();
                var codeNamespace = new CodeNamespace(_package);
                targetUnit.Namespaces.Add(codeNamespace);
                codeNamespace.Types.Add(beanDecl);
                generateCSharpCode.Invoke(targetUnit, _package, _typeName);
            }
        }

        /// <summary>
        /// Generate the DTO/bean object declaration (may call itself recursively for nested types).
        /// </summary>
        /// <returns></returns>
        private CodeTypeDeclaration GenerateCodeTypeDeclaration(Func<string,SerializableType> getSerializableType)
        {
            // skip hand-coded types
            if (SerializableTypeMapper.IsTypeDefined(FullTypeName))
            {
                Console.WriteLine("Skipping DTO codegen for predefined type: " + FullTypeName);
                return null;
            }

            // for now we require all types with a custom serializer also have a hand-coded class/enum for the DTO
            if (RpcSerializableType.CustomFieldSerializer != null)
            {
                throw new Exception(
                    "Serializable type has a custom field serializer but no predefined class/enum: " +
                    RpcSerializableType.Name);
            }

            // create the type decl as a class
            var codeTypeDeclaration = new CodeTypeDeclaration
            {
                Name = _typeName,
                IsEnum = IsEnum()
            };

            // generate any nested types as members of this declaration
            foreach (var nestedType in Children)
            {
                codeTypeDeclaration.Members.Add(nestedType.GenerateCodeTypeDeclaration(getSerializableType));
            }

            // if there is a super type, specify it
            if (RpcSerializableType.SuperTypeId != null)
            {
                var superType = getSerializableType.Invoke(RpcSerializableType.SuperTypeId);
                codeTypeDeclaration.BaseTypes.Add(new CodeTypeReference(superType.FullTypeName));
            }

            // generate properties/fields
            GenerateTypeDeclarationMembers(codeTypeDeclaration, getSerializableType);

            return codeTypeDeclaration;
        }
    }
}
