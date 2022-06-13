/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.API.Impl
{
    /// <summary>
    /// Trivial implementation, where type signatures are provided by a subclass that implements <see cref="GetSerializationSignature(Type)"/>.
    /// </summary>
    public abstract class TypeSerializerImpl : ITypeSerializer
    {
        protected abstract FieldSerializer Serializer(string name);

        public void Deserialize(ISerializationStreamReader stream, object instance, string typeSignature) {
            Serializer(typeSignature).Deserial(stream, instance);
        }

        public abstract string GetSerializationSignature(Type type);

        public object Instantiate(ISerializationStreamReader stream, string typeSignature) {
            return Serializer(typeSignature).Create(stream);
        }

        public void Serialize(ISerializationStreamWriter stream, object instance, string typeSignature) {
            Serializer(typeSignature).Serial(stream, instance);
        }
    }
}
