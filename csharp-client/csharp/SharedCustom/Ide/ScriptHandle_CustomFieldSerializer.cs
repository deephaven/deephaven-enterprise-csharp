/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using System;
using System.Collections.Concurrent;
using Deephaven.OpenAPI.Core.RPC.Serialization.API;
using Deephaven.OpenAPI.Shared.Cmd;

namespace Deephaven.OpenAPI.Shared.Ide
{
    public class ScriptHandle_CustomFieldSerializer
    {
        private static readonly ConcurrentDictionary<int, IdFactory<ScriptHandle>> IdFactories =
            new ConcurrentDictionary<int, IdFactory<ScriptHandle>>();

        public static void RegisterIdFactory(int connectionId, IdFactory<ScriptHandle> factory)
        {
            if (IdFactories.GetOrAdd(connectionId, factory) != factory)
            {
                throw new ArgumentException("Multiple IdFactory registered for " + connectionId);
            }
        }

        private static IdFactory<ScriptHandle> GetFactory(int connectionId)
        {
            IdFactory<ScriptHandle> factory;
            if (!IdFactories.TryGetValue(connectionId, out factory))
            {
                throw new ArgumentException("No IdFactory registered for " + connectionId);
            }
            return factory;
        }

        public static void Serialize(ISerializationStreamWriter writer, ScriptHandle instance)
        {
            var connection = GetFactory(instance.ConnectionId).GetReplyToId();
            writer.Write(connection);
            writer.Write(instance.ClientId);
            writer.Write(instance.ScriptId);
        }

        public static void Deserialize(ISerializationStreamReader reader, ScriptHandle instance)
        {
            instance.ScriptId = reader.ReadInt32();
        }

        public static ScriptHandle Instantiate(ISerializationStreamReader reader)
        {
            var connectionId = reader.ReadInt32();
            var clientId = reader.ReadInt32();
            var factory = GetFactory(connectionId);
            var instance = factory.GetOrCreateHandle(clientId);
            return instance;
        }
    }
}
