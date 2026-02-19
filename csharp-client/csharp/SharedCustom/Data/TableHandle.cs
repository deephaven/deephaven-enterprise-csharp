/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Runtime.CompilerServices;
using Deephaven.OpenAPI.Shared.Cmd;

namespace Deephaven.OpenAPI.Shared.Data
{
    public class TableHandle : ServerObjectHandle, IComparable<TableHandle>, IEquatable<TableHandle>
    {
        public int ClientId { get; }
        public int ConnectionId { get; }
        private int _serverId;

        public int ServerId
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _serverId;
        }

        public TableHandle(int clientId, int connectionId)
        {
            if (clientId == 0)
            {
                throw new InvalidOperationException("clientId must be non-zero");
            }

            ClientId = clientId;
            ConnectionId = connectionId;
            _serverId = Uninitialized;
        }

        public override int GetClientId()
        {
            return ClientId;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetServerId(int newValue)
        {
            if (_serverId == newValue)
            {
                return;
            }

            if (_serverId >= 0)
            {
                throw new Exception($"TableHandle already has server id {_serverId}; can't change to {newValue}");
            }

            _serverId = newValue;
        }

        public bool ServerIdIsAssigned
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _serverId != Uninitialized;
        }

        public bool Equals(TableHandle other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other != null && ClientId == other.ClientId && ConnectionId == other.ConnectionId;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TableHandle);
        }

        public override int GetHashCode()
        {
            return ClientId ^ 31 + ConnectionId;
        }

        public override string ToString()
        {
            return $"TableHandle{{clientId={ClientId}, serverId={ServerId}, connectionId={ConnectionId}}}";
        }

        public int CompareTo(TableHandle other)
        {
            return ClientId.CompareTo(other.ClientId);
        }
    }
}
