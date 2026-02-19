/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client
{
    /// <summary>
    /// A log message originating from a Deephaven service.
    /// </summary>
    public class LogMessage
    {
        private LogItem _logItem;

        internal LogMessage(LogItem logItem)
        {
            _logItem = logItem;
        }
        
        public double Micros { get { return _logItem.Micros; } }
        public string LogLevel { get { return _logItem.LogLevel; } }
        public string Message { get { return _logItem.Message; } }
    }
}
