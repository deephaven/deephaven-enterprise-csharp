using System;

namespace Deephaven.OpenAPI.Core.API.Util
{
    /// <summary>
    /// An exception thrown when the watchdog expires.
    /// </summary>
    public class WatchdogException : Exception
    {
        internal WatchdogException(string reason) : base(reason)
        {
        }
    }
}