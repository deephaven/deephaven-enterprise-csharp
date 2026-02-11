/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

namespace Deephaven.OpenAPI.Shared.Data
{
    public class HandleMapping
    {
        public TableHandle Source { get; set; }
        public TableHandle NewId { get; set; }

        public HandleMapping()
        {
        }

        public HandleMapping(TableHandle source, TableHandle newId)
        {
            Source = source;
            NewId = newId;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            if (obj == null || GetType() != obj.GetType())
                return false;
            var that = (HandleMapping) obj;
            if (!Source.Equals(that.Source))
                return false;
            return NewId.Equals(that.NewId);
        }

        public override int GetHashCode()
        {
            var result = Source.GetHashCode();
            return 31 * result + NewId.GetHashCode();
        }

        public override string ToString()
        {
            return "HandleMapping{source=" + Source + ", newId=" + NewId + "}";
        }
    }
}
