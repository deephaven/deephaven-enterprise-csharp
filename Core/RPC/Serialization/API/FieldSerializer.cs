/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.API
{
    /// <summary>
    /// Do not use this as a base type for CustomFieldSerializers. This is intended for implementation by
    /// generated Field Serializers, which will either directly read/write each object field or
    /// delegate to static methods on Custom field serializers if a custom serializer types is
    /// specified in the JSON type description.
    /// </summary>
    public abstract class FieldSerializer
    {
        public virtual void Deserial(ISerializationStreamReader reader, object instance)
        {
            // default impl does nothing
        }

        public virtual void Serial(ISerializationStreamWriter writer, object instance)
        {
            // default impl does nothing
        }

        public virtual object Create(ISerializationStreamReader reader)
        {
            throw new Exception(
                "Cannot create an instance of this type - abstract, has no default constructor, or only subtypes are whitelisted");
        }
    }
}
