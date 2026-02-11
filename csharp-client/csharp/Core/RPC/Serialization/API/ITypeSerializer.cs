/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.API
{
    public interface ITypeSerializer
    {
        /// <summary>
        /// Restore an instantiated object from the serialized stream.
        /// </summary>
        /// <param name="stream">The stream from which to read data for the given object.</param>
        /// <param name="instance"></param>
        /// <param name="typeSignature"></param>
        void Deserialize(ISerializationStreamReader stream, object instance, string typeSignature);

        /// <summary>
        /// Return the serialization signature for the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string GetSerializationSignature(Type type);

        /// <summary>
        /// Instantiate an object of the given typeSignature from the serialized stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="typeSignature"></param>
        /// <returns></returns>
        object Instantiate(ISerializationStreamReader stream, string typeSignature);

        /// <summary>
        /// Save an instance into the serialization stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="instance"></param>
        /// <param name="typeSignature"></param>
        void Serialize(ISerializationStreamWriter stream, object instance, string typeSignature);
    }
}
