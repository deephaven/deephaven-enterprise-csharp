/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
namespace Deephaven.OpenAPI.Core.RPC.Serialization.API
{
    public interface ISerializationStreamWriter
    {
        void Write(bool value);
        void Write(sbyte value);
        void Write(char value);
        void Write(double value);
        void Write(float value);
        void Write(int value);
        void Write(long value);
        void WriteObject(object value);
        void Write(short value);
        void Write(string value);
    }
}
