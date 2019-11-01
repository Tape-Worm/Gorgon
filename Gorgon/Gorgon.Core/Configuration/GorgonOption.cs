#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: August 16, 2016 4:02:49 PM
// 
#endregion

using System;
using Gorgon.Core;

namespace Gorgon.Configuration
{
    /// <summary>
    /// An option to be stored in a <see cref="IGorgonOptionBag"/>.
    /// </summary>
    public class GorgonOption
        : GorgonNamedObject, IGorgonOption
    {
        #region Variables.
        // The value stored in this option.
        private object _value;
        // The default value stored in this option.
        private readonly object _defaultValue;
        // The minimum allowed value for this option.
        private readonly object _minValue;
        // The maximum allowed value for this option.
        private readonly object _maxValue;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the type of data stored in the option.
        /// </summary>
        public Type Type
        {
            get;
        }

        /// <summary>
        /// Property to return text to display regarding this option.
        /// </summary>
        /// <remarks>
        /// This is pulled from the first line of the <see cref="Description"/>.
        /// </remarks>
        public string Text
        {
            get;
        }

        /// <summary>
        /// Property to return the friendly description of this option.
        /// </summary>
        public string Description
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the default value for this option.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <returns>The value, strongly typed.</returns>
        public T GetDefaultValue<T>()
        {
            Type type = typeof(T);

            return typeof(T) != type ? (T)Convert.ChangeType(_defaultValue, type) : (T)_defaultValue;
        }

        /// <summary>
        /// Function to retrieve the minimum allowed value for this option.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <returns>The value, strongly typed.</returns>
        public T GetMinValue<T>()
        {
            Type type = typeof(T);

            return typeof(T) != type ? (T)Convert.ChangeType(_minValue, type) : (T)_minValue;
        }

        /// <summary>
        /// Function to retrieve the maximum allowed value for this option.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <returns>The value, strongly typed.</returns>
        public T GetMaxValue<T>()
        {
            Type type = typeof(T);

            return typeof(T) != type ? (T)Convert.ChangeType(_maxValue, type) : (T)_maxValue;
        }

        /// <summary>
        /// Function to retrieve the value stored in this option.
        /// </summary>
        /// <typeparam name="T">The type for the value.</typeparam>
        /// <returns>The value, strongly typed.</returns>
        public T GetValue<T>()
        {
            Type type = typeof(T);

            return (typeof(T) != Type) && (_value is IConvertible) ? (T)Convert.ChangeType(_value, type) : (T)_value;
        }

        /// <summary>
        /// Function to assign a value for the option.
        /// </summary>
        /// <typeparam name="T">The type parmeter for the value.</typeparam>
        /// <param name="value">The value to assign.</param>
        public void SetValue<T>(T value)
        {
            object newValue = value;

            // Convert to the type used by this option.
            if ((typeof(T) != Type) && (value is IConvertible))
            {
                newValue = Convert.ChangeType(value, Type);
            }

            // If we're using a numeric, or date/time value, ensure that it's within the min/max range.
            switch (Type.GetTypeCode(Type))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Double:
                case TypeCode.Single:
                case TypeCode.Decimal:
                    {
                        // Convert to a decimal first, just because it's the largest primitive number format.
                        decimal convertedValue = (decimal)Convert.ChangeType(newValue, typeof(decimal));

                        if (_minValue != null)
                        {
                            decimal minValue = (decimal)Convert.ChangeType(_minValue, typeof(decimal));

                            if (convertedValue < minValue)
                            {
                                convertedValue = minValue;
                            }
                        }

                        if (_maxValue != null)
                        {
                            decimal maxValue = (decimal)Convert.ChangeType(_maxValue, typeof(decimal));

                            if (convertedValue > maxValue)
                            {
                                convertedValue = maxValue;
                            }
                        }

                        newValue = Convert.ChangeType(convertedValue, Type);
                    }
                    break;
                case TypeCode.DateTime:
                    {
                        var convertedValue = (DateTime)Convert.ChangeType(newValue, typeof(DateTime));

                        if (_minValue != null)
                        {
                            var minValue = (DateTime)Convert.ChangeType(_minValue, typeof(DateTime));

                            if (convertedValue < minValue)
                            {
                                convertedValue = minValue;
                            }
                        }

                        if (_maxValue != null)
                        {
                            var maxValue = (DateTime)Convert.ChangeType(_maxValue, typeof(DateTime));
                            if (convertedValue > maxValue)
                            {
                                convertedValue = maxValue;
                            }
                        }

                        newValue = Convert.ChangeType(convertedValue, Type);
                    }
                    break;
            }

            _value = newValue;
        }

        /// <summary>
        /// Function to create an option that stores a byte value.
        /// </summary>
        /// <param name="name">The name of the option.</param>
        /// <param name="defaultValue">The default value for the option.</param>
        /// <param name="description">[Optional] The friendly description for this option.</param>
        /// <param name="minValue">[Optional] The minimum value for the option.</param>
        /// <param name="maxValue">[Optional] The maximum value for the option.</param>
        /// <returns>A new <see cref="IGorgonOption"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> is empty.</exception>
        /// <remarks>
        /// <para>
        /// The <see cref="Text"/> for this option is derived from the <paramref name="description"/> by using the first line of text (ended by a new line character). If the <paramref name="description"/> 
        /// is a single line, then the <see cref="Text"/> and <see cref="Description"/> fields will be the same.
        /// </para>
        /// </remarks>
        public static IGorgonOption CreateByteOption(string name, byte defaultValue, string description = null, byte? minValue = null, byte? maxValue = null)
        {
            if ((minValue != null) && (defaultValue < minValue.Value))
            {
                defaultValue = minValue.Value;
            }

            if ((maxValue != null) && (defaultValue > maxValue.Value))
            {
                defaultValue = maxValue.Value;
            }

            return new GorgonOption(name,
                                    typeof(byte),
                                    defaultValue,
                                    minValue != null ? (object)minValue.Value : null,
                                    maxValue != null ? (object)maxValue.Value : null,
                                    description);
        }

        /// <summary>
        /// Function to create an option that stores a signed byte value.
        /// </summary>
        /// <param name="name">The name of the option.</param>
        /// <param name="defaultValue">The default value for the option.</param>
        /// <param name="description">[Optional] The friendly description for this option.</param>
        /// <param name="minValue">[Optional] The minimum value for the option.</param>
        /// <param name="maxValue">[Optional] The maximum value for the option.</param>
        /// <returns>A new <see cref="IGorgonOption"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> is empty.</exception>
        /// <remarks>
        /// <para>
        /// The <see cref="Text"/> for this option is derived from the <paramref name="description"/> by using the first line of text (ended by a new line character). If the <paramref name="description"/> 
        /// is a single line, then the <see cref="Text"/> and <see cref="Description"/> fields will be the same.
        /// </para>
        /// </remarks>
        public static IGorgonOption CreateSByteOption(string name, sbyte defaultValue, string description = null, sbyte? minValue = null, sbyte? maxValue = null)
        {
            if ((minValue != null) && (defaultValue < minValue.Value))
            {
                defaultValue = minValue.Value;
            }

            if ((maxValue != null) && (defaultValue > maxValue.Value))
            {
                defaultValue = maxValue.Value;
            }

            return new GorgonOption(name,
                                    typeof(sbyte),
                                    defaultValue,
                                    minValue != null ? (object)minValue.Value : null,
                                    maxValue != null ? (object)maxValue.Value : null,
                                    description);
        }

        /// <summary>
        /// Function to create an option that stores a signed byte value.
        /// </summary>
        /// <param name="name">The name of the option.</param>
        /// <param name="defaultValue">The default value for the option.</param>
        /// <param name="description">[Optional] The friendly description for this option.</param>
        /// <param name="minValue">[Optional] The minimum value for the option.</param>
        /// <param name="maxValue">[Optional] The maximum value for the option.</param>
        /// <returns>A new <see cref="IGorgonOption"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> is empty.</exception>
        /// <remarks>
        /// <para>
        /// The <see cref="Text"/> for this option is derived from the <paramref name="description"/> by using the first line of text (ended by a new line character). If the <paramref name="description"/> 
        /// is a single line, then the <see cref="Text"/> and <see cref="Description"/> fields will be the same.
        /// </para>
        /// </remarks>
        public static IGorgonOption CreateInt16Option(string name, short defaultValue, string description = null, short? minValue = null, short? maxValue = null)
        {
            if ((minValue != null) && (defaultValue < minValue.Value))
            {
                defaultValue = minValue.Value;
            }

            if ((maxValue != null) && (defaultValue > maxValue.Value))
            {
                defaultValue = maxValue.Value;
            }

            return new GorgonOption(name,
                                    typeof(short),
                                    defaultValue,
                                    minValue != null ? (object)minValue.Value : null,
                                    maxValue != null ? (object)maxValue.Value : null,
                                    description);
        }

        /// <summary>
        /// Function to create an option that stores a signed byte value.
        /// </summary>
        /// <param name="name">The name of the option.</param>
        /// <param name="defaultValue">The default value for the option.</param>
        /// <param name="description">[Optional] The friendly description for this option.</param>
        /// <param name="minValue">[Optional] The minimum value for the option.</param>
        /// <param name="maxValue">[Optional] The maximum value for the option.</param>
        /// <returns>A new <see cref="IGorgonOption"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> is empty.</exception>
        /// <remarks>
        /// <para>
        /// The <see cref="Text"/> for this option is derived from the <paramref name="description"/> by using the first line of text (ended by a new line character). If the <paramref name="description"/> 
        /// is a single line, then the <see cref="Text"/> and <see cref="Description"/> fields will be the same.
        /// </para>
        /// </remarks>
        public static IGorgonOption CreateUInt16Option(string name, ushort defaultValue, string description = null, ushort? minValue = null, ushort? maxValue = null)
        {
            if ((minValue != null) && (defaultValue < minValue.Value))
            {
                defaultValue = minValue.Value;
            }

            if ((maxValue != null) && (defaultValue > maxValue.Value))
            {
                defaultValue = maxValue.Value;
            }

            return new GorgonOption(name,
                                    typeof(ushort),
                                    defaultValue,
                                    minValue != null ? (object)minValue.Value : null,
                                    maxValue != null ? (object)maxValue.Value : null,
                                    description);
        }

        /// <summary>
        /// Function to create an option that stores a signed byte value.
        /// </summary>
        /// <param name="name">The name of the option.</param>
        /// <param name="defaultValue">The default value for the option.</param>
        /// <param name="description">[Optional] The friendly description for this option.</param>
        /// <param name="minValue">[Optional] The minimum value for the option.</param>
        /// <param name="maxValue">[Optional] The maximum value for the option.</param>
        /// <returns>A new <see cref="IGorgonOption"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> is empty.</exception>
        /// <remarks>
        /// <para>
        /// The <see cref="Text"/> for this option is derived from the <paramref name="description"/> by using the first line of text (ended by a new line character). If the <paramref name="description"/> 
        /// is a single line, then the <see cref="Text"/> and <see cref="Description"/> fields will be the same.
        /// </para>
        /// </remarks>
        public static IGorgonOption CreateInt32Option(string name, int defaultValue, string description, int? minValue = null, int? maxValue = null)
        {
            if ((minValue != null) && (defaultValue < minValue.Value))
            {
                defaultValue = minValue.Value;
            }

            if ((maxValue != null) && (defaultValue > maxValue.Value))
            {
                defaultValue = maxValue.Value;
            }

            return new GorgonOption(name,
                                    typeof(int),
                                    defaultValue,
                                    minValue != null ? (object)minValue.Value : null,
                                    maxValue != null ? (object)maxValue.Value : null,
                                    description);
        }

        /// <summary>
        /// Function to create an option that stores a signed byte value.
        /// </summary>
        /// <param name="name">The name of the option.</param>
        /// <param name="defaultValue">The default value for the option.</param>
        /// <param name="description">[Optional] The friendly description for this option.</param>
        /// <param name="minValue">[Optional] The minimum value for the option.</param>
        /// <param name="maxValue">[Optional] The maximum value for the option.</param>
        /// <returns>A new <see cref="IGorgonOption"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> is empty.</exception>
        /// <remarks>
        /// <para>
        /// The <see cref="Text"/> for this option is derived from the <paramref name="description"/> by using the first line of text (ended by a new line character). If the <paramref name="description"/> 
        /// is a single line, then the <see cref="Text"/> and <see cref="Description"/> fields will be the same.
        /// </para>
        /// </remarks>
        public static IGorgonOption CreateUInt32Option(string name, uint defaultValue, string description = null, uint? minValue = null, uint? maxValue = null)
        {
            if ((minValue != null) && (defaultValue < minValue.Value))
            {
                defaultValue = minValue.Value;
            }

            if ((maxValue != null) && (defaultValue > maxValue.Value))
            {
                defaultValue = maxValue.Value;
            }

            return new GorgonOption(name,
                                    typeof(uint),
                                    defaultValue,
                                    minValue != null ? (object)minValue.Value : null,
                                    maxValue != null ? (object)maxValue.Value : null,
                                    description);
        }

        /// <summary>
        /// Function to create an option that stores a signed byte value.
        /// </summary>
        /// <param name="name">The name of the option.</param>
        /// <param name="defaultValue">The default value for the option.</param>
        /// <param name="description">[Optional] The friendly description for this option.</param>
        /// <param name="minValue">[Optional] The minimum value for the option.</param>
        /// <param name="maxValue">[Optional] The maximum value for the option.</param>
        /// <returns>A new <see cref="IGorgonOption"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> is empty.</exception>
        /// <remarks>
        /// <para>
        /// The <see cref="Text"/> for this option is derived from the <paramref name="description"/> by using the first line of text (ended by a new line character). If the <paramref name="description"/> 
        /// is a single line, then the <see cref="Text"/> and <see cref="Description"/> fields will be the same.
        /// </para>
        /// </remarks>
        public static IGorgonOption CreateInt64Option(string name, long defaultValue, string description = null, long? minValue = null, long? maxValue = null)
        {
            if ((minValue != null) && (defaultValue < minValue.Value))
            {
                defaultValue = minValue.Value;
            }

            if ((maxValue != null) && (defaultValue > maxValue.Value))
            {
                defaultValue = maxValue.Value;
            }

            return new GorgonOption(name,
                                    typeof(long),
                                    defaultValue,
                                    minValue != null ? (object)minValue.Value : null,
                                    maxValue != null ? (object)maxValue.Value : null,
                                    description);
        }

        /// <summary>
        /// Function to create an option that stores a signed byte value.
        /// </summary>
        /// <param name="name">The name of the option.</param>
        /// <param name="defaultValue">The default value for the option.</param>
        /// <param name="description">[Optional] The friendly description for this option.</param>
        /// <param name="minValue">[Optional] The minimum value for the option.</param>
        /// <param name="maxValue">[Optional] The maximum value for the option.</param>
        /// <returns>A new <see cref="IGorgonOption"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> is empty.</exception>
        /// <remarks>
        /// <para>
        /// The <see cref="Text"/> for this option is derived from the <paramref name="description"/> by using the first line of text (ended by a new line character). If the <paramref name="description"/> 
        /// is a single line, then the <see cref="Text"/> and <see cref="Description"/> fields will be the same.
        /// </para>
        /// </remarks>
        public static IGorgonOption CreateUInt64Option(string name, ulong defaultValue, string description = null, ulong? minValue = null, ulong? maxValue = null)
        {
            if ((minValue != null) && (defaultValue < minValue.Value))
            {
                defaultValue = minValue.Value;
            }

            if ((maxValue != null) && (defaultValue > maxValue.Value))
            {
                defaultValue = maxValue.Value;
            }

            return new GorgonOption(name,
                                    typeof(ulong),
                                    defaultValue,
                                    minValue != null ? (object)minValue.Value : null,
                                    maxValue != null ? (object)maxValue.Value : null,
                                    description);
        }

        /// <summary>
        /// Function to create an option that stores a signed byte value.
        /// </summary>
        /// <param name="name">The name of the option.</param>
        /// <param name="defaultValue">The default value for the option.</param>
        /// <param name="description">[Optional] The friendly description for this option.</param>
        /// <param name="minValue">[Optional] The minimum value for the option.</param>
        /// <param name="maxValue">[Optional] The maximum value for the option.</param>
        /// <returns>A new <see cref="IGorgonOption"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> is empty.</exception>
        /// <remarks>
        /// <para>
        /// The <see cref="Text"/> for this option is derived from the <paramref name="description"/> by using the first line of text (ended by a new line character). If the <paramref name="description"/> 
        /// is a single line, then the <see cref="Text"/> and <see cref="Description"/> fields will be the same.
        /// </para>
        /// </remarks>
        public static IGorgonOption CreateDoubleOption(string name, double defaultValue, string description = null, double? minValue = null, double? maxValue = null)
        {
            if ((minValue != null) && (defaultValue < minValue.Value))
            {
                defaultValue = minValue.Value;
            }

            if ((maxValue != null) && (defaultValue > maxValue.Value))
            {
                defaultValue = maxValue.Value;
            }

            return new GorgonOption(name, typeof(double), defaultValue, minValue != null ? (object)minValue.Value : null, maxValue != null ? (object)maxValue.Value : null, description);
        }

        /// <summary>
        /// Function to create an option that stores a signed byte value.
        /// </summary>
        /// <param name="name">The name of the option.</param>
        /// <param name="defaultValue">The default value for the option.</param>
        /// <param name="description">[Optional] The friendly description for this option.</param>
        /// <param name="minValue">[Optional] The minimum value for the option.</param>
        /// <param name="maxValue">[Optional] The maximum value for the option.</param>
        /// <returns>A new <see cref="IGorgonOption"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> is empty.</exception>
        /// <remarks>
        /// <para>
        /// The <see cref="Text"/> for this option is derived from the <paramref name="description"/> by using the first line of text (ended by a new line character). If the <paramref name="description"/> 
        /// is a single line, then the <see cref="Text"/> and <see cref="Description"/> fields will be the same.
        /// </para>
        /// </remarks>
        public static IGorgonOption CreateSingleOption(string name, float defaultValue, string description = null, float? minValue = null, float? maxValue = null)
        {
            if ((minValue != null) && (defaultValue < minValue.Value))
            {
                defaultValue = minValue.Value;
            }

            if ((maxValue != null) && (defaultValue > maxValue.Value))
            {
                defaultValue = maxValue.Value;
            }

            return new GorgonOption(name,
                                    typeof(float),
                                    defaultValue,
                                    minValue != null ? (object)minValue.Value : null,
                                    maxValue != null ? (object)maxValue.Value : null,
                                    description);
        }

        /// <summary>
        /// Function to create an option that stores a signed byte value.
        /// </summary>
        /// <param name="name">The name of the option.</param>
        /// <param name="defaultValue">The default value for the option.</param>
        /// <param name="description">[Optional] The friendly description for this option.</param>
        /// <param name="minValue">[Optional] The minimum value for the option.</param>
        /// <param name="maxValue">[Optional] The maximum value for the option.</param>
        /// <returns>A new <see cref="IGorgonOption"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> is empty.</exception>
        /// <remarks>
        /// <para>
        /// The <see cref="Text"/> for this option is derived from the <paramref name="description"/> by using the first line of text (ended by a new line character). If the <paramref name="description"/> 
        /// is a single line, then the <see cref="Text"/> and <see cref="Description"/> fields will be the same.
        /// </para>
        /// </remarks>
        public static IGorgonOption CreateDecimalOption(string name, decimal defaultValue, string description = null, decimal? minValue = null, decimal? maxValue = null)
        {
            if ((minValue != null) && (defaultValue < minValue.Value))
            {
                defaultValue = minValue.Value;
            }

            if ((maxValue != null) && (defaultValue > maxValue.Value))
            {
                defaultValue = maxValue.Value;
            }

            return new GorgonOption(name,
                                    typeof(decimal),
                                    defaultValue,
                                    minValue != null ? (object)minValue.Value : null,
                                    maxValue != null ? (object)maxValue.Value : null,
                                    description);
        }

        /// <summary>
        /// Function to create an option that stores a signed byte value.
        /// </summary>
        /// <param name="name">The name of the option.</param>
        /// <param name="defaultValue">The default value for the option.</param>
        /// <param name="minValue">[Optional] The minimum value for the option.</param>
        /// <param name="maxValue">[Optional] The maximum value for the option.</param>
        /// <param name="description">[Optional] The friendly description for this option.</param>
        /// <returns>A new <see cref="IGorgonOption"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> is empty.</exception>
        /// <remarks>
        /// <para>
        /// The <see cref="Text"/> for this option is derived from the <paramref name="description"/> by using the first line of text (ended by a new line character). If the <paramref name="description"/> 
        /// is a single line, then the <see cref="Text"/> and <see cref="Description"/> fields will be the same.
        /// </para>
        /// </remarks>
        public static IGorgonOption CreateDateTimeOption(string name, DateTime defaultValue, string description = null, DateTime? minValue = null, DateTime? maxValue = null)
        {
            if ((minValue != null) && (defaultValue < minValue.Value))
            {
                defaultValue = minValue.Value;
            }

            if ((maxValue != null) && (defaultValue > maxValue.Value))
            {
                defaultValue = maxValue.Value;
            }

            return new GorgonOption(name, typeof(decimal), defaultValue, minValue != null ? (object)minValue.Value : null, maxValue != null ? (object)maxValue.Value : null, description);
        }

        /// <summary>
        /// Function to create an option that stores a signed byte value.
        /// </summary>
        /// <typeparam name="T">The type of value to store.</typeparam>
        /// <param name="name">The name of the option.</param>
        /// <param name="defaultValue">[Optional] The default value for the option.</param>
        /// <param name="description">[Optional] The friendly description for this option.</param>
        /// <returns>A new <see cref="IGorgonOption"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> is empty.</exception>
        /// <remarks>
        /// <para>
        /// The <see cref="Text"/> for this option is derived from the <paramref name="description"/> by using the first line of text (ended by a new line character). If the <paramref name="description"/> 
        /// is a single line, then the <see cref="Text"/> and <see cref="Description"/> fields will be the same.
        /// </para>
        /// <para>
        /// If the <paramref name="defaultValue"/> is omitted, then the default value for the type is used.
        /// </para>
        /// </remarks>
        public static IGorgonOption CreateOption<T>(string name, T defaultValue = default, string description = null) => new GorgonOption(name, typeof(T), defaultValue, null, null, description);

        /// <summary>
        /// Function to create an option that stores a signed byte value.
        /// </summary>
        /// <typeparam name="T">The type of value to store.</typeparam>
        /// <param name="name">The name of the option.</param>
        /// <param name="value">The initial value to assign.</param>
        /// <param name="defaultValue">The default value for the option.</param>
        /// <param name="description">[Optional] The friendly description for this option.</param>
        /// <returns>A new <see cref="IGorgonOption"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> is empty.</exception>
        /// <remarks>
        /// <para>
        /// The <see cref="Text"/> for this option is derived from the <paramref name="description"/> by using the first line of text (ended by a new line character). If the <paramref name="description"/> 
        /// is a single line, then the <see cref="Text"/> and <see cref="Description"/> fields will be the same.
        /// </para>
        /// </remarks>
        public static IGorgonOption CreateOption<T>(string name, T value, T defaultValue, string description = null)
        {
            IGorgonOption result = new GorgonOption(name, typeof(T), defaultValue, null, null, description);
            result.SetValue(value);
            return result;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonNamedObject" /> class.
        /// </summary>
        /// <param name="name">The name of this object.</param>
        /// <param name="type">The type of data for the value stored in this option.</param>
        /// <param name="defaultValue">The default value for this option.</param>
        /// <param name="minValue">The minimum allowed value for this option.</param>
        /// <param name="maxValue">The maximum allowed value for this option.</param>
        /// <param name="description">The description for this option.</param>
        private GorgonOption(string name, Type type, object defaultValue, object minValue, object maxValue, string description)
            : base(name)
        {
            Type = type;
            _defaultValue = defaultValue;
            _value = _defaultValue;
            _minValue = minValue;
            _maxValue = maxValue;
            Description = description ?? string.Empty;

            // Extract the text from the description.
            int newLineIndex = Description.IndexOf('\n');

            if ((newLineIndex != -1) && (newLineIndex != Description.Length - 1))
            {
                Text = Description.Substring(newLineIndex + 1);
            }
            else
            {
                Text = Description;
            }
        }
        #endregion
    }
}
