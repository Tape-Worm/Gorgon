
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 16, 2016 4:02:49 PM
// 

using System;
using Gorgon.Core;

namespace Gorgon.Configuration;

/// <summary>
/// Functions to create the options stored in an <see cref="IGorgonOptionBag"/>.
/// </summary>
/// <seealso cref="GorgonOption{T}"/>
/// <seealso cref="GorgonRangedOption{T}"/>
public static class GorgonOption
{
    /// <summary>
    /// Function to create an option that stores a <see cref="byte"/> value.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <param name="defaultValue">The default value for the option.</param>
    /// <param name="description">[Optional] The friendly description for the option.</param>
    /// <param name="minValue">[Optional] The smallest value allowed for the option.</param>
    /// <param name="maxValue">[Optional] The largest value allowed for the option.</param>
    /// <returns>A new <see cref="GorgonRangedOption{T}"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// A value assigned to the option is clamped to the <paramref name="minValue"/> and the <paramref name="maxValue"/>, and this includes the <paramref name="defaultValue"/>. The 
    /// <see cref="IGorgonOption.Text"/> for the option is the first line of the <paramref name="description"/>.
    /// </para>
    /// <para>
    /// The default values are an empty string for the <paramref name="description"/>, and no limit for the <paramref name="minValue"/> and the <paramref name="maxValue"/>.
    /// </para>
    /// </remarks>
    public static GorgonRangedOption<byte> CreateByteOption(string name, byte defaultValue, string? description = null, byte? minValue = null, byte? maxValue = null) =>
        new(name, defaultValue, description, minValue, maxValue);

    /// <summary>
    /// Function to create an option that stores a <see cref="sbyte"/> value.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <param name="defaultValue">The default value for the option.</param>
    /// <param name="description">[Optional] The friendly description for the option.</param>
    /// <param name="minValue">[Optional] The smallest value allowed for the option.</param>
    /// <param name="maxValue">[Optional] The largest value allowed for the option.</param>
    /// <returns>A new <see cref="GorgonRangedOption{T}"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// A value assigned to the option is clamped to the <paramref name="minValue"/> and the <paramref name="maxValue"/>, and this includes the <paramref name="defaultValue"/>. The 
    /// <see cref="IGorgonOption.Text"/> for the option is the first line of the <paramref name="description"/>.
    /// </para>
    /// <para>
    /// The default values are an empty string for the <paramref name="description"/>, and no limit for the <paramref name="minValue"/> and the <paramref name="maxValue"/>.
    /// </para>
    /// </remarks>
    public static GorgonRangedOption<sbyte> CreateSByteOption(string name, sbyte defaultValue, string? description = null, sbyte? minValue = null, sbyte? maxValue = null) =>
        new(name, defaultValue, description, minValue, maxValue);

    /// <summary>
    /// Function to create an option that stores a <see cref="short"/> value.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <param name="defaultValue">The default value for the option.</param>
    /// <param name="description">[Optional] The friendly description for the option.</param>
    /// <param name="minValue">[Optional] The smallest value allowed for the option.</param>
    /// <param name="maxValue">[Optional] The largest value allowed for the option.</param>
    /// <returns>A new <see cref="GorgonRangedOption{T}"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// A value assigned to the option is clamped to the <paramref name="minValue"/> and the <paramref name="maxValue"/>, and this includes the <paramref name="defaultValue"/>. The 
    /// <see cref="IGorgonOption.Text"/> for the option is the first line of the <paramref name="description"/>.
    /// </para>
    /// <para>
    /// The default values are an empty string for the <paramref name="description"/>, and no limit for the <paramref name="minValue"/> and the <paramref name="maxValue"/>.
    /// </para>
    /// </remarks>
    public static GorgonRangedOption<short> CreateInt16Option(string name, short defaultValue, string? description = null, short? minValue = null, short? maxValue = null) =>
        new(name, defaultValue, description, minValue, maxValue);

    /// <summary>
    /// Function to create an option that stores a <see cref="ushort"/> value.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <param name="defaultValue">The default value for the option.</param>
    /// <param name="description">[Optional] The friendly description for the option.</param>
    /// <param name="minValue">[Optional] The smallest value allowed for the option.</param>
    /// <param name="maxValue">[Optional] The largest value allowed for the option.</param>
    /// <returns>A new <see cref="GorgonRangedOption{T}"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// A value assigned to the option is clamped to the <paramref name="minValue"/> and the <paramref name="maxValue"/>, and this includes the <paramref name="defaultValue"/>. The 
    /// <see cref="IGorgonOption.Text"/> for the option is the first line of the <paramref name="description"/>.
    /// </para>
    /// <para>
    /// The default values are an empty string for the <paramref name="description"/>, and no limit for the <paramref name="minValue"/> and the <paramref name="maxValue"/>.
    /// </para>
    /// </remarks>
    public static GorgonRangedOption<ushort> CreateUInt16Option(string name, ushort defaultValue, string? description = null, ushort? minValue = null, ushort? maxValue = null) =>
        new(name, defaultValue, description, minValue, maxValue);

    /// <summary>
    /// Function to create an option that stores a <see cref="int"/> value.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <param name="defaultValue">The default value for the option.</param>
    /// <param name="description">[Optional] The friendly description for the option.</param>
    /// <param name="minValue">[Optional] The smallest value allowed for the option.</param>
    /// <param name="maxValue">[Optional] The largest value allowed for the option.</param>
    /// <returns>A new <see cref="GorgonRangedOption{T}"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// A value assigned to the option is clamped to the <paramref name="minValue"/> and the <paramref name="maxValue"/>, and this includes the <paramref name="defaultValue"/>. The 
    /// <see cref="IGorgonOption.Text"/> for the option is the first line of the <paramref name="description"/>.
    /// </para>
    /// <para>
    /// The default values are an empty string for the <paramref name="description"/>, and no limit for the <paramref name="minValue"/> and the <paramref name="maxValue"/>.
    /// </para>
    /// </remarks>
    public static GorgonRangedOption<int> CreateInt32Option(string name, int defaultValue, string? description = null, int? minValue = null, int? maxValue = null) =>
        new(name, defaultValue, description, minValue, maxValue);

    /// <summary>
    /// Function to create an option that stores a <see cref="uint"/> value.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <param name="defaultValue">The default value for the option.</param>
    /// <param name="description">[Optional] The friendly description for the option.</param>
    /// <param name="minValue">[Optional] The smallest value allowed for the option.</param>
    /// <param name="maxValue">[Optional] The largest value allowed for the option.</param>
    /// <returns>A new <see cref="GorgonRangedOption{T}"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// A value assigned to the option is clamped to the <paramref name="minValue"/> and the <paramref name="maxValue"/>, and this includes the <paramref name="defaultValue"/>. The 
    /// <see cref="IGorgonOption.Text"/> for the option is the first line of the <paramref name="description"/>.
    /// </para>
    /// <para>
    /// The default values are an empty string for the <paramref name="description"/>, and no limit for the <paramref name="minValue"/> and the <paramref name="maxValue"/>.
    /// </para>
    /// </remarks>
    public static GorgonRangedOption<uint> CreateUInt32Option(string name, uint defaultValue, string? description = null, uint? minValue = null, uint? maxValue = null) =>
        new(name, defaultValue, description, minValue, maxValue);

    /// <summary>
    /// Function to create an option that stores a <see cref="long"/> value.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <param name="defaultValue">The default value for the option.</param>
    /// <param name="description">[Optional] The friendly description for the option.</param>
    /// <param name="minValue">[Optional] The smallest value allowed for the option.</param>
    /// <param name="maxValue">[Optional] The largest value allowed for the option.</param>
    /// <returns>A new <see cref="GorgonRangedOption{T}"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// A value assigned to the option is clamped to the <paramref name="minValue"/> and the <paramref name="maxValue"/>, and this includes the <paramref name="defaultValue"/>. The 
    /// <see cref="IGorgonOption.Text"/> for the option is the first line of the <paramref name="description"/>.
    /// </para>
    /// <para>
    /// The default values are an empty string for the <paramref name="description"/>, and no limit for the <paramref name="minValue"/> and the <paramref name="maxValue"/>.
    /// </para>
    /// </remarks>
    public static GorgonRangedOption<long> CreateInt64Option(string name, long defaultValue, string? description = null, long? minValue = null, long? maxValue = null) =>
        new(name, defaultValue, description, minValue, maxValue);

    /// <summary>
    /// Function to create an option that stores a <see cref="ulong"/> value.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <param name="defaultValue">The default value for the option.</param>
    /// <param name="description">[Optional] The friendly description for the option.</param>
    /// <param name="minValue">[Optional] The smallest value allowed for the option.</param>
    /// <param name="maxValue">[Optional] The largest value allowed for the option.</param>
    /// <returns>A new <see cref="GorgonRangedOption{T}"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// A value assigned to the option is clamped to the <paramref name="minValue"/> and the <paramref name="maxValue"/>, and this includes the <paramref name="defaultValue"/>. The 
    /// <see cref="IGorgonOption.Text"/> for the option is the first line of the <paramref name="description"/>.
    /// </para>
    /// <para>
    /// The default values are an empty string for the <paramref name="description"/>, and no limit for the <paramref name="minValue"/> and the <paramref name="maxValue"/>.
    /// </para>
    /// </remarks>
    public static GorgonRangedOption<ulong> CreateUInt64Option(string name, ulong defaultValue, string? description = null, ulong? minValue = null, ulong? maxValue = null) =>
        new(name, defaultValue, description, minValue, maxValue);

    /// <summary>
    /// Function to create an option that stores a <see cref="float"/> value.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <param name="defaultValue">The default value for the option.</param>
    /// <param name="description">[Optional] The friendly description for the option.</param>
    /// <param name="minValue">[Optional] The smallest value allowed for the option.</param>
    /// <param name="maxValue">[Optional] The largest value allowed for the option.</param>
    /// <returns>A new <see cref="GorgonRangedOption{T}"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// A value assigned to the option is clamped to the <paramref name="minValue"/> and the <paramref name="maxValue"/>, and this includes the <paramref name="defaultValue"/>. The 
    /// <see cref="IGorgonOption.Text"/> for the option is the first line of the <paramref name="description"/>.
    /// </para>
    /// <para>
    /// The default values are an empty string for the <paramref name="description"/>, and no limit for the <paramref name="minValue"/> and the <paramref name="maxValue"/>.
    /// </para>
    /// </remarks>
    public static GorgonRangedOption<float> CreateSingleOption(string name, float defaultValue, string? description = null, float? minValue = null, float? maxValue = null) =>
        new(name, defaultValue, description, minValue, maxValue);

    /// <summary>
    /// Function to create an option that stores a <see cref="double"/> value.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <param name="defaultValue">The default value for the option.</param>
    /// <param name="description">[Optional] The friendly description for the option.</param>
    /// <param name="minValue">[Optional] The smallest value allowed for the option.</param>
    /// <param name="maxValue">[Optional] The largest value allowed for the option.</param>
    /// <returns>A new <see cref="GorgonRangedOption{T}"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// A value assigned to the option is clamped to the <paramref name="minValue"/> and the <paramref name="maxValue"/>, and this includes the <paramref name="defaultValue"/>. The 
    /// <see cref="IGorgonOption.Text"/> for the option is the first line of the <paramref name="description"/>.
    /// </para>
    /// <para>
    /// The default values are an empty string for the <paramref name="description"/>, and no limit for the <paramref name="minValue"/> and the <paramref name="maxValue"/>.
    /// </para>
    /// </remarks>
    public static GorgonRangedOption<double> CreateDoubleOption(string name, double defaultValue, string? description = null, double? minValue = null, double? maxValue = null) =>
        new(name, defaultValue, description, minValue, maxValue);

    /// <summary>
    /// Function to create an option that stores a <see cref="decimal"/> value.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <param name="defaultValue">The default value for the option.</param>
    /// <param name="description">[Optional] The friendly description for the option.</param>
    /// <param name="minValue">[Optional] The smallest value allowed for the option.</param>
    /// <param name="maxValue">[Optional] The largest value allowed for the option.</param>
    /// <returns>A new <see cref="GorgonRangedOption{T}"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// A value assigned to the option is clamped to the <paramref name="minValue"/> and the <paramref name="maxValue"/>, and this includes the <paramref name="defaultValue"/>. The 
    /// <see cref="IGorgonOption.Text"/> for the option is the first line of the <paramref name="description"/>.
    /// </para>
    /// <para>
    /// The default values are an empty string for the <paramref name="description"/>, and no limit for the <paramref name="minValue"/> and the <paramref name="maxValue"/>.
    /// </para>
    /// </remarks>
    public static GorgonRangedOption<decimal> CreateDecimalOption(string name, decimal defaultValue, string? description = null, decimal? minValue = null, decimal? maxValue = null) =>
        new(name, defaultValue, description, minValue, maxValue);

    /// <summary>
    /// Function to create an option that stores a <see cref="DateTime"/> value.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <param name="defaultValue">The default value for the option.</param>
    /// <param name="description">[Optional] The friendly description for the option.</param>
    /// <param name="minValue">[Optional] The smallest value allowed for the option.</param>
    /// <param name="maxValue">[Optional] The largest value allowed for the option.</param>
    /// <returns>A new <see cref="GorgonRangedOption{T}"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// A value assigned to the option is clamped to the <paramref name="minValue"/> and the <paramref name="maxValue"/>, and this includes the <paramref name="defaultValue"/>. The 
    /// <see cref="IGorgonOption.Text"/> for the option is the first line of the <paramref name="description"/>.
    /// </para>
    /// <para>
    /// The default values are an empty string for the <paramref name="description"/>, and no limit for the <paramref name="minValue"/> and the <paramref name="maxValue"/>.
    /// </para>
    /// </remarks>
    public static GorgonRangedOption<DateTime> CreateDateTimeOption(string name, DateTime defaultValue, string? description = null, DateTime? minValue = null, DateTime? maxValue = null) =>
        new(name, defaultValue, description, minValue, maxValue);

    /// <summary>
    /// Function to create an option that stores a value of any type.
    /// </summary>
    /// <typeparam name="T">The type of value to store.</typeparam>
    /// <param name="name">The name of the option.</param>
    /// <param name="defaultValue">[Optional] The default value for the option.</param>
    /// <param name="description">[Optional] The friendly description for the option.</param>
    /// <returns>A new <see cref="GorgonOption{T}"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// The <see cref="IGorgonOption.Text"/> for the option is the first line of the <paramref name="description"/>.
    /// </para>
    /// <para>
    /// The default values are the default for <typeparamref name="T"/> for the <paramref name="defaultValue"/>, and an empty string for the <paramref name="description"/>.
    /// </para>
    /// </remarks>
    public static GorgonOption<T> CreateOption<T>(string name, T? defaultValue = default, string? description = null) => new(name, defaultValue, description);

    /// <summary>
    /// Function to create an option that stores a value of any type, with a value that differs from its default.
    /// </summary>
    /// <typeparam name="T">The type of value to store.</typeparam>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The initial value for the option.</param>
    /// <param name="defaultValue">The default value for the option.</param>
    /// <param name="description">[Optional] The friendly description for the option.</param>
    /// <returns>A new <see cref="GorgonOption{T}"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// The <see cref="IGorgonOption.Text"/> for the option is the first line of the <paramref name="description"/>.
    /// </para>
    /// <para>
    /// The default value is an empty string for the <paramref name="description"/>.
    /// </para>
    /// </remarks>
    public static GorgonOption<T> CreateOption<T>(string name, T? value, T? defaultValue, string? description = null) => new(name, defaultValue, description)
    {
        Value = value
    };
}

/// <summary>
/// An option that stores a value of a specific type in an <see cref="IGorgonOptionBag"/>.
/// </summary>
/// <typeparam name="T">The type of value stored in the option.</typeparam>
/// <remarks>
/// <para>
/// The option stores its value as <typeparamref name="T"/>, so code that knows the type of the option can read and write the <see cref="Value"/> directly, without any conversion. Code that only has access to 
/// the option bag (for example, a user interface that lists the options) uses the <see cref="IGorgonOption"/> methods instead, which convert the value to and from the type requested.
/// </para>
/// <para>
/// To limit the value of an option to a minimum and maximum, use a <see cref="GorgonRangedOption{T}"/>.
/// </para>
/// <para>
/// <note type="important">
/// <para>
/// When <typeparamref name="T"/> is a reference type, the option stores a reference to the object, and the <see cref="Value"/> starts out as the same instance as the <see cref="DefaultValue"/>. Use an 
/// immutable or read-only type (e.g. <see cref="IReadOnlyList{T}"/>) so that changes to the value cannot alter the default.
/// </para>
/// </note>
/// </para>
/// </remarks>
/// <seealso cref="GorgonRangedOption{T}"/>
/// <seealso cref="GorgonOptionBag"/>
public class GorgonOption<T>
    : IGorgonOption
{
    // The value stored in this option.
    private T? _value;

    /// <inheritdoc/>
    public Type Type => typeof(T);

    /// <inheritdoc/>
    public string Text
    {
        get;
    }

    /// <inheritdoc/>
    public string Description
    {
        get;
    }

    /// <inheritdoc/>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Property to return the default value for this option.
    /// </summary>
    public T? DefaultValue
    {
        get;
    }

    /// <summary>
    /// Property to set or return the value for this option.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is the <see cref="DefaultValue"/>.
    /// </para>
    /// </remarks>
    public T? Value
    {
        get => _value;
        set => _value = OnClampValue(value);
    }

    /// <summary>
    /// Function to convert a stored value to the type requested by a caller.
    /// </summary>
    /// <typeparam name="TValue">The type requested by the caller.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value.</returns>
    private protected static TValue? ConvertValue<TValue>(T? value) => value switch
    {
        TValue typedValue => typedValue,
        null => default,
        _ => (TValue?)Convert.ChangeType(value, typeof(TValue))
    };

    /// <summary>
    /// Function to restrict a value before it is assigned to the <see cref="Value"/>.
    /// </summary>
    /// <param name="value">The value being assigned.</param>
    /// <returns>The value to store.</returns>
    /// <remarks>
    /// <para>
    /// Implementors override this to limit the values that the option accepts. The base implementation returns the <paramref name="value"/> unchanged.
    /// </para>
    /// </remarks>
    protected virtual T? OnClampValue(T? value) => value;

    /// <inheritdoc/>
    public TValue? GetValue<TValue>() => ConvertValue<TValue>(_value);

    /// <inheritdoc/>
    public void SetValue<TValue>(TValue? value) => Value = value switch
    {
        T typedValue => typedValue,
        null => default,
        _ => (T?)Convert.ChangeType(value, typeof(T))
    };

    /// <inheritdoc/>
    public TValue? GetDefaultValue<TValue>() => ConvertValue<TValue>(DefaultValue);

    /// <inheritdoc/>
    public virtual TValue? GetMinValue<TValue>() => default;

    /// <inheritdoc/>
    public virtual TValue? GetMaxValue<TValue>() => default;

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonOption{T}"/> class.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <param name="defaultValue">[Optional] The default value for the option.</param>
    /// <param name="description">[Optional] The friendly description for the option.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// The <see cref="Text"/> for the option is the first line of the <paramref name="description"/>. If the <paramref name="description"/> is a single line, then the <see cref="Text"/> and the 
    /// <see cref="Description"/> are the same.
    /// </para>
    /// <para>
    /// The default values are the default for <typeparamref name="T"/> for the <paramref name="defaultValue"/>, and an empty string for the <paramref name="description"/>.
    /// </para>
    /// </remarks>
    public GorgonOption(string name, T? defaultValue = default, string? description = null)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Description = description ?? string.Empty;

        int newLineIndex = Description.IndexOf('\n');

        Text = (newLineIndex != -1) && (newLineIndex != Description.Length - 1) ? Description[..newLineIndex].TrimEnd('\r') : Description;

        DefaultValue = defaultValue;
        _value = defaultValue;
    }
}
