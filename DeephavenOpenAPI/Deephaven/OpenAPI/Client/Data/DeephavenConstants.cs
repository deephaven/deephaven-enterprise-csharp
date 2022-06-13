namespace Deephaven.OpenAPI.Client.Data
{
    public static class DeephavenConstants
    {
        public const sbyte NULL_BYTE = sbyte.MinValue;
        public const sbyte MIN_BYTE = sbyte.MinValue + 1;
        public const sbyte MAX_BYTE = sbyte.MaxValue;

        public const char NULL_CHAR = (char) (char.MaxValue - 1);
        public const char MIN_CHAR = char.MinValue;
        public const char MAX_CHAR = char.MaxValue;

        public const double NULL_DOUBLE = -double.MaxValue;
        public const float NULL_FLOAT = -float.MaxValue;

        public const int NULL_INT = int.MinValue;
        public const int MIN_INT = int.MinValue + 1;
        public const int MAX_INT = int.MaxValue;

        public const long NULL_LONG = long.MinValue;
        public const long MIN_LONG = long.MinValue + 1;
        public const long MAX_LONG = long.MaxValue;

        public const short NULL_SHORT = short.MinValue;
        public const short MIN_SHORT = short.MinValue + 1;
        public const short MAX_SHORT = short.MaxValue;
    }
}
