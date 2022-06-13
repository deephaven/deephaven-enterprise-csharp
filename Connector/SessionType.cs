/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
namespace Deephaven.Connector
{
    /// <summary>
    /// Deephaven session type. When a new <see cref="DeephavenConnection"/> is
    /// established, a session of one of the following types is established.
    /// Queries on that connection must then be expressed in the relevant language.
    /// </summary>
    public enum SessionType
    {
        Groovy,
        Python
    }
}
