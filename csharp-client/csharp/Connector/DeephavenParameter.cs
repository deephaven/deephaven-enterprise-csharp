/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Data;
using System.Data.Common;
using Deephaven.Connector.Internal.Parameters;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.Connector
{
    /// <summary>
    /// Represents a parameter to a <see cref="DeephavenCommand"/>.
    /// </summary>
    public class DeephavenParameter : DbParameter
    {
        private string _parameterName;
        private object _value;
        private DbType _dbType;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeephavenParameter"/> class.
        /// </summary>
        public DeephavenParameter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeephavenParameter"/> class with the given parameter name and value.
        /// The <see cref="DbType"/> is inferred from the object type of <code>value</code>.
        /// </summary>
        /// <param name="parameterName">The parameter name. Must be prefixed with "@".</param>
        /// <param name="value">The parameter value.</param>
        public DeephavenParameter(string parameterName, object value)
        {
            SetParameterName(parameterName);
            _value = value;
            _dbType = inferDbType(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeephavenParameter"/> class with the given parameter name and DbType.
        /// </summary>
        /// <param name="parameterName">The parameter name. Must be prefixed with "@".</param>
        /// <param name="dbType">The parameter <see cref="DbType"/>.</param>
        public DeephavenParameter(string parameterName, DbType dbType) : this(parameterName, dbType, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeephavenParameter"/> class with the given parameter name, DbType and value.
        /// The value provided should be consistent with the specified <see cref="DbType"/>.
        /// </summary>
        /// <param name="parameterName">The parameter name. Must be prefixed with "@"</param>
        /// <param name="dbType">The parameter <see cref="DbType"/>.</param>
        /// <param name="value">The parameter value.</param>
        public DeephavenParameter(string parameterName, DbType dbType, object value)
        {
            SetParameterName(parameterName);
            _dbType = dbType;
            _value = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="DbType"/> of the parameter.
        /// </summary>
        public override DbType DbType
        {
            get
            {
                return _dbType;
            }
            set
            {
                _dbType = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the parameter is input-only, output-only, bidirectional, or a stored procedure return value parameter.
        /// Only input-only parameters are supported.
        /// </summary>
        public override ParameterDirection Direction
        {
            get
            {
                return ParameterDirection.Input;
            }
            set
            {
                if (value != ParameterDirection.Input)
                    throw new ArgumentException("Unsupported ParameterDirection: " + value);
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the parameter accepts null values.
        /// All Deephaven values are nullable, may not be set to false.
        /// </summary>
        public override bool IsNullable
        {
            get
            {
                return true;
            }
            set
            {
                if (value == false)
                    throw new ArgumentException("Unsupported IsNullable: " + value);
            }
        }

        private void SetParameterName(string value)
        {
            if (value == null || value.Length == 0)
            {
                throw new ArgumentException("Parameter name may not be empty/null");
            }
            else if (value[0] == '@')
            {
                if (value.Length < 2)
                {
                    throw new ArgumentException("Invalid parameter name");
                }
                // add underscores to try to make it unique while preserving the original name for debugging
                BoundName = "__" + value.Substring(1);
                _parameterName = value;
            }
            else
            {
                throw new ArgumentException("Bound parameters must start with the \"@\" character.");
            }
        }

        /// <summary>
        /// Gets or sets the name of the <see cref="DbParameter"/>.
        /// </summary>
        public override string ParameterName
        {
            get => _parameterName;
            set
            {
                SetParameterName(value);
            }
        }

        // represents the name we use to bind the variable (parameter name w/o the optional "@" prefix)
        internal string BoundName { get; private set; } = string.Empty;

        internal object BoundValue
        {
            get
            {
                object boundValue = _value;
                if (_value != null)
                {
                    if (_dbType == DbType.Date)
                    {
                        if (_value is DateTime)
                        {
                            DateTime dateTime = (DateTime)_value;
                            boundValue = new LocalDate(dateTime.Year, (byte)dateTime.Month, (byte)dateTime.Day);
                        }
                        else if (!(_value is LocalDate))
                        {
                            throw new InvalidOperationException(string.Format("Unable to bind value of type {0} to Date", _value.GetType()));
                        }
                    }
                    if(_dbType == DbType.Time)
                    {
                        if (_value is DateTime)
                        {
                            DateTime dateTime = (DateTime)_value;
                            boundValue = new LocalTime(dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond * 1_000_000_000);
                        }
                        else if (!(_value is LocalTime))
                        {
                            throw new InvalidOperationException(string.Format("Unable to bind value of type {0} to Time", _value.GetType()));
                        }
                    }
                }
                return boundValue;
            }
        }

        private DbType inferDbType(object value)
        {
            if (value == null)
                throw new ArgumentException("Unable to infer DbType from null value");
            else if (value is sbyte)
                return DbType.SByte;
            else if (value is byte)
                return DbType.Byte;
            else if (value is int)
                return DbType.Int32;
            else if (value is long)
                return DbType.Int64;
            else if (value is short)
                return DbType.Int32;
            else if (value is string)
                return DbType.String;
            else if (value is float)
                return DbType.Single;
            else if (value is double)
                return DbType.Double;
            else if (value is decimal)
                return DbType.Decimal;
            else if (value is DateTime)
                return DbType.DateTime;
            else if (value is LocalDate)
                return DbType.Date;
            else if (value is LocalTime)
                return DbType.Time;
            else
                throw new ArgumentException("Unable to infer DbType from object of type: " + value.GetType());
        }

        /// <summary>
        /// Factory that creates the appropriate Deephaven parameter type based
        /// on the DbType. Each BindParameter subclass takes care of making
        /// sure the actual type of the value is compatible.
        /// Note that we have no mappings for CharParameter or BigInteger
        /// right now because there aren't any DbTypes that match these exactly.
        /// </summary>
        /// <returns></returns>
        internal BindParameter GetBindParameter()
        {
            BindParameter bindParameter;
            switch (_dbType)
            {
                case DbType.String:
                    bindParameter = StringParameter.Of(_value);
                    break;
                case DbType.Boolean:
                    bindParameter = BooleanParameter.Of(_value);
                    break;
                case DbType.Byte:
                    bindParameter = ByteParameter.Of(_value);
                    break;
                case DbType.SByte:
                    bindParameter = ByteParameter.Of(_value);
                    break;
                case DbType.Int16:
                    bindParameter = Int16Parameter.Of(_value);
                    break;
                case DbType.Int32:
                    bindParameter = Int32Parameter.Of(_value);
                    break;
                case DbType.Int64:
                    bindParameter = Int64Parameter.Of(_value);
                    break;
                case DbType.Single:
                    bindParameter = FloatParameter.Of(_value);
                    break;
                case DbType.Double:
                    bindParameter = DoubleParameter.Of(_value);
                    break;
                case DbType.Date:
                    bindParameter = LocalDateParameter.Of(_value);
                    break;
                case DbType.DateTime:
                    bindParameter = DBDateTimeParameter.Of(_value);
                    break;
                case DbType.Decimal:
                    bindParameter = BigDecimalParameter.Of(_value);
                    break;
                case DbType.Time:
                    bindParameter = LocalTimeParameter.Of(_value);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported DbType: " + _dbType);
            }
            return bindParameter;
        }

        /// <summary>
        /// Gets or sets the maximum size, in bytes, of the data within the column.
        /// Deephaven does not support sizes in parameters, this value is ignored.
        /// </summary>
        public override int Size { get; set; }

        /// <summary>
        /// Gets or sets the name of the source column mapped to the <see cref="DataSet"/> and used for loading or returning the Value.
        /// Deephaven does not support <see cref="DataSet"/>, this property is ignored.
        /// </summary>
        public override string SourceColumn { get; set; }

        /// <summary>
        /// Sets or gets a value which indicates whether the source column is nullable.
        /// This allows <see cref="DbCommandBuilder"/> to correctly generate Update statements for nullable columns.
        /// Deephaven does not support <see cref="DbCommandBuilder"/>, this property is ignored.
        /// </summary>
        public override bool SourceColumnNullMapping { get; set; }

        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        public override object Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        /// <summary>
        /// Create a copy of this parameter.
        /// </summary>
        /// <returns></returns>
        public DeephavenParameter Clone()
        {
            return new DeephavenParameter(_parameterName, _dbType, _value);
        }

        /// <summary>
        /// Resets the DbType property to its original settings.
        /// This examines the <see cref="Value"/> object and infers a <see cref="DbType"/> from that.
        /// </summary>
        public override void ResetDbType()
        {
            _dbType = inferDbType(_value);
        }
    }
}
