/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

namespace Deephaven.OpenAPI.Core.RPC.Serialization.API
{
    public interface ISerializationStreamReader
    {
        /// <summary>
        /// Reads the next piece of data in the stream as if it were a boolean.
        /// </summary>
        /// <exception cref="SerializationException">
        /// If not enough data remains, or the data is not formatted correctly.
        /// </exception>
        /// <returns>The boolean.</returns>
        bool ReadBoolean();

        /// <summary>
        /// Reads the next piece of data in the stream as if it were a byte.
        /// </summary>
        /// <exception cref="SerializationException">
        /// If not enough data remains, or the data is not formatted correctly.
        /// </exception>
        /// <returns>The next byte in the stream.</returns>
        sbyte ReadSByte();

        /// <summary>
        /// Reads the next piece of data in the stream as if it were a char.
        /// </summary>
        /// <exception cref="SerializationException">
        /// If not enough data remains, or the data is not formatted correctly.
        /// </exception>
        /// <returns>The next char in the stream.</returns>
        char ReadChar();

        /// <summary>
        /// Reads the next piece of data in the stream as if it were a double.
        /// </summary>
        /// <exception cref="SerializationException">
        /// If not enough data remains, or the data is not formatted correctly.
        /// </exception>
        /// <returns>The next double in the stream.</returns>
        double ReadDouble();

        /// <summary>
        /// Reads the next piece of data in the stream as if it were a float.
        /// </summary>
        /// <exception cref="SerializationException">
        /// If not enough data remains, or the data is not formatted correctly.
        /// </exception>
        /// <returns>The next float in the stream.</returns>
        float ReadSingle();

        /// <summary>
        /// Reads the next piece of data in the stream as if it were a 32 bit signed integer.
        /// </summary>
        /// <exception cref="SerializationException">
        /// If not enough data remains, or the data is not formatted correctly.
        /// </exception>
        /// <returns>The next int in the stream.</returns>
        int ReadInt32();

        /// <summary>
        /// Reads the next piece of data in the stream as if it were a long.
        /// </summary>
        /// <exception cref="SerializationException">
        /// If not enough data remains, or the data is not formatted correctly.
        /// </exception>
        /// <returns>The next long in the stream.</returns>
        long ReadInt64();

        /// <summary>
        /// Reads the next piece of data in the stream as if it were an Object, delegating to
        /// the wrapped TypeSerializer to instantiate instances and deserialize fields. Will
        /// be recursive as necessary to pick up more objects and fields.
        /// </summary>
        /// <exception cref="SerializationException">
        /// If not enough data remains, or the data is not formatted correctly.
        /// </exception>
        /// <returns>The next Object in the stream, with all of its fields.</returns>
        object ReadObject();

        /// <summary>
        /// Reads the next piece of data in the stream as if it were a signed 16 bit integer.
        /// </summary>
        /// <exception cref="SerializationException">
        /// If not enough data remains, or the data is not formatted correctly.
        /// </exception>
        /// <returns>The next short in the stream.</returns>
        short ReadInt16();

        /// <summary>
        /// Reads the next reference from the stream, and use that to look up a string in the table.
        /// </summary>
        /// <exception cref="SerializationException">
        /// If not enough data remains, or the data is not formatted correctly.
        /// </exception>
        /// <returns>The next String in the stream, without reading it as an object first.</returns>
        string ReadString();

        /// <summary>
        /// Utility to ensure that there are enough remaining pieces of data in the payload
        /// to allow an array of that size to be allocated.Normally this will just do a quick
        /// bounds check which will pass, and decrement a field in the stream to prevent those
        /// items from being claimed again.
        ///
        /// The attack that this mitigates is where a client pretends to have an Object[]
        /// with 1m items, and the first item in there is an Object[] with 1m items, and
        /// so on.Each "layer" deep needs enough memory for another million references so with
        /// a request that is only a few kb, many gigabytes of memory are needed.
        ///
        /// "Claiming" those slots ahead of time would force a bounds check on the total size
        /// of the incoming payload - if there aren't a million items in the payload, obviously
        /// that array won't actually be that large. Once they are claimed by one array, they
        /// can't be claimed again, so even if there are 1 million items, the next array can't
        /// also pretend to be that large, unless the incoming stream is actually that big.
        ///
        /// As an implementation detail, the range check should be performed such that the
        /// sum of all claimed items is smaller than the size of the smallest item, across
        /// all parts of the stream (main payload and string table). For example, if a stream
        /// implementation stores booleans as a single bit, but longs as 64 bits, a boolean[]
        /// could well be 64 times smaller than a long[], but this limits the attacker to only
        /// consuming a constant factor more memory than the request itself uses. The check
        /// might look something like this:
        ///
        ///   if (alreadyClaimed + newlyClaimed > slotsInStream + stringsInTable) {
        ///       throw new SerializationException("Request claims to be larger than it is");
        ///   }
        ///   alreadyClaimed += newlyClaimed;
        ///
        /// </summary>
        /// <exception cref="SerializationException">
        /// if there are too few remaining pieces of data in the stream to allow this
        /// collection to be deserialized
        /// </exception>
        void ClaimItems(int slots);
    }
}
