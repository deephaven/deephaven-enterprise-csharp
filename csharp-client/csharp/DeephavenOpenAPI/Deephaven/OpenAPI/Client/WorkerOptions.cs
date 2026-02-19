/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Linq;
using Deephaven.OpenAPI.Shared.Ide;

namespace Deephaven.OpenAPI.Client
{
    /// <summary>
    /// An object representing the options available when starting a new worker
    /// process.
    /// </summary>
    public class WorkerOptions
    {
        internal ConsoleConfig ConsoleConfig { get; }

        /// <summary>
        /// Construct a WorkerOptions object
        /// </summary>
        /// <param name="jvmProfile">Named JVM profile to use, or the string "Default"</param>
        /// <param name="maxHeapMb">Java max heap size for the worker, in megabytes</param>
        public WorkerOptions(string jvmProfile, int maxHeapMb = 2000)
        {
            ConsoleConfig = new ConsoleConfig
            {
                JvmProfile = jvmProfile,
                MaxHeapMb = maxHeapMb,
                JvmArgs = new string[0]
            };
        }

        public string JvmProfile {
            get => ConsoleConfig.JvmProfile;
            set => ConsoleConfig.JvmProfile = value;
        }

        /// <summary>
        /// Java max heap size.
        /// </summary>
        /// <value>
        /// Java max heap size for the worker, in megabytes.
        /// </value>
        public int MaxHeapMB {
            get => ConsoleConfig.MaxHeapMb;
            set => ConsoleConfig.MaxHeapMb = value;
        }

        public bool Debug
        {
            get => ConsoleConfig.Debug;
            set => ConsoleConfig.Debug = value;
        }

        public string[] ExtraJvmArgs
        {
            get => ConsoleConfig.JvmArgs;
            set => ConsoleConfig.JvmArgs = value;
        }

        public void AddJvmArg(string arg)
        {
            ConsoleConfig.JvmArgs = ConsoleConfig.JvmArgs.Concat(new[] {arg}).ToArray();
        }

        public string DispatcherHost
        {
            get => ConsoleConfig.DispatcherHost;
            set => ConsoleConfig.DispatcherHost = value;
        }

        public int DispatcherPort
        {
            get => ConsoleConfig.DispatcherPort;
            set => ConsoleConfig.DispatcherPort = value;
        }

        public string[] Classpath
        {
            get => ConsoleConfig.Classpath;
            set => ConsoleConfig.Classpath = value;
        }

        public string QueryDescription
        {
            get => ConsoleConfig.QueryDescription;
            set => ConsoleConfig.QueryDescription = value;
        }

        public bool DetailedGCLogging
        {
            get => ConsoleConfig.DetailedGCLogging;
            set => ConsoleConfig.DetailedGCLogging = value;
        }

        public bool OmitDefaultGcParameters
        {
            get => ConsoleConfig.OmitDefaultGcParameters;
            set => ConsoleConfig.OmitDefaultGcParameters = value;
        }

        public string[][] EnvVars
        {
            get => ConsoleConfig.EnvVars;
            set => ConsoleConfig.EnvVars = value;
        }
    }
}
