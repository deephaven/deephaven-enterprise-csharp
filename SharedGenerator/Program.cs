/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.IO;
using Deephaven.OpenAPI.SharedGenerator;

namespace DeephavenOpenAPI
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.Out.WriteLine("usage: SharedGenerator <schema path> <schema prefixes> <output directory> [<RPC timeout (milliseconds)>]");
                Console.Out.WriteLine();
                Console.Out.WriteLine("This program will generate C# classes for each serializable type and a corresponding"
                                      + " FieldSerializer implementation into the specified <output directory>, unless"
                                      + " custom/hand-coded classes for those types already exist.");
                Console.Out.WriteLine();
                Console.Out.WriteLine(
                    "The provided <schema path> should contain the following JSON files:");
                Console.Out.WriteLine("\t<schema prefix>Server.json");
                Console.Out.WriteLine("\t<schema prefix>Server_ImplSerializer.json");
                Console.Out.WriteLine("\t<schema prefix>Client.json");
                Console.Out.WriteLine("\t<schema prefix>Client_ImplSerializer.json");
                Console.Out.WriteLine();
                Console.Out.WriteLine("<schema prefixes> is a comma-delimited list of Schema Prefixes. The Generator will run for each prefix specified.");
                Console.Out.WriteLine();
                Console.Out.WriteLine("If specified, the RPC timeout parameter is used as the timeout for blocking RPC calls (defaults to 10 minutes).");
                return -1;
            }

            var argIdx = 0;
            var schemaPath = args[argIdx++];
            var schemaPrefixes = args[argIdx++];
            var outputRoot = args[argIdx++];
            var defaultTimeoutMs = args.Length > argIdx ? int.Parse(args[argIdx]) : 10 * 60 * 1000;

            foreach (var schemaPrefix in schemaPrefixes.Split(','))
            {
                Console.Out.WriteLine("Generating code for \"{0}\" schema...", schemaPrefix);

                var serverEndpointJson =
                    File.ReadAllText(schemaPath + Path.DirectorySeparatorChar + schemaPrefix + "Server.json");
                var serverTypesJson =
                    File.ReadAllText(schemaPath + Path.DirectorySeparatorChar + schemaPrefix +
                                     "Server_ImplSerializer.json");
                var clientEndpointJson =
                    File.ReadAllText(schemaPath + Path.DirectorySeparatorChar + schemaPrefix + "Client.json");
                var clientTypesJson =
                    File.ReadAllText(schemaPath + Path.DirectorySeparatorChar + schemaPrefix +
                                     "Client_ImplSerializer.json");

                var codeGenerator = new CodeGenerator(outputRoot, serverEndpointJson, serverTypesJson,
                    clientEndpointJson, clientTypesJson, defaultTimeoutMs);
                codeGenerator.GenerateSyntheticTypes();
                codeGenerator.GenerateArrayTypes();
                codeGenerator.GenerateClientInterface();
                codeGenerator.GenerateServerEndpoint();
            }

            return 0;
        }
    }
}
