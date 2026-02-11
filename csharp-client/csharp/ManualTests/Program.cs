using System;
using System.Collections.Generic;

namespace Deephaven.OpenAPI.ManualTests
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                string exampleName = null;
                string host = null;
                string username = null;
                string password = null;
                string operateAs = null;
                if (args.Length == 4 || args.Length == 5)
                {
                    var argIdx = 0;
                    exampleName = args[argIdx++];
                    host = args[argIdx++];
                    username = args[argIdx++];
                    password = args[argIdx++];
                    operateAs = argIdx < args.Length ? args[argIdx] : username;
                }
                else
                {
                    Console.Error.WriteLine("Program arguments:");
                    Console.Error.WriteLine("  exampleName server username password [operateAs]");
                    Environment.Exit(1);
                }

                new Program(host, username, password, operateAs).Run(exampleName);
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Caught exception: {e}");
                return 1;
            }
        }

        private readonly string _host;
        private readonly string _username;
        private readonly string _password;
        private readonly string _operateAs;
        private readonly Dictionary<string, Action<string, string, string, string>> _actions;

        private Program(string host, string username, string password, string operateAs)
        {
            _host = host;
            _username = username;
            _password = password;
            _operateAs = operateAs;
            _actions = new Dictionary<string, Action<string, string, string, string>>
            {
                { "ids7473", IDS_7473.CrashPQ },
                { "dh11068", DH_11068.TestMalformedHostname },
            };
        }

        private void Run(string exampleName)
        {
            if (!_actions.TryGetValue(exampleName, out var action))
            {
                throw new Exception($"Can't find example {exampleName}");
            }
            action(_host, _username, _password, _operateAs);
        }
    }
}
