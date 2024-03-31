
// 
// Gorgon
// Copyright (C) 2017 Michael Winsor
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
// Created: April 26, 2017 12:46:08 PM
// 

using System.Runtime.CompilerServices;
using Gorgon.Properties;

namespace Gorgon.Core;

/// <summary>
/// An exception that should be thrown when a non-null parameter (e.g. <see cref="string"/>) requires a value, and does not.  
/// </summary>
public class ArgumentEmptyException
    : Exception
{
    /// <summary>
    /// Function to throw the appropriate exception if the string is <b>null</b>, empty or contains only whitespace.
    /// </summary>
    /// <param name="value">The value to evaluate.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="value"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="value"/> parameter is empty or only has whitespace.</exception>
    public static void ThrowIfNullOrWhiteSpace(string value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value is null)
        {
            throw new ArgumentNullException(parameterName);
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentEmptyException(parameterName);
        }
    }

    /// <summary>
    /// Function to throw the appropriate exception if the string is <b>null</b>, empty.
    /// </summary>
    /// <param name="value">The value to evaluate.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="value"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="value"/> parameter is empty.</exception>
    public static void ThrowIfNullOrEmpty(string value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value is null)
        {
            throw new ArgumentNullException(parameterName);
        }

        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentEmptyException(parameterName);
        }
    }

    /// <summary>
    /// Function to throw the appropriate exception if the argument is <b>null</b>, or empty.
    /// </summary>
    /// <typeparam name="T">The type of value in the collection.</typeparam>
    /// <param name="value">The value to evaluate.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="value"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="value"/> parameter is empty.</exception>
    public static void ThrowIfNullOrEmpty<T>(IReadOnlyCollection<T> value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value is null)
        {
            throw new ArgumentNullException(parameterName);
        }

        if (value.Count == 0)
        {
            throw new ArgumentEmptyException(parameterName);
        }
    }

    /// <summary>
    /// Function to throw the appropriate exception if the argument is <b>null</b>, or empty.
    /// </summary>
    /// <typeparam name="TKey">The type of key in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of value in the dictionary.</typeparam>
    /// <param name="value">The value to evaluate.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="value"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="value"/> parameter is empty.</exception>
    public static void ThrowIfNullOrEmpty<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value is null)
        {
            throw new ArgumentNullException(parameterName);
        }

        if (value.Count == 0)
        {
            throw new ArgumentEmptyException(parameterName);
        }
    }

    /// <summary>
    /// Function to throw the appropraite exception if the span is empty.
    /// </summary>
    /// <typeparam name="T">The type of value in the span.</typeparam>
    /// <param name="value">The value to evaluate.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="value"/> parameter is empty.</exception>
    public static void ThrowIfEmpty<T>(ReadOnlySpan<T> value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value.IsEmpty)
        {
            throw new ArgumentEmptyException(parameterName);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentEmptyException"/> class.
    /// </summary>
    /// <param name="parameterName">The name of the parameter that caused the exception.</param>
    public ArgumentEmptyException(string? parameterName)
        : base(string.Format(Resources.GOR_ERR_PARAMETER_MUST_NOT_BE_EMPTY, parameterName ?? string.Empty))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentEmptyException"/> class.
    /// </summary>
    /// <param name="parameterName">The name of the parameter that caused the exception.</param>
    /// <param name="innerException">The inner exception for this exception.</param>
    public ArgumentEmptyException(string parameterName, Exception innerException)
        : base(string.Format(Resources.GOR_ERR_PARAMETER_MUST_NOT_BE_EMPTY, parameterName), innerException)
    {
    }
}
