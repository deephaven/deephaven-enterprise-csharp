/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Concurrent;
using Deephaven.OpenAPI.Core.RPC.Serialization.API;
using Deephaven.OpenAPI.Shared.Cmd;

namespace Deephaven.OpenAPI.Shared.Data
{
    public class TableHandle_CustomFieldSerializer
    {
        private static readonly ConcurrentDictionary<int, IdFactory<TableHandle>> IdFactories =
            new ConcurrentDictionary<int, IdFactory<TableHandle>>();

        public static void RegisterIdFactory(int connectionId, IdFactory<TableHandle> factory)
        {
            if (IdFactories.GetOrAdd(connectionId, factory) != factory)
            {
                throw new ArgumentException($"Multiple IdFactories registered for {connectionId}");
            }
        }

        private static IdFactory<TableHandle> GetFactory(int connectionId)
        {
            if (!IdFactories.TryGetValue(connectionId, out var factory))
            {
                throw new ArgumentException($"No IdFactory registered for ${connectionId}");
            }
            return factory;
        }

        public static void Deserialize(ISerializationStreamReader reader, TableHandle instance)
        {
            var serverId = reader.ReadInt32();
            if (serverId >= 0)
            {
                instance.SetServerId(serverId);
            }
        }

        public static TableHandle Instantiate(ISerializationStreamReader reader)
        {
            var connectionId = reader.ReadInt32();
            var clientId = reader.ReadInt32();
            var factory = GetFactory(connectionId);
            return factory.GetOrCreateHandle(clientId);
        }

        public static void Serialize(ISerializationStreamWriter writer, TableHandle instance)
        {
            var connectionId = GetFactory(instance.ConnectionId).GetReplyToId();
            writer.Write(connectionId);
            writer.Write(instance.ClientId);
            writer.Write(instance.ServerId);
        }
    }
}
