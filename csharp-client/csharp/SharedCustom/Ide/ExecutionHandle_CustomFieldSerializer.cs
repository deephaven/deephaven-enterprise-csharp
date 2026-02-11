/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Concurrent;
using Deephaven.OpenAPI.Core.RPC.Serialization.API;
using Deephaven.OpenAPI.Shared.Cmd;

namespace Deephaven.OpenAPI.Shared.Ide
{
    public class ExecutionHandle_CustomFieldSerializer
    {
        private static readonly ConcurrentDictionary<int, IdFactory<ExecutionHandle>> IdFactories =
            new ConcurrentDictionary<int, IdFactory<ExecutionHandle>>();

        public static void RegisterIdFactory(int connectionId, IdFactory<ExecutionHandle> factory)
        {
            if (IdFactories.GetOrAdd(connectionId, factory) != factory)
            {
                throw new ArgumentException("Multiple ExecutionHandle IdFactory registered for " + connectionId);
            }
        }

        private static IdFactory<ExecutionHandle> GetFactory(int connectionId)
        {
            IdFactory<ExecutionHandle> factory;
            if (!IdFactories.TryGetValue(connectionId, out factory))
            {
                throw new ArgumentException("No IdFactory registered for " + connectionId);
            }
            return factory;
        }

        public static void Serialize(ISerializationStreamWriter writer, ExecutionHandle instance)
        {
            int connection = GetFactory(instance.ConnectionId).GetReplyToId();
            writer.Write(connection);
            writer.Write(instance.ClientId);
            writer.Write(instance.ScriptId);
        }

        public static void Deserialize(ISerializationStreamReader reader, ExecutionHandle instance)
        {
            instance.ScriptId = reader.ReadInt32();
        }

        public static ExecutionHandle Instantiate(ISerializationStreamReader reader)
        {
            var connectionId = reader.ReadInt32();
            var clientId = reader.ReadInt32();
            var factory = GetFactory(connectionId);
            var instance = factory.GetOrCreateHandle(clientId);
            return instance;
        }
    }
}
