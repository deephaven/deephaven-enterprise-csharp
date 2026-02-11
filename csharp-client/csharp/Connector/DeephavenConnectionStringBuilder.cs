/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System.Collections;

namespace Deephaven.Connector
{
    /// <summary>
    /// A strongly typed connection string builder for Deephaven connections.
    /// </summary>
    public class DeephavenConnectionStringBuilder : DbConnectionStringBuilder
    {
        /// <summary>
        /// Makes all valid keywords for a property to that property (e.g. User Name -> Username, UserId -> Username...)
        /// </summary>
        static readonly Dictionary<string, PropertyInfo> PropertiesByKeyword;

        /// <summary>
        /// Maps CLR property names (e.g. BufferSize) to their canonical keyword name, which is the
        /// property's [DisplayName] (e.g. Buffer Size)
        /// </summary>
        static readonly Dictionary<string, string> PropertyNameToCanonicalKeyword;

        /// <summary>
        /// Maps each property to its [DefaultValue]
        /// </summary>
        static readonly Dictionary<PropertyInfo, object> PropertyDefaults;

        // we have strongly typed values for all the standard properties;
        // these will be set to the default value if not overridden by the
        // user (unlike the dictionary values kept by the base class, which are
        // null if not explicitly set)
        private string _host;
        private int _port;
        private string _username;
        private string _password;
        private string _operateAs;
        private int _remoteDebugPort;
        private bool _suspendWorker;
        private int _maxHeapMb;
        private int _timeoutMs;
        private SessionType _sessionType;
        private bool _localDateAsString;
        private bool _localTimeAsString;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeephavenConnectionStringBuilder"/> class.
        /// </summary>
        public DeephavenConnectionStringBuilder()
        {
            Init();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeephavenConnectionStringBuilder"/> class, optionally using ODBC rules for quoting values.
        /// </summary>
        /// <param name="useOdbcRules">true to use {} to delimit fields; false to use quotation marks.</param>
        public DeephavenConnectionStringBuilder(bool useOdbcRules) : base(useOdbcRules)
        {
            Init();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeephavenConnectionStringBuilder"/> class with the given connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        public DeephavenConnectionStringBuilder(string connectionString)
        {
            Init();
            ConnectionString = connectionString;
        }

        private void Init()
        {
            // Set the strongly-typed properties to their default values
            foreach (var kv in PropertyDefaults)
                kv.Key.SetValue(this, kv.Value);

            // Setting the strongly-typed properties here also set the string-based properties in the base class.
            // Clear them (default settings = empty connection string)
            base.Clear();
        }

        /// <summary>
        /// DataSource represents "the database" which in our case is the URL to the primary API.
        /// </summary>
        internal string DataSource => $"wss://{Host}:{Port}/socket";

        #region Attributes

        /// <summary>
        /// Marks on <see cref="DeephavenConnectionStringBuilder"/> which participate in the connection
        /// string. Optionally holds a set of synonyms for the property.
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class DeephavenConnectionStringPropertyAttribute : Attribute
        {
            /// <summary>
            /// Holds a list of synonyms for the property.
            /// </summary>
            public string[] Synonyms { get; }

            /// <summary>
            /// Creates a <see cref="DeephavenConnectionStringPropertyAttribute"/>.
            /// </summary>
            public DeephavenConnectionStringPropertyAttribute()
            {
                Synonyms = DeephavenConnectionStringBuilder.EmptyStringArray;
            }

            /// <summary>
            /// Creates a <see cref="DeephavenConnectionStringPropertyAttribute"/>.
            /// </summary>
            public DeephavenConnectionStringPropertyAttribute(params string[] synonyms)
            {
                Synonyms = synonyms;
            }
        }

        #endregion

        #region Static initialization

        static DeephavenConnectionStringBuilder()
        {
            var properties = typeof(DeephavenConnectionStringBuilder)
                .GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(DeephavenConnectionStringPropertyAttribute)))
                .ToArray();

            Debug.Assert(properties.All(p => p.CanRead && p.CanWrite));
            Debug.Assert(properties.All(p => p.GetCustomAttribute<DisplayNameAttribute>() != null));

            PropertiesByKeyword = (
                from p in properties
                let displayName = p.GetCustomAttribute<DisplayNameAttribute>().DisplayName.ToUpperInvariant()
                let propertyName = p.Name.ToUpperInvariant()
                from k in new[] { displayName }
                  .Concat(propertyName != displayName ? new[] { propertyName } : EmptyStringArray)
                  .Concat(p.GetCustomAttribute<DeephavenConnectionStringPropertyAttribute>().Synonyms
                    .Select(a => a.ToUpperInvariant())
                  )
                  .Select(k => new { Property = p, Keyword = k })
                select k
            ).ToDictionary(t => t.Keyword, t => t.Property);

            PropertyNameToCanonicalKeyword = properties.ToDictionary(
                p => p.Name,
                p => p.GetCustomAttribute<DisplayNameAttribute>().DisplayName
            );

            PropertyDefaults = properties
                .Where(p => p.GetCustomAttribute<ObsoleteAttribute>() == null)
                .ToDictionary(
                p => p,
                p => p.GetCustomAttribute<DefaultValueAttribute>() != null
                    ? p.GetCustomAttribute<DefaultValueAttribute>().Value
                    : (p.PropertyType.GetTypeInfo().IsValueType ? Activator.CreateInstance(p.PropertyType) : null)
            );
        }

        #endregion

        #region Non-static property handling

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="keyword">The key of the item to get or set.</param>
        /// <returns>The value associated with the specified key.</returns>
        public override object this[string keyword]
        {
            get
            {
                if (!TryGetValue(keyword, out var value))
                    throw new ArgumentException("Keyword not supported: " + keyword, nameof(keyword));
                return value;
            }
            set
            {
                if (value == null)
                {
                    Remove(keyword);
                    return;
                }

                var p = GetProperty(keyword);
                try
                {
                    object convertedValue;
                    if (p.PropertyType.GetTypeInfo().IsEnum && value is string)
                    {
                        convertedValue = Enum.Parse(p.PropertyType, (string)value);
                    }
                    else
                    {
                        convertedValue = Convert.ChangeType(value, p.PropertyType);
                    }
                    p.SetValue(this, convertedValue);
                }
                catch (Exception e)
                {
                    throw new ArgumentException("Couldn't set " + keyword, keyword, e);
                }
            }
        }

        /// <summary>
        /// Adds an item to the <see cref="DeephavenConnectionStringBuilder"/>.
        /// </summary>
        /// <param name="item">The key-value pair to be added.</param>
        public void Add(KeyValuePair<string, object> item)
            => this[item.Key] = item.Value;

        /// <summary>
        /// Removes the entry with the specified key from the DbConnectionStringBuilder instance.
        /// </summary>
        /// <param name="keyword">The key of the key/value pair to be removed from the connection string in this DbConnectionStringBuilder.</param>
        /// <returns><b>true</b> if the key existed within the connection string and was removed; <b>false</b> if the key did not exist.</returns>
        public override bool Remove(string keyword)
        {
            var p = GetProperty(keyword);
            var canonicalName = PropertyNameToCanonicalKeyword[p.Name];
            var removed = base.ContainsKey(canonicalName);
            // Note that string property setters call SetValue, which itself calls base.Remove().
            p.SetValue(this, PropertyDefaults[p]);
            base.Remove(canonicalName);
            return removed;
        }

        /// <summary>
        /// Removes the entry from the DbConnectionStringBuilder instance.
        /// </summary>
        /// <param name="item">The key/value pair to be removed from the connection string in this DbConnectionStringBuilder.</param>
        /// <returns><b>true</b> if the key existed within the connection string and was removed; <b>false</b> if the key did not exist.</returns>
        public bool Remove(KeyValuePair<string, object> item)
            => Remove(item.Key);

        /// <summary>
        /// Clears the contents of the <see cref="DeephavenConnectionStringBuilder"/> instance.
        /// </summary>
        public override void Clear()
        {
            Debug.Assert(Keys != null);
            foreach (var k in Keys.ToArray())
            {
                Remove(k);
            }
        }

        /// <summary>
        /// Determines whether the <see cref="DeephavenConnectionStringBuilder"/> contains a specific key.
        /// </summary>
        /// <param name="keyword">The key to locate in the <see cref="DeephavenConnectionStringBuilder"/>.</param>
        /// <returns><b>true</b> if the <see cref="DeephavenConnectionStringBuilder"/> contains an entry with the specified key; otherwise <b>false</b>.</returns>
        public override bool ContainsKey(string keyword)
            => keyword == null
                ? throw new ArgumentNullException(nameof(keyword))
                : PropertiesByKeyword.ContainsKey(keyword.ToUpperInvariant());

        /// <summary>
        /// Determines whether the <see cref="DeephavenConnectionStringBuilder"/> contains a specific key-value pair.
        /// </summary>
        /// <param name="item">The item to locate in the <see cref="DeephavenConnectionStringBuilder"/>.</param>
        /// <returns><b>true</b> if the <see cref="DeephavenConnectionStringBuilder"/> contains the entry; otherwise <b>false</b>.</returns>
        public bool Contains(KeyValuePair<string, object> item)
            => TryGetValue(item.Key, out var value) &&
               ((value == null && item.Value == null) || (value != null && value.Equals(item.Value)));

        PropertyInfo GetProperty(string keyword)
            => PropertiesByKeyword.TryGetValue(keyword.ToUpperInvariant(), out var p)
                ? p
                : throw new ArgumentException("Keyword not supported: " + keyword, nameof(keyword));

        /// <summary>
        /// Retrieves a value corresponding to the supplied key from this <see cref="DeephavenConnectionStringBuilder"/>.
        /// </summary>
        /// <param name="keyword">The key of the item to retrieve.</param>
        /// <param name="value">The value corresponding to the key.</param>
        /// <returns><b>true</b> if keyword was found within the connection string, <b>false</b> otherwise.</returns>
        public override bool TryGetValue(string keyword, out object value)
        {
            if (keyword == null)
                throw new ArgumentNullException(nameof(keyword));

            if (!PropertiesByKeyword.ContainsKey(keyword.ToUpperInvariant()))
            {
                value = null;
                return false;
            }

            value = GetProperty(keyword).GetValue(this) ?? "";
            return true;

        }

        private void SetValue(string propertyName, object value)
        {
            var canonicalKeyword = PropertyNameToCanonicalKeyword[propertyName];
            if (value == null)
                base.Remove(canonicalKeyword);
            else
                base[canonicalKeyword] = value;
        }

        #endregion

        /// <summary>
        /// The hostname or IP address of the WebAPI server.
        /// </summary>
        [Category("Connection")]
        [Description("The host or IP address of the Deephaven WebAPI server.")]
        [DisplayName("Host")]
        [DeephavenConnectionStringProperty("Server")]
        public string Host
        {
            get => _host;
            set
            {
                _host = value;
                SetValue(nameof(Host), value);
            }
        }

        /// <summary>
        /// The TCP/IP port of the WebAPI server.
        /// </summary>
        [Category("Connection")]
        [Description("The TCP/IP port of the WebAPI server.")]
        [DisplayName("Port")]
        [DeephavenConnectionStringProperty]
        [DefaultValue(DeephavenConnection.DefaultPort)]
        public int Port
        {
            get => _port;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Invalid port: " + value);
                _port = value;
                SetValue(nameof(Port), value);
            }
        }

        /// <summary>
        /// The username to connect with.
        /// </summary>
        [Category("Connection")]
        [Description("The username to connect with.")]
        [DisplayName("Username")]
        [DeephavenConnectionStringProperty("User Name", "UserId", "User Id", "UID")]
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                SetValue(nameof(Username), value);
            }
        }

        /// <summary>
        /// The user to operate-as.
        /// </summary>
        [Category("Connection")]
        [Description("The username to operate-as.")]
        [DisplayName("OperateAs")]
        [DeephavenConnectionStringProperty]
        public string OperateAs
        {
            get => _operateAs;
            set
            {
                _operateAs = value;
                SetValue(nameof(OperateAs), value);
            }
        }

        /// <summary>
        /// The password to connect with.
        /// </summary>
        [Category("Connection")]
        [Description("The password to connect with.")]
        [PasswordPropertyText(true)]
        [DisplayName("Password")]
        [DeephavenConnectionStringProperty("PSW", "PWD")]
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                SetValue(nameof(Password), value);
            }
        }

        /// <summary>
        /// Port on which the server process should bind on for debugging
        /// </summary>
        [Category("Connection")]
        [Description("Remote session debugging port.")]
        [DisplayName("RemoteDebugPort")]
        [DeephavenConnectionStringProperty]
        public int RemoteDebugPort
        {
            get => _remoteDebugPort;
            set
            {
                _remoteDebugPort = value;
                SetValue(nameof(RemoteDebugPort), value);
            }
        }

        /// <summary>
        /// Whether the worker process should suspend and wait for a debugger on startup.
        /// </summary>
        [Category("Connection")]
        [Description("Suspend worker.")]
        [DisplayName("SuspendWorker")]
        [DeephavenConnectionStringProperty]
        public bool SuspendWorker
        {
            get => _suspendWorker;
            set
            {
                _suspendWorker = value;
                SetValue(nameof(SuspendWorker), value);
            }
        }

        /// <summary>
        /// Maximum heap size for worker process in MB.
        /// </summary>
        [Category("Connection")]
        [Description("Maximum heap size in MB.")]
        [DisplayName("MaxHeapMb")]
        [DeephavenConnectionStringProperty]
        [DefaultValue(DeephavenConnection.DefaultMaxHeapMb)]
        public int MaxHeapMb
        {
            get => _maxHeapMb;
            set
            {
                _maxHeapMb = value;
                SetValue(nameof(MaxHeapMb), value);
            }
        }

        /// <summary>
        /// Connection timeout in milliseconds.
        /// </summary>
        [Category("Connection")]
        [Description("Connection timeout in milliseconds.")]
        [DisplayName("TimeoutMs")]
        [DeephavenConnectionStringProperty]
        [DefaultValue(DeephavenConnection.DefaultTimeoutMs)]
        public int TimeoutMs
        {
            get => _timeoutMs;
            set
            {
                _timeoutMs = value;
                SetValue(nameof(TimeoutMs), value);
            }
        }

        /// <summary>
        /// The query session type.
        /// </summary>
        [Category("Connection")]
        [Description("The session type (Groovy/Python).")]
        [DisplayName("SessionType")]
        [DeephavenConnectionStringProperty]
        [DefaultValue(DeephavenConnection.DefaultSessionType)]
        public SessionType SessionType
        {
            get => _sessionType;
            set
            {
                _sessionType = value;
                SetValue(nameof(SessionType), value);
            }
        }

        /// <summary>
        /// Whether to return LocalDate column data as strings instead of DateTime.
        /// </summary>
        [Category("Connection")]
        [Description("Whether to return LocalDate column data as strings instead of DateTime.")]
        [DisplayName("LocalDateAsString")]
        [DeephavenConnectionStringProperty]
        [DefaultValue(false)]
        public bool LocalDateAsString
        {
            get => _localDateAsString;
            set
            {
                _localDateAsString = value;
                SetValue(nameof(LocalDateAsString), value);
            }
        }

        /// <summary>
        /// Whether to return LocalTime column data as strings instead of DateTime.
        /// </summary>
        [Category("Connection")]
        [Description("Whether to return LocalTime column data as strings instead of DateTime.")]
        [DisplayName("LocalTimeAsString")]
        [DeephavenConnectionStringProperty]
        [DefaultValue(false)]
        public bool LocalTimeAsString
        {
            get => _localTimeAsString;
            set
            {
                _localTimeAsString = value;
                SetValue(nameof(LocalTimeAsString), value);
            }
        }

        #region Misc

        internal string ToStringWithoutPassword()
        {
            var clone = Clone();
            clone.Password = null;
            return clone.ToString();
        }

        internal DeephavenConnectionStringBuilder Clone() => new DeephavenConnectionStringBuilder(ConnectionString);

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public override bool Equals(object obj)
            => obj is DeephavenConnectionStringBuilder o && EquivalentTo(o);

        /// <summary>
        /// Hash function.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => Host?.GetHashCode() ?? 0;

        #endregion

        #region IDictionary<string, object>

        /// <summary>
        /// Gets an ICollection{string} containing the keys of the <see cref="DeephavenConnectionStringBuilder"/>.
        /// </summary>
        public new ICollection<string> Keys => new List<string>(base.Keys.Cast<string>());

        /// <summary>
        /// Gets an ICollection{string} containing the values in the <see cref="DeephavenConnectionStringBuilder"/>.
        /// </summary>
        public new ICollection<object> Values => base.Values.Cast<object>().ToList();

        /// <summary>
        /// Copies the elements of the <see cref="DeephavenConnectionStringBuilder"/> to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional Array that is the destination of the elements copied from <see cref="DeephavenConnectionStringBuilder"/>.
        /// The Array must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in array at which copying begins.
        /// </param>
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            foreach (var kv in this)
                array[arrayIndex++] = kv;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="DeephavenConnectionStringBuilder"/>.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach (var k in Keys)
                yield return new KeyValuePair<string, object>(k, this[k]);
        }

        #endregion IDictionary<string, object>

        #region ICustomTypeDescriptor

        protected override void GetProperties(Hashtable propertyDescriptors)
        {
            // Tweak which properties are exposed via TypeDescriptor. This affects the VS DDEX
            // provider, for example.
            base.GetProperties(propertyDescriptors);

            var toRemove = propertyDescriptors.Values
                .Cast<PropertyDescriptor>()
                .Where(d =>
                    !d.Attributes.Cast<Attribute>().Any(a => a is DeephavenConnectionStringPropertyAttribute) ||
                    d.Attributes.Cast<Attribute>().Any(a => a is ObsoleteAttribute)
                )
                .ToList();
            foreach (var o in toRemove)
                propertyDescriptors.Remove(o.DisplayName);
        }

        #endregion

        internal static readonly string[] EmptyStringArray = new string[0];
    }
}
