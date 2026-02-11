/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;

namespace Deephaven.Connector
{
    /// <summary>
    /// A collection of parameters relevant to a <see cref="DeephavenCommand"/>.
    /// </summary>
    public class DeephavenParameterCollection : DbParameterCollection, IList<DeephavenParameter>
    {
        readonly List<DeephavenParameter> _internalList = new List<DeephavenParameter>(5);

        // Dictionary lookups for GetValue to improve performance
        Dictionary<string, int> _lookup;
        Dictionary<string, int> _lookupIgnoreCase;

        /// <summary>
        /// Initializes a new instance of the DeephavenParameterCollection class.
        /// </summary>
        internal DeephavenParameterCollection()
        {
            InvalidateHashLookups();
        }

        /// <summary>
        /// Invalidate the hash lookup tables.  This should be done any time a change
        /// may throw the lookups out of sync with the list.
        /// </summary>
        internal void InvalidateHashLookups()
        {
            _lookup = null;
            _lookupIgnoreCase = null;
        }

        #region DeephavenParameterCollection Member

        /// <summary>
        /// Gets the <see cref="DeephavenParameter">DeephavenParameter</see> with the specified name.
        /// </summary>
        /// <param name="parameterName">The name of the <see cref="DeephavenParameter">DeephavenParameter</see> to retrieve.</param>
        /// <value>The <see cref="DeephavenParameter">DeephavenParameter</see> with the specified name, or a null reference if the parameter is not found.</value>
        public new DeephavenParameter this[string parameterName]
        {
            get
            {
                var index = IndexOf(parameterName);

                if (index == -1)
                    throw new ArgumentException("Parameter not found");

                return _internalList[index];
            }
            set
            {
                var index = IndexOf(parameterName);

                if (index == -1)
                    throw new ArgumentException("Parameter not found");

                var oldValue = _internalList[index];

                if (value.ParameterName != oldValue.ParameterName)
                    InvalidateHashLookups();

                _internalList[index] = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="DeephavenParameter">DeephavenParameter</see> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the <see cref="DeephavenParameter">DeephavenParameter</see> to retrieve.</param>
        /// <value>The <see cref="DeephavenParameter">DeephavenParameter</see> at the specified index.</value>
        public new DeephavenParameter this[int index]
        {
            get => _internalList[index];
            set
            {
                var oldValue = _internalList[index];

                if (oldValue == value)
                    return;

                if (value.ParameterName != oldValue.ParameterName)
                    InvalidateHashLookups();

                _internalList[index] = value;
            }
        }

        /// <summary>
        /// Adds the specified <see cref="DeephavenParameter">DeephavenParameter</see> object to the <see cref="DeephavenParameterCollection">DeephavenParameterCollection</see>.
        /// </summary>
        /// <param name="value">The <see cref="DeephavenParameter">DeephavenParameter</see> to add to the collection.</param>
        /// <returns>The index of the new <see cref="DeephavenParameter">DeephavenParameter</see> object.</returns>
        public DeephavenParameter Add(DeephavenParameter value)
        {
            _internalList.Add(value);
            InvalidateHashLookups();
            return value;
        }

        /// <inheritdoc />
        void ICollection<DeephavenParameter>.Add(DeephavenParameter item) => Add(item);

        /// <summary>
        /// Adds a <see cref="DeephavenParameter">DeephavenParameter</see> to the <see cref="DeephavenParameterCollection">DeephavenParameterCollection</see> given the specified parameter name and value.
        /// </summary>
        /// <param name="parameterName">The name of the <see cref="DeephavenParameter">DeephavenParameter</see>.</param>
        /// <param name="value">The Value of the <see cref="DeephavenParameter">DeephavenParameter</see> to add to the collection.</param>
        /// <returns>The paramater that was added.</returns>
        public DeephavenParameter AddWithValue(string parameterName, object value)
            => Add(new DeephavenParameter(parameterName, value));

        #endregion

        #region IDataParameterCollection Member

        /// <inheritdoc />
        public override void RemoveAt(string parameterName)
        {
            if (parameterName == null)
                throw new ArgumentNullException(nameof(parameterName));

            RemoveAt(IndexOf(parameterName));
        }

        /// <inheritdoc />
        public override bool Contains(string parameterName)
        {
            if (parameterName == null)
                throw new ArgumentNullException(nameof(parameterName));

            return IndexOf(parameterName) != -1;
        }

        /// <inheritdoc />
        public override int IndexOf(string parameterName)
        {
            if (parameterName == null)
                return -1;

            if (parameterName.Length > 0 && (parameterName[0] == ':' || parameterName[0] == '@'))
                parameterName = parameterName.Remove(0, 1);

            // Using a dictionary is much faster for 5 or more items
            if (_internalList.Count >= 5)
            {
                if (_lookup == null)
                {
                    _lookup = new Dictionary<string, int>();
                    for (int i = 0; i < _internalList.Count; i++)
                    {
                        var item = _internalList[i];

                        // Store only the first of each distinct value
                        if (!_lookup.ContainsKey(item.BoundName))
                            _lookup.Add(item.BoundName, i);
                    }
                }

                // Try to access the case sensitive parameter name first
                if (_lookup.TryGetValue(parameterName, out var retIndex))
                    return retIndex;

                // Case sensitive lookup failed, generate a case insensitive lookup
                if (_lookupIgnoreCase == null)
                {
                    _lookupIgnoreCase = new Dictionary<string, int>(/*PGUtil.InvariantCaseIgnoringStringComparer*/);
                    for (var i = 0; i < _internalList.Count; i++)
                    {
                        var item = _internalList[i];

                        // Store only the first of each distinct value
                        if (!_lookupIgnoreCase.ContainsKey(item.BoundName))
                            _lookupIgnoreCase.Add(item.BoundName, i);
                    }
                }

                // Then try to access the case insensitive parameter name
                if (_lookupIgnoreCase.TryGetValue(parameterName, out retIndex))
                    return retIndex;

                return -1;
            }

            // First try a case-sensitive match
            for (var i = 0; i < _internalList.Count; i++)
                if (parameterName == _internalList[i].BoundName)
                    return i;

            // If not fond, try a case-insensitive match
            for (var i = 0; i < _internalList.Count; i++)
                if (string.Equals(parameterName, _internalList[i].BoundName, StringComparison.OrdinalIgnoreCase))
                    return i;

            return -1;
        }

        #endregion

        #region IList Member

        /// <inheritdoc />
        public override bool IsReadOnly => false;

        /// <summary>
        /// Removes the specified <see cref="DeephavenParameter">DeephavenParameter</see> from the collection using a specific index.
        /// </summary>
        /// <param name="index">The zero-based index of the parameter.</param>
        public override void RemoveAt(int index)
        {
            if (_internalList.Count - 1 < index)
                throw new IndexOutOfRangeException();
            Remove(_internalList[index]);
        }

        /// <inheritdoc />
        public override void Insert(int index, object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (!(value is DeephavenParameter param))
                throw new InvalidCastException($"{nameof(value)} must be a DeephavenParameter");

            _internalList.Insert(index, param);
            InvalidateHashLookups();
        }

        /// <summary>
        /// Removes the specified <see cref="DeephavenParameter">DeephavenParameter</see> from the collection.
        /// </summary>
        /// <param name="parameterName">The name of the <see cref="DeephavenParameter">DeephavenParameter</see> to remove from the collection.</param>
        public void Remove(string parameterName)
        {
            var index = IndexOf(parameterName);
            if (index < 0)
                throw new InvalidOperationException("No parameter with the specified name exists in the collection");
            RemoveAt(index);
        }

        /// <summary>
        /// Removes the specified <see cref="DeephavenParameter">DeephavenParameter</see> from the collection.
        /// </summary>
        /// <param name="value">The <see cref="DeephavenParameter">DeephavenParameter</see> to remove from the collection.</param>
        public override void Remove(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (!(value is DeephavenParameter param))
                throw new InvalidCastException($"{nameof(value)} must be a DeephavenParameter");

            Remove(param);
        }

        /// <inheritdoc />
        public override bool Contains(object value)
            => value is DeephavenParameter param && _internalList.Contains(param);

        /// <summary>
        /// Gets a value indicating whether a <see cref="DeephavenParameter">DeephavenParameter</see> with the specified parameter name exists in the collection.
        /// </summary>
        /// <param name="parameterName">The name of the <see cref="DeephavenParameter">DeephavenParameter</see> object to find.</param>
        /// <param name="parameter">A reference to the requested parameter is returned in this out param if it is found in the list.  This value is null if the parameter is not found.</param>
        /// <returns><b>true</b> if the collection contains the parameter and param will contain the parameter; otherwise, <b>false</b>.</returns>
        public bool TryGetValue(string parameterName, out DeephavenParameter parameter)
        {
            var index = IndexOf(parameterName);

            if (index != -1)
            {
                parameter = _internalList[index];
                return true;
            }
            parameter = null;
            return false;
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public override void Clear()
        {
            _internalList.Clear();
            InvalidateHashLookups();
        }

        /// <inheritdoc />
        public override int IndexOf(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (!(value is DeephavenParameter param))
                throw new InvalidCastException($"{nameof(value)} must be a DeephavenParameter");
            return _internalList.IndexOf(param);
        }

        /// <inheritdoc />
        public override int Add(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (!(value is DeephavenParameter param))
                throw new InvalidCastException($"{nameof(value)} must be a DeephavenParameter");
            Add(param);
            return Count - 1;
        }

        /// <inheritdoc />
        public override bool IsFixedSize => false;

        #endregion

        #region ICollection Member

        /// <inheritdoc />
        public override bool IsSynchronized => (_internalList as ICollection).IsSynchronized;

        /// <summary>
        /// Gets the number of <see cref="DeephavenParameter">DeephavenParameter</see> objects in the collection.
        /// </summary>
        /// <value>The number of <see cref="DeephavenParameter">DeephavenParameter</see> objects in the collection.</value>
        public override int Count => _internalList.Count;

        /// <inheritdoc />
        public override void CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            ((ICollection)_internalList).CopyTo(array, index);
        }

        /// <inheritdoc />
        bool ICollection<DeephavenParameter>.IsReadOnly => false;

        /// <inheritdoc />
        public override object SyncRoot => ((ICollection)_internalList).SyncRoot;

        #endregion

        #region IEnumerable Member

        IEnumerator<DeephavenParameter> IEnumerable<DeephavenParameter>.GetEnumerator()
            => _internalList.GetEnumerator();

        /// <inheritdoc />
        public override IEnumerator GetEnumerator() => _internalList.GetEnumerator();

        #endregion

        /// <inheritdoc />
        public override void AddRange(Array values)
        {
            foreach (DeephavenParameter parameter in values)
                Add(parameter);
        }

        /// <inheritdoc />
        protected override DbParameter GetParameter(string parameterName)
            => parameterName == null
                ? throw new ArgumentNullException(nameof(parameterName))
                : this[parameterName];

        /// <inheritdoc />
        protected override DbParameter GetParameter(int index)
            => this[index];

        /// <inheritdoc />
        protected override void SetParameter(string parameterName, DbParameter value)
        {
            if (parameterName == null)
                throw new ArgumentNullException(nameof(parameterName));
            this[parameterName] = (DeephavenParameter)value;
        }

        /// <inheritdoc />
        protected override void SetParameter(int index, DbParameter value)
            => this[index] = (DeephavenParameter)value;

        /// <summary>
        /// Report the offset within the collection of the given parameter.
        /// </summary>
        /// <param name="item">Parameter to find.</param>
        /// <returns>Index of the parameter, or -1 if the parameter is not present.</returns>
        public int IndexOf(DeephavenParameter item)
            => _internalList.IndexOf(item);

        /// <summary>
        /// Insert the specified parameter into the collection.
        /// </summary>
        /// <param name="index">Index of the existing parameter before which to insert the new one.</param>
        /// <param name="item">Parameter to insert.</param>
        public void Insert(int index, DeephavenParameter item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _internalList.Insert(index, item);
            InvalidateHashLookups();
        }

        /// <summary>
        /// Report whether the specified parameter is present in the collection.
        /// </summary>
        /// <param name="item">Parameter to find.</param>
        /// <returns>True if the parameter was found, otherwise false.</returns>
        public bool Contains(DeephavenParameter item) => _internalList.Contains(item);

        /// <summary>
        /// Remove the specified parameter from the collection.
        /// </summary>
        /// <param name="item">Parameter to remove.</param>
        /// <returns>True if the parameter was found and removed, otherwise false.</returns>
        public bool Remove(DeephavenParameter item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (_internalList.Remove(item))
            {
                InvalidateHashLookups();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Convert collection to a System.Array.
        /// </summary>
        /// <param name="array">Destination array.</param>
        /// <param name="arrayIndex">Starting index in destination array.</param>
        public void CopyTo(DeephavenParameter[] array, int arrayIndex)
            => _internalList.CopyTo(array, arrayIndex);

        /// <summary>
        /// Convert collection to a System.Array.
        /// </summary>
        /// <returns>DeephavenParameter[]</returns>
        public DeephavenParameter[] ToArray() => _internalList.ToArray();

        internal void CloneTo(DeephavenParameterCollection other)
        {
            other._internalList.Clear();
            foreach (var param in _internalList)
            {
                // deep copy since parameters are mutable
                var newParam = param.Clone();
                other._internalList.Add(param);
            }
            other._lookup = _lookup;
            other._lookupIgnoreCase = _lookupIgnoreCase;
        }

        internal bool HasOutputParameters
        {
            get
            {
                // we don't support output parameters
                return false;
            }
        }
    }
}
