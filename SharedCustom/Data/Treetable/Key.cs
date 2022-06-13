/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;
using Deephaven.OpenAPI.Core.API.Util;

namespace Deephaven.OpenAPI.Shared.Data.Treetable
{
    public class Key
    {
        public object Leaf { get; set; }
        public long? Nanos { get; set; }
        public object[] Array { get; set; }
        public object[] List { get; set; }

        public static Key Root()
        {
            return new Key();
        }

        public static Key OfArray(Object[] array)
        {
            Key key = new Key();
            key.Array = array;
            return key;
        }

        public static Key OfList<T>(List<T> array)
        {
            Key key = new Key();
            key.List = array.ConvertAll<object>(item => (object)item).ToArray(); // deliberately to object[]
            return key;
        }

        public static Key OfDateTime(long nanos)
        {
            Key key = new Key();
            key.Nanos = nanos;
            return key;
        }

        public static Key OfObject(Object serverKey)
        {
            Key key = new Key();
            key.Leaf = serverKey ?? throw new InvalidOperationException("Server key must not be null");
            return key;
        }

        public bool IsRoot()
        {
            return !IsList() && !IsArray() && !IsLeaf() && !IsDateTime();
        }

        public bool IsArray()
        {
            return Array != null;
        }

        public bool IsList()
        {
            return List != null;
        }

        public bool IsLeaf()
        {
            return Leaf != null;
        }

        public bool IsDateTime()
        {
            return Nanos != null;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj == null || GetType() != obj.GetType()) return false;

            Key key = (Key)obj;
            if (!Object.Equals(Leaf, key.Leaf)) return false;
            if (!Object.Equals(Nanos, key.Nanos)) return false;
            if (!ArrayUtil.ArraysEqual(Array, key.Array)) return false;
            return ArrayUtil.ArraysEqual(List, key.List);
        }

        public override int GetHashCode()
        {
            int result = Leaf == null ? 0 : Leaf.GetHashCode();
            result = 31 * result + (Nanos == null ? 0 : Nanos.GetHashCode());
            result = 31 * result + ArrayUtil.GetHashCode(Array);
            result = 31 * result + ArrayUtil.GetHashCode(List);
            return result;
        }

        public override String ToString()
        {
            if (IsDateTime())
            {
                return "DateTime " + Nanos;
            }
            if (IsArray())
            {
                return "Array " + ArrayUtil.ToString(Array);
            }
            if (IsList())
            {
                return "List " + ArrayUtil.ToString(List);
            }
            if (IsLeaf())
            {
                return "Value " + Leaf;
            }
            return "Root";
        }
    }
}
