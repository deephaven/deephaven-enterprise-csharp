/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

namespace Deephaven.OpenAPI.Shared.Data
{
    public class TableMapHandle
    {
        public int ServerId { get; set; }

        public TableMapHandle()
        {
        }

        public TableMapHandle(int serverId)
        {
            ServerId = serverId;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj == null || GetType() != obj.GetType()) return false;

            TableMapHandle that = (TableMapHandle)obj;

            return ServerId == that.ServerId;
        }

        public override int GetHashCode()
        {
            return ServerId;
        }
    }
}
