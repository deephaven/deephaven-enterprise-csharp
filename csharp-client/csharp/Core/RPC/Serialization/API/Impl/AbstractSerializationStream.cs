/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

namespace Deephaven.OpenAPI.Core.RPC.Serialization.API.Impl
{
    /// <summary>
    /// Base class for the client and server serialization streams. This class
    /// handles the basic serialization and deserialization formatting for primitive
    /// types since these are common between the client and server.
    /// </summary>
    public class AbstractSerializationStream
    {
        /// <summary>
        /// The default flags to be used by serialization streams.
        /// </summary>
        public const int DefaultFlags = 0;

        /// <summary>
        /// The current RPC protocol version.
        /// </summary>
        public const int SerializationStreamVersion = 7;

        /// <summary>
        /// Indicates that obfuscated type names should be used in the RPC payload.
        /// </summary>
        public const int FlagElideTypeNames = 0x1;

        /// <summary>
        /// Indicates that RPC token is included in the RPC payload.
        /// </summary>
        public const int FlagRpcTokenIncluded = 0x2;

        /// <summary>
        /// Bit mask representing all valid flags.
        /// </summary>
        public const int ValidFlagsMask = FlagElideTypeNames | FlagRpcTokenIncluded;

        private int _flags = DefaultFlags;
        private int _version = SerializationStreamVersion;

        public void AddFlags(int flags)
        {
            this._flags |= flags;
        }

        /// <summary>
        /// Checks if flags are valid.
        /// </summary>
        /// <returns><code>true</code> if flags are valid and <code>false</code> otherwise.</returns>
        public bool AreFlagsValid()
        {
            return ((_flags | ValidFlagsMask) ^ ValidFlagsMask) == 0;
        }

        public int GetFlags()
        {
            return _flags;
        }

        public int GetVersion()
        {
            return _version;
        }

        public bool HasFlags(int flags)
        {
            return (GetFlags() & flags) == flags;
        }

        public void SetFlags(int flags)
        {
            _flags = flags;
        }

        protected void SetVersion(int version)
        {
            _version = version;
        }
    }
}
