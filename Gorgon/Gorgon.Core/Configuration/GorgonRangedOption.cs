//
// Gorgon
// Copyright (C) 2026 Michael Winsor
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
// Created: October 4, 2026 8:20:00 PM
//

using System;
using Gorgon.Core;

namespace Gorgon.Configuration;

/// <summary>
/// An option that limits its value to a range, and stores it in an <see cref="IGorgonOptionBag"/>.
/// </summary>
/// <typeparam name="T">The type of value stored in the option.</typeparam>
/// <remarks>
/// <para>
/// The option keeps its value between the <see cref="MinValue"/> and the <see cref="MaxValue"/>. A value outside of the range is clamped to the nearest limit rather than rejected, and this includes the 
/// <see cref="GorgonOption{T}.DefaultValue"/>.
/// </para>
/// </remarks>
/// <seealso cref="GorgonOption{T}"/>
/// <seealso cref="GorgonOptionBag"/>
/// <param name="name">The name of the option.</param>
/// <param name="defaultValue">The default value for the option.</param>
/// <param name="description">[Optional] The friendly description for the option.</param>
/// <param name="minValue">[Optional] The smallest value allowed for the option.</param>
/// <param name="maxValue">[Optional] The largest value allowed for the option.</param>
/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
/// <remarks>
/// <para>
/// The <see cref="GorgonOption{T}.Text"/> for the option is the first line of the <paramref name="description"/>. If the <paramref name="description"/> is a single line, then the 
/// <see cref="GorgonOption{T}.Text"/> and the <see cref="GorgonOption{T}.Description"/> are the same.
/// </para>
/// <para>
/// The default values are an empty string for the <paramref name="description"/>, and no limit for the <paramref name="minValue"/> and the <paramref name="maxValue"/>.
/// </para>
/// </remarks>
public sealed class GorgonRangedOption<T>(string name, T defaultValue, string? description = null, T? minValue = null, T? maxValue = null)
    : GorgonOption<T>(name, Clamp(defaultValue, minValue, maxValue), description)
    where T : struct, IComparable<T>
{
    /// <summary>
    /// Property to return the smallest value allowed for this option.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If this value is <b>null</b>, then the option has no lower limit.
    /// </para>
    /// </remarks>
    public T? MinValue
    {
        get;
    } = minValue;

    /// <summary>
    /// Property to return the largest value allowed for this option.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If this value is <b>null</b>, then the option has no upper limit.
    /// </para>
    /// </remarks>
    public T? MaxValue
    {
        get;
    } = maxValue;

    /// <summary>
    /// Function to clamp a value to a range.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="minValue">The lower limit, or <b>null</b> for no lower limit.</param>
    /// <param name="maxValue">The upper limit, or <b>null</b> for no upper limit.</param>
    /// <returns>The clamped value.</returns>
    private static T Clamp(T value, T? minValue, T? maxValue)
    {
        if ((minValue is not null) && (value.CompareTo(minValue.Value) < 0))
        {
            return minValue.Value;
        }

        if ((maxValue is not null) && (value.CompareTo(maxValue.Value) > 0))
        {
            return maxValue.Value;
        }

        return value;
    }

    /// <inheritdoc/>
    protected override T OnClampValue(T value) => Clamp(value, MinValue, MaxValue);

    /// <inheritdoc/>
    public override TValue? GetMinValue<TValue>()
        where TValue : default => MinValue is null ? default : ConvertValue<TValue>(MinValue.Value);

    /// <inheritdoc/>
    public override TValue? GetMaxValue<TValue>()
        where TValue : default => MaxValue is null ? default : ConvertValue<TValue>(MaxValue.Value);
}
