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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 16, 2016 4:40:09 PM
// 

using System;
using Gorgon.Core;

namespace Gorgon.Configuration;

/// <summary>
/// An option stored in an <see cref="IGorgonOptionBag"/>.
/// </summary>
/// <remarks>
/// <para>
/// This is the view of an option used by code that only has the option bag (for example, a user interface that lists the options). The value is read and written through methods that convert it to and from the 
/// type requested by the caller. Use <see cref="GorgonOption"/> to create options.
/// </para>
/// </remarks>
/// <seealso cref="GorgonOption{T}"/>
/// <seealso cref="GorgonRangedOption{T}"/>
public interface IGorgonOption
    : IGorgonNamedObject
{
    /// <summary>
    /// Property to return the type of the value stored in the option.
    /// </summary>
    Type Type
    {
        get;
    }

    /// <summary>
    /// Property to return the text to display for this option.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the first line of the <see cref="Description"/>.
    /// </para>
    /// </remarks>
    public string Text
    {
        get;
    }

    /// <summary>
    /// Property to return the friendly description of this option.
    /// </summary>
    string Description
    {
        get;
    }

    /// <summary>
    /// Function to retrieve the value stored in this option.
    /// </summary>
    /// <typeparam name="T">The type to return the value as.</typeparam>
    /// <returns>The value, converted to <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to <typeparamref name="T"/>.</exception>
    /// <exception cref="OverflowException">Thrown when the value is outside of the range of <typeparamref name="T"/>.</exception>
    /// <remarks>
    /// <para>
    /// If the value is already a <typeparamref name="T"/>, then it is returned as is. Otherwise, it is converted with <see cref="Convert.ChangeType(object, Type)"/>, which requires a type that implements 
    /// <see cref="IConvertible"/> (e.g. the numeric types, <see cref="DateTime"/>, enumerations, and <see cref="string"/>).
    /// </para>
    /// </remarks>
    T? GetValue<T>();

    /// <summary>
    /// Function to assign a value for the option.
    /// </summary>
    /// <typeparam name="T">The type of the value being assigned.</typeparam>
    /// <param name="value">The value to assign.</param>
    /// <exception cref="InvalidCastException">Thrown when the <paramref name="value"/> cannot be converted to the <see cref="Type"/> of the option.</exception>
    /// <exception cref="OverflowException">Thrown when the <paramref name="value"/> is outside of the range of the <see cref="Type"/> of the option.</exception>
    /// <remarks>
    /// <para>
    /// If the <paramref name="value"/> is not already the <see cref="Type"/> of the option, then it is converted with <see cref="Convert.ChangeType(object, Type)"/>. An option that limits its value to a range 
    /// clamps the converted value to that range.
    /// </para>
    /// </remarks>
    void SetValue<T>(T? value);

    /// <summary>
    /// Function to retrieve the default value for this option.
    /// </summary>
    /// <typeparam name="T">The type to return the value as.</typeparam>
    /// <returns>The default value, converted to <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to <typeparamref name="T"/>.</exception>
    /// <exception cref="OverflowException">Thrown when the value is outside of the range of <typeparamref name="T"/>.</exception>
    /// <inheritdoc cref="GetValue{T}" path="/remarks"/>
    T? GetDefaultValue<T>();

    /// <summary>
    /// Function to retrieve the smallest value allowed for this option.
    /// </summary>
    /// <typeparam name="T">The type to return the value as.</typeparam>
    /// <returns>The smallest value allowed, converted to <typeparamref name="T"/>, or the default for <typeparamref name="T"/> if the option has no lower limit.</returns>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to <typeparamref name="T"/>.</exception>
    /// <exception cref="OverflowException">Thrown when the value is outside of the range of <typeparamref name="T"/>.</exception>
    /// <inheritdoc cref="GetValue{T}" path="/remarks"/>
    T? GetMinValue<T>();

    /// <summary>
    /// Function to retrieve the largest value allowed for this option.
    /// </summary>
    /// <typeparam name="T">The type to return the value as.</typeparam>
    /// <returns>The largest value allowed, converted to <typeparamref name="T"/>, or the default for <typeparamref name="T"/> if the option has no upper limit.</returns>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to <typeparamref name="T"/>.</exception>
    /// <exception cref="OverflowException">Thrown when the value is outside of the range of <typeparamref name="T"/>.</exception>
    /// <inheritdoc cref="GetValue{T}" path="/remarks"/>
    T? GetMaxValue<T>();
}
