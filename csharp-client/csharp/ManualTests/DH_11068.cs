using System;
using Deephaven.OpenAPI.Client;

namespace Deephaven.OpenAPI.ManualTests
{
    public class DH_11068
    {
        public static void TestMalformedHostname(string host, string username, string password, string operateAs)
        {
            try
            {
                OpenApi.Connect(null);
                throw new Exception("Expected exception, but call was successful");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Success! Caught *expected* exception {e.Message}");
            }
        }
    }
}
