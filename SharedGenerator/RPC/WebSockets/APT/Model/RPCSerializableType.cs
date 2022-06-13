/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

namespace Deephaven.OpenAPI.SharedGenerator.RPC.WebSockets.APT.Model
{
    public class RpcSerializableTypeKind
    {
        public const string Array = "ARRAY";
        public const string Composite = "COMPOSITE";
        public const string Enum = "ENUM";
    }

    public class RpcSerializableType
    {
        // for composites, the super type
        public string SuperTypeId { get; set; }

        // any interfaces this type implements
        public string[] InterfaceTypeIds { get; set; }

        // if this is an abstract class (don't try to instantiate)
        public bool IsAbstract { get; set; }

        // array/composite/primitive/enum
        public string Kind { get; set; }

        // the type name to use in generated code, _with_ the namespace/package
        // e.g. primitive type:     System.Byte
        //      primitive array:    System.Byte[]
        //      composite:          com.illumon.iris.web.shared.cmd.RequestId
        //      composite array:    com.illumon.iris.web.shared.cmd.RequestId[]
        public string Name { get; set; }

        // for array types, the component type id (NOT the TypeName)
        public string ComponentTypeId { get; set; }

        // for composite types, the fields to serialize
        public RpcSerializableTypeField[] Properties { get; set; }

        // for enum types, the legal values
        public string[] EnumValues { get; set; }

        public bool CanInstantiate { get; set; }
        public bool CanDeserialize { get; set; }
        public bool CanSerialize { get; set; }
        public string CustomFieldSerializer { get; set; }
    }
}
